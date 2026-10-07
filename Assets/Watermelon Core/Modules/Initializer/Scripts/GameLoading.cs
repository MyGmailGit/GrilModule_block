using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using FirebaseRemote;
using System.Threading;
using DG.Tweening;
using System.Globalization;
using GameBase;

namespace Watermelon
{
    [StaticUnload]
    public class GameLoading : MonoBehaviour
    {
        public const string IsNewUser = "new_user";
        // 为了避免加载页一闪而过，至少显示这么久。
        private const float MINIMUM_LOADING_TIME = 2.0f;

        private static GameLoading gameLoading;

        // 场景里的初始化器，负责启动各个 InitModule 和 SDK。
        [SerializeField] Initializer initializer;
        // 加载界面的表现层，负责进度条、文案、错误提示等。
        [SerializeField] LoadingGraphics loadingGraphics;

        [Space]
        [Tooltip("If manual mode is enabled, the loading screen will be active until GameLoading.MarkAsReadyToHide method has been called.")]
        // 开启后，即使目标场景已经加载完成，也要等外部显式调用 MarkAsReadyToHide 才会关闭加载页。
        [SerializeField] bool useManualControl;
        // 是否在正式初始化前先做一次联网检测。
        [SerializeField] bool checkNetworkConnection = true;

        // 可选模块。若存在，则会在进入游戏前先拉远程配置。
        private RemoteConfigHandler remoteConfigHandler;

        private ServerRemoteMgr serverRemoteCtrl;

        // Unity 异步场景加载句柄，用来读取加载进度。
        private static AsyncOperation loadingOperation;

        // 手动控制模式下，外部通过该标记通知加载页可以结束了。
        private static bool isReadyToHide;

        // 当前加载文案，支持外部动态修改。
        private static string loadingMessage;
        // 允许外部在切场景前挂接额外启动任务，按顺序执行。
        private static List<LoadingTask> loadingTasks = new List<LoadingTask>();

        // 当前初始化协程，用于失败后支持重试。
        private Coroutine initCoroutine;

        // 指定要跳转的目标场景；若为 -1，则默认加载 Build Settings 中下一个场景。
        public static int LoadingSceneBuildIndex = -1;

        private void Awake()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            gameLoading = this;

            // 加载器需要跨场景存活，直到真正完成收尾才销毁。
            DontDestroyOnLoad(gameObject);

            // 这个远程配置结构设计属于自己服务器模式，没法用，只能使用firebase
            // remoteConfigHandler = initializer.GetComponent<RemoteConfigHandler>();
            serverRemoteCtrl = ServerRemoteMgr.Create();//gameObject.AddComponent<ServerRemoteMgr>();
            // 先初始化基础系统和加载界面，再开始完整的加载流程。
            initializer.Init();
            loadingGraphics.Init(this);

            Utility.CoroutineTool.Create();

            initCoroutine = StartCoroutine(ConnectionCheckCoroutine());
            // 如果没有newuser这个key说明是newuser
            if (!PlayerPrefs.HasKey(IsNewUser))
            {
                PlayerPrefs.SetInt(IsNewUser, 1);
            }
            else if (PlayerPrefs.GetInt(IsNewUser, 1) == 1)
            {
                // 如果是newuser 就设置为不是newuser
                PlayerPrefs.SetInt(IsNewUser, 0);
            }
        }

        public void RetryConnection()
        {
            // 只有在之前协程已经结束的情况下才允许重试。
            if (initCoroutine == null)
            {
                initCoroutine = StartCoroutine(ConnectionCheckCoroutine());
            }
        }

        private IEnumerator ConnectionCheckCoroutine()
        {
            // 每次进入流程前，先清掉上一次的错误 UI。
            loadingGraphics.HideErrorMessage();
            loadingGraphics.SetLoadingState(0.0f, "Checking connection..");

            NetTimeMgr.Create();

            // if (checkNetworkConnection)
            // {
            //     bool isConnected = false;

            //     // 通过一个外部地址做联网可用性检测。
            //     NetworkConnection networkConnection = new NetworkConnection("https://google.com/");
            //     IEnumerator connectionCheck = networkConnection.CheckConnection((state) => isConnected = state);

            //     yield return connectionCheck;

            //     if (!isConnected)
            //     {
            //         // 联网失败时直接停在加载页，等待用户点击重试。
            //         loadingGraphics.ShowErrorMessage("Connection error");

            //         initCoroutine = null;

            //         yield break;
            //     }
            // }
            {
                bool? isConnected = null;

                // 检查联网不如直接获取AB test 云函数数据 ，firebase的forceA取消了
                NetworkCheckAbConfig networkConnection = new();

                StartCoroutine(networkConnection.CheckConnection((state) => isConnected = state));
                loadingGraphics.SetLoadingState(0.3f, "Checking connection..");
                // 等个1s，因为后面还有adjust的等待

                var abConfigStartTime = Time.realtimeSinceStartup + 6.0f;
                while (isConnected == null && Time.realtimeSinceStartup < abConfigStartTime)
                {
                    yield return null;
                }
            }

            if (remoteConfigHandler != null)
            {
                bool isConfigLoaded = false;

                // 若项目启用了远程配置，则在切场景前先把配置拉下来。
                loadingGraphics.SetLoadingState(0.3f, "Loading Data..");

                IEnumerator configLoad = remoteConfigHandler.LoadConfig((state) => isConfigLoaded = state);

                yield return configLoad;

                if (!isConfigLoaded)
                {
                    // 远程配置失败同样停留在加载页，由外部决定是否重试。
                    loadingGraphics.ShowErrorMessage("Failed to load data");

                    initCoroutine = null;

                    yield break;
                }
            }
            else
            {
                loadingGraphics.SetLoadingState(0.3f, "Loading..");
            }


#if !UNITY_EDITOR
            yield return null;
            yield return null;
            var timeSinceStartup = Time.unscaledTime + 4.0f;// + UnityEngine.Random.Range(3, 4);
            while (!AdjustAnalyticsModule.IsGetAdjustData && Time.realtimeSinceStartup < timeSinceStartup)
            {
                yield return null;
            }
#endif
            Debug.Log($"[GameLoading] Wait for Adjust Data: {AdjustAnalyticsModule.IsGetAdjustData}, Time: {Time.realtimeSinceStartup}");

            // 前置检查完成后，开始初始化业务模块和第三方 SDK。
            initializer.InitModules();
            initializer.InitSDKs();

            // 顺序执行外部注册的启动任务，例如预热、缓存构建等。
            int taskIndex = 0;
            while (taskIndex < loadingTasks.Count)
            {
                if (!loadingTasks[taskIndex].IsActive)
                    loadingTasks[taskIndex].Activate();

                if (loadingTasks[taskIndex].IsFinished)
                {
                    taskIndex++;
                }

                yield return null;
            }

            float realtimeSinceStartup = Time.realtimeSinceStartup;

            int sceneIndex = LoadingSceneBuildIndex;
            if (sceneIndex == -1)
            {
                // 未显式指定目标场景时，默认跳到 Build Settings 的下一个场景。
                sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (SceneManager.sceneCount < sceneIndex)
                    Debug.LogError("[Loading]: First scene is missing!");
            }

            // 保证加载页至少显示一段时间，避免切换过快造成闪屏。
            float minimumFinishTime = realtimeSinceStartup + MINIMUM_LOADING_TIME;

            loadingOperation = SceneManager.LoadSceneAsync(sceneIndex);

            loadingMessage = "Loading..";

            // 同时等待两个条件：
            // 1. 目标场景异步加载完成
            // 2. 最短显示时间已到
            while (!loadingOperation.isDone || realtimeSinceStartup < minimumFinishTime)
            {
                yield return null;

                realtimeSinceStartup = Time.realtimeSinceStartup;

                // 进度条只映射到 0.2 ~ 0.9，剩余部分留给最终完成动画。
                loadingGraphics.SetLoadingState(Mathf.Lerp(0.3f, 0.9f, loadingOperation.progress), loadingMessage);
            }

            loadingGraphics.SetLoadingState(1f, loadingMessage);

            if (useManualControl)
            {
                // 手动模式下，如果外部忘了调用 MarkAsReadyToHide，这里会给出调试提示。
                DOVirtual.DelayedCall(10, () =>
                {
                    if (!isReadyToHide)
                        Debug.LogError("[Loading]: Seems like you forget to call MarkAsReadyToHide method to finish the loading process.");
                });

                // 等待业务侧确认“加载页现在可以安全关闭”。
                while (!isReadyToHide)
                {
                    yield return null;
                }
            }

            // 等待adjust 返回数据
            loadingGraphics.SetLoadingState(1f, "Completed");

            // 通知加载界面播放收尾表现，然后销毁自己。
            loadingGraphics.OnLoadingFinished();

            Destroy(gameObject);
        }

        public static void SetLoadingMessage(string message)
        {
            // 允许外部在加载过程中更新提示文案。
            loadingMessage = message;

            float progress = 0.0f;
            if (loadingOperation != null)
                progress = loadingOperation.progress;
        }

        public static void AddTask(LoadingTask loadingTask)
        {
            // 在正式切场景前追加一个启动任务。
            loadingTasks.Add(loadingTask);
        }

        public static void MarkAsReadyToHide()
        {
            // 手动模式下，外部通过这个入口通知加载页可以关闭。
            isReadyToHide = true;
        }

        private static void UnloadStatic()
        {
            // 静态清理，避免下次进入时沿用旧状态。
            isReadyToHide = false;
            loadingTasks.Clear();
        }

        public delegate void LoadingCallback(float state, string message);
    }
}

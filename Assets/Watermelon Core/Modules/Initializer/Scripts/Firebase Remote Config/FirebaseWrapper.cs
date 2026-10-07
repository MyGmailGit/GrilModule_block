using System;
using System.Collections;
using System.Threading.Tasks;
using AdjustSdk;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
using Firebase.RemoteConfig;
using UnityEngine;
using UnityEngine.Android;

namespace FirebaseRemote
{
    public class FirebaseWrapper : MonoBehaviour
    {
        public event Action remoteGetAction = null;

        void Start()
        {
            // var status = await FirebaseApp.CheckAndFixDependenciesAsync();
            // if (status == DependencyStatus.Available)
            // {
            //     // 拉取服务器配置
            //     await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);

            //     // 激活配置
            //     await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            //     remoteGetAction?.Invoke();

            //     FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += ConfigUpdateListenerEventHandler;

            //     Debug.Log("Remote Config Ready");
            //     // notifacation

            //     Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            //     Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

            //     await FirebaseMessaging.SubscribeAsync("global");

            //     await Task.Delay(6000);
            //     await FirebaseMessaging.RequestPermissionAsync();
            // }
            // else
            // {
            //     UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", status));
            // }

            // InitFirebase().ContinueWithOnMainThread(task => { });

            StartCoroutine(IEInitFirebase());
        }

        public IEnumerator IEInitFirebase()
        {

            System.Collections.Generic.Dictionary<string, object> defaults = new System.Collections.Generic.Dictionary<string, object>();

            defaults.Add(RemoteKey.InterTimeDelay, 60);
            defaults.Add(RemoteKey.IFForceToA, true);
            defaults.Add(RemoteKey.InterAdMinLevel, 5);

            var defValueTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
            yield return new WaitUntil(() => defValueTask.IsCompleted);

            var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

            yield return new WaitUntil(() => dependencyTask.IsCompleted);

            if (dependencyTask.Exception != null)
            {
                Debug.LogError($"[FirebaseWrapper] Firebase dependency error: {dependencyTask.Exception}");
                yield break;
            }

            var status = dependencyTask.Result;

            if (status != DependencyStatus.Available)
            {
                Debug.LogError($"[FirebaseWrapper] Could not resolve Firebase dependencies: {status}");
                yield break;
            }

            // Remote Config
            var fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);

            yield return new WaitUntil(() => fetchTask.IsCompleted);

            var activateTask = FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

            yield return new WaitUntil(() => activateTask.IsCompleted);

            remoteGetAction?.Invoke();

            FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += ConfigUpdateListenerEventHandler;

            Debug.Log("[FirebaseWrapper] Remote Config Ready");

            // Messaging
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            var subscribeTask = FirebaseMessaging.SubscribeAsync("global");

            yield return new WaitUntil(() => subscribeTask.IsCompleted);


            // 取消自动获取 改成点击play按钮显示
            /*
            // 延迟6秒
            yield return new WaitForSeconds(6f);

#if UNITY_ANDROID
            UnityEngine.Debug.Log("[FirebaseWrapper] Request POST_NOTIFICATIONS ing");
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
#endif
            var permissionTask = FirebaseMessaging.RequestPermissionAsync();

            yield return new WaitUntil(() => permissionTask.IsCompleted);
            */

            Debug.Log("[FirebaseWrapper] Firebase init complete");
        }

        public static void CheckNoticfacation()
        {

            UnityEngine.Debug.Log("[FirebaseWrapper] Request POST_NOTIFICATIONS ing");
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
#endif
#if !UNITY_EDITOR
            var permissionTask = FirebaseMessaging.RequestPermissionAsync();
#endif
        }

        // public async Task InitFirebase()
        // {
        //     try
        //     {
        //         var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        //         if (status == DependencyStatus.Available)
        //         {
        //             // 拉取服务器配置
        //             await FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero);

        //             // 激活配置
        //             await FirebaseRemoteConfig.DefaultInstance.ActivateAsync();

        //             remoteGetAction?.Invoke();

        //             FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener += ConfigUpdateListenerEventHandler;

        //             Debug.Log("Remote Config Ready");
        //             // notifacation

        //             Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        //             Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

        //             await FirebaseMessaging.SubscribeAsync("global");

        //             // 取消自动获取 改成点击play按钮显示
        //             // await Task.Delay(6000);
        //             // await RequestNotifacation();//FirebaseMessaging.RequestPermissionAsync();
        //         }
        //         else
        //         {
        //             UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", status));
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogError($"[FirebaseWrapper] init Exception:{e}");
        //     }
        // }

        public async Task RequestNotifacation()
        {
#if UNITY_ANDROID
            UnityEngine.Debug.Log("[FirebaseWrapper] Request POST_NOTIFICATIONS ing");
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
            }
#endif

            await FirebaseMessaging.RequestPermissionAsync();
        }

        // Stop the listener.
        void OnDestroy()
        {
            Debug.Log("[FirebaseWrapper] Firebase OnDestroy");
            FirebaseRemoteConfig.DefaultInstance.OnConfigUpdateListener -= ConfigUpdateListenerEventHandler;
        }

        // Handle real-time Remote Config events.
        void ConfigUpdateListenerEventHandler(object sender, Firebase.RemoteConfig.ConfigUpdateEventArgs args)
        {
            if (args.Error != Firebase.RemoteConfig.RemoteConfigError.None)
            {
                Debug.Log(String.Format("Error occurred while listening: {0}", args.Error));
                return;
            }

#if TEST_MODE
            Debug.Log("[FirebaseWrapper] Updated keys: " + string.Join(", ", args.UpdatedKeys));
#endif

            FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWithOnMainThread(
                task =>
                {
                    remoteGetAction.Invoke();
                });
        }

        private void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
#if TEST_MODE
            UnityEngine.Debug.Log("[FirebaseWrapper] Received Registration Token: " + token.Token);
#endif
            Adjust.SetPushToken(token.Token);
        }

        private void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
#if TEST_MODE
            UnityEngine.Debug.Log("[FirebaseWrapper] Received a new message from: " + e.Message.From);
#endif
        }

    }
}

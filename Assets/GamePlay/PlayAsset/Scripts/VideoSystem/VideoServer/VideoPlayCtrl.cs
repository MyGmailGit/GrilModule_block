using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using VideoSystem;
using UnityEngine.Networking;
using Util;

namespace Game.Video
{
    #region Video Data Model

    // [System.Serializable]
    // public class VideoSystemConfig
    // {
    // public string RemoteBaseUrl = "https://d38ry694xn4cmr.cloudfront.net/GameBlockJam1/";

    // }

    #endregion

    #region Video Play Controller (Non-Singleton)

    public class VideoPlayCtrl : MonoBehaviour
    {
        private static string isEnterVideo = null;
        private enum DownloadWaitState
        {
            None,
            Waitting,
            Complete,
            Fail,
        }
        private const string LOG_TAG = "[VideoPlayCtrl]";

        public event Action<string> OnVideoStarted;      // fileNameId
        public event Action OnVideoStopped;
        public event Action<string> OnVideoCompleted;    // fileNameId
        public event Action<string, string> OnVideoError; // fileNameId, error message


        private string LocalFolderPath = ServerUtil.MENU_VIDEO_UNITY_FOLDER_PATH;//"Assets/StreamingAssets/MenuBackgroundVideos";
                                                                                 // public string CacheFolderName = ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME;
        public string TempDecryptFolderName { get; private set; } = "local.82f3f81d948044806972b405c182df33";
        public string FileExtension { get; private set; } = ServerUtil.MENU_VIDEO_FILE_EXTENSION;
        // public float DownloadTimeout = 5.0f;
        // public bool LoopVideo { get; private set; } = true;
        // public int DefaultfileNameId { get; private set; } = 1;
        // public int MaxMappedfileNameId { get; private set; } = 5;  // 只有5个本地视频映射

        // [SerializeField] private VideoSystemConfig config;

        private VideoPlayer videoPlayer;
        private RawImage displayTarget;
        private RenderTexture videoRenderTexture;
        private string currentfileNameId = null;
        private string targetfileNameId = null;
        private string currentDecryptedFilePath;  // 当前解密的临时文件路径
        private bool isPrepared;
        private Coroutine currentLoadCoroutine;
        private Coroutine fallbackRetryCoroutine;

        private DownloadWaitState isWaitForDownload = DownloadWaitState.None;

        // private IMGBackgroundManager iMGBackgroundManager = null;

        private Color colorHide = new Color(1, 1, 1, 0);
        private Color colorShow = new Color(1, 1, 1, 1);

        // Public properties
        // public bool IsPlaying => videoPlayer != null && videoPlayer.isPlaying;
        public bool IsPrepared => isPrepared;
        public string CurrentfileNameId => currentfileNameId;


        public bool iaAutoPause { get; set; } = false;




        private void Awake()
        {
            // if (config == null)
            // {
            //     config = new VideoSystemConfig();
            // }

            InitializeVideoPlayer();
            EnsureDirectories();
        }
        private void Start()
        {
            // 订阅事件
            // EventBus.Subscribe<VideoDownloadCompleteEvent>(OnVideoDownloadComplete);
            // EventBus.Subscribe<VideoDownloadStartEvent>(OnVideoDownloadStart);
            // EventBus.Subscribe<VideoDownloadAllServersFailedEvent>(OnVideoDownloadFail);
        }

        private void OnDestroy()
        {
            Dispose();
            // 取消订阅（重要！避免内存泄漏）
            // EventBus.Unsubscribe<VideoDownloadCompleteEvent>(OnVideoDownloadComplete);
            // EventBus.Unsubscribe<VideoDownloadStartEvent>(OnVideoDownloadStart);
            // EventBus.Unsubscribe<VideoDownloadAllServersFailedEvent>(OnVideoDownloadFail);

        }

        private void InitializeVideoPlayer()
        {

            videoRenderTexture = new RenderTexture(1080, 1920, 24, RenderTextureFormat.ARGB32);
            videoRenderTexture.Create();

            GameObject videoObject = new GameObject("VideoPlayer", typeof(VideoPlayer));
            videoObject.transform.SetParent(transform, false);
            videoPlayer = videoObject.GetComponent<VideoPlayer>();

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.skipOnDrop = true;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = videoRenderTexture;
            videoPlayer.aspectRatio = VideoAspectRatio.FitOutside;
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.timeUpdateMode = VideoTimeUpdateMode.DSPTime;

            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioMute(0, false);
            videoPlayer.SetDirectAudioVolume(0, 1.0f);

        }

        // private void InitializeVideoPlayer()
        // {
        //     // 1. 创建 GameObject 并挂载 MediaPlayer 组件
        //     GameObject videoObject = new GameObject("VideoPlayer", typeof(VideoPlayer_AVPro));
        //     videoObject.transform.SetParent(transform, false);
        //     mediaPlayer = videoObject.GetComponent<VideoPlayer_AVPro>();
        //     videoObject.AddComponent<AudioSource>();
        //     videoObject.AddComponent<AudioOutput>().Player = mediaPlayer;

        //     // 2. 基础播放设置
        //     mediaPlayer.AutoStart = false;        // 不自动播放，对应 playOnAwake = false
        //     mediaPlayer.Loop = true;//LoopVideo; // 是否循环，对应 isLooping

        //     // 3. 音频设置
        //     // AVPro 默认启用所有音频轨道，如果只需第一轨，可以这样显式控制
        //     mediaPlayer.AudioMuted = false;
        //     // 如果有多轨需求，可通过 mediaPlayer.AudioManager 进一步控制

        //     // 4. 画面缩放模式：对应 aspectRatio = FitOutside
        //     mediaPlayer.aspectRatio = ScaleMode.StretchToFill;

        //     // 5. 获取视频纹理 (相当于 VideoPlayer 的 texture 属性)
        //     // 后续需要渲染时，通过 mediaPlayer.TextureProducer.GetTexture() 获取纹理
        //     // 或直接挂载 AVPro Video 的 uGUI 组件来显示

        //     // 6. 事件监听：对应 loopPointReached
        //     mediaPlayer.Events.AddListener(OnPlayerEvent);

        //     // 注意：你原来代码中还有 skipOnDrop、waitForFirstFrame 和音量设置
        //     // - skipOnDrop：AVPro 默认行为就是尽可能不丢帧，无需额外设置
        //     // - waitForFirstFrame：默认会等待首帧，如果遇到问题可以尝试设置 mediaPlayer.m_Persistent = true
        //     // - 音量设置：如果你需要初始音量为 1.0，默认就是 1.0，不需要额外设置
        //     //   如果是 0，可以用 mediaPlayer.m_AudioManager.m_Volume = 0f
        // }

        // private void OnPlayerEvent(MediaPlayer arg0, MediaPlayerEvent.EventType arg1, ErrorCode arg2)
        // {
        //     // 根据事件类型处理
        //     if (arg1 == MediaPlayerEvent.EventType.FinishedPlaying)
        //     {
        //         OnVideoFinished();
        //     }

        //     Debug.Log($"==============={arg1}");
        // }

        private void OnVideoFinished(VideoPlayer source)
        {
            OnVideoCompleted?.Invoke(CurrentfileNameId);
        }

        // private void OnVideoDownloadComplete(VideoDownloadCompleteEvent evt)
        // {
        //     if (evt.fileNameId == targetfileNameId)
        //     {
        //         Debug.Log($"Video download started: {evt.LocalPath}");
        //         isWaitForDownload = DownloadWaitState.Complete;
        //     }
        // }
        // private void OnVideoDownloadFail(VideoDownloadAllServersFailedEvent evt)
        // {
        //     if (evt.fileNameId == targetfileNameId)
        //     {
        //         isWaitForDownload = DownloadWaitState.Fail;
        //     }
        // }

        private void EnsureDirectories()
        {
            // 确保缓存目录存在
            string cacheDirectory = GetCacheDirectory();
            if (!Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            // 确保临时解密目录存在（迷惑性路径）
            string tempDirectory = GetTempDecryptDirectory();
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
        }

        private static string GetCacheDirectory()
        {
            return Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
        }

        private string GetTempDecryptDirectory()
        {
            // 使用迷惑性路径名称
            string pathd = Path.Combine(Application.persistentDataPath, "System", "Cache", TempDecryptFolderName);
            return pathd;
        }

        // private int GetMappedfileNameId(int fileNameId)
        // {
        //     if (fileNameId <= 0)
        //     {
        //         return 1;
        //     }
        //     // 将任意关卡ID映射到1-5范围
        //     if (fileNameId <= MaxMappedfileNameId)
        //         return fileNameId;

        //     // 循环映射：6->1, 7->2, 8->3, 9->4, 10->5, 11->1...
        //     int mappedId = ((fileNameId - 1) % MaxMappedfileNameId) + 1;
        //     return mappedId;
        // }

        private static string GetVideoFileName(string imageId)
        {
            // int mappedId = GetMappedfileNameId(fileNameId);
            // return $"100{fileNameId:00}{config.FileExtension}";
            return ServerUtil.GetVideoFileName(imageId);
        }

        // private byte[] GetEncryptionKey(int fileNameId)
        // {
        //     // string keyString = $"{fileNameId}level{fileNameId}";
        //     return ServerUtil.GetEncryptionKey(fileNameId);//System.Text.Encoding.UTF8.GetBytes(keyString);
        // }

        // private byte[] EncryptOrDecrypt(byte[] data, int fileNameId)
        // {
        //     byte[] key = GetEncryptionKey(fileNameId);
        //     return Utility.CryptoHelper.XorQuick(data, key);
        // }

        /// <summary>
        /// 从StreamingAssets拷贝到临时目录，仅仅用来播放视频
        /// </summary>
        private IEnumerator CopyToTempFromStreamingAssets(string fileNameId, string sourceFilePath)
        {
            Debug.Log($"{LOG_TAG} Copying and decrypting from StreamingAssets for level {fileNameId}");

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android需要使用UnityWebRequest读取
            using (UnityWebRequest request = UnityWebRequest.Get(sourceFilePath))
            {
                yield return request.SendWebRequest();
                
#if UNITY_2020_2_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    Debug.LogError($"{LOG_TAG} Failed to read from StreamingAssets: {request.error}");
                    yield break;
                }
                
                byte[] encryptedData = request.downloadHandler.data;
                // byte[] decryptedData = EncryptOrDecrypt(encryptedData, fileNameId);
                
                // 保存解密的视频到临时目录
                string tempFilePath = GetTempVideoPath(fileNameId);
                File.WriteAllBytes(tempFilePath, encryptedData);
                currentDecryptedFilePath = tempFilePath;
                
                Debug.Log($"{LOG_TAG} Decrypted video saved to: {tempFilePath}");
            }
#else
            // Editor或Standalone可以直接读取文件
            byte[] encryptedData = File.ReadAllBytes(sourceFilePath);
            // byte[] decryptedData = EncryptOrDecrypt(encryptedData, fileNameId);

            string tempFilePath = GetTempVideoPath(fileNameId);
            File.WriteAllBytes(tempFilePath, encryptedData);
            currentDecryptedFilePath = tempFilePath;

            Debug.Log($"{LOG_TAG} Decrypted video saved to: {tempFilePath}");
            yield return null;
#endif
        }

        /// <summary>
        /// 从缓存或下载的视频解密到临时目录
        /// </summary>
        private IEnumerator DecryptFromCache(string fileNameId, string encryptedFilePath)
        {
            Debug.Log($"{LOG_TAG} Decrypting video from cache for level {fileNameId}");

            if (File.Exists(encryptedFilePath))
            {
                byte[] encryptedData = File.ReadAllBytes(encryptedFilePath);
                // byte[] decryptedData = EncryptOrDecrypt(encryptedData, fileNameId);
                byte[] decryptedData = ServerUtil.EncryptOrDecrypt(encryptedData, fileNameId);

                string tempFilePath = GetTempVideoPath(fileNameId);
                File.WriteAllBytes(tempFilePath, decryptedData);
                currentDecryptedFilePath = tempFilePath;

                Debug.Log($"{LOG_TAG} Decrypted video saved to: {tempFilePath}");
                yield return null;
            }
            else
            {
                currentDecryptedFilePath = null;
            }
        }

        private string GetTempVideoPath(string fileNameId)
        {
            string md5 = MD5Util.GetMD5(fileNameId.ToString());
            string tempDir = GetTempDecryptDirectory();
            return Path.Combine(tempDir, $"{md5}_{FileExtension}");
        }

        private static string GetCachedEncryptedPath(string fileNameId)
        {
            string fileName = GetVideoFileName(fileNameId);
            return Path.Combine(GetCacheDirectory(), fileName);
        }

        public void SetDisplayTarget(RawImage target)
        {
            displayTarget = target;
            // if (isPrepared && videoPlayer != null && videoPlayer.texture != null && displayTarget != null)
            if (displayTarget != null && videoRenderTexture != null)
            {
                displayTarget.texture = videoRenderTexture;
                displayTarget.enabled = true;
            }
        }

        /// <summary>
        /// 播放指定关卡的视频
        /// </summary>
        public void PlayLevelVideo(string imageId, RawImage displayTarget = null)
        {
            targetfileNameId = imageId;

            // isWaitForDownload = DownloadWaitState.None;

            // 停止当前加载协程
            if (currentLoadCoroutine != null)
            {
                StopCoroutine(currentLoadCoroutine);
            }

            if (fallbackRetryCoroutine != null)
            {
                StopCoroutine(fallbackRetryCoroutine);
                fallbackRetryCoroutine = null;
            }

            // 设置显示目标
            if (displayTarget != null)
            {
                SetDisplayTarget(displayTarget);
            }

            //             string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic);//"organic");

            //             //event
            //             if (isEnterVideo == null || isEnterVideo != enterType)
            //             {
            //                 isEnterVideo = enterType;
            //                 // 每次进游戏就上传一次事件，后面如果改变再重新传一个
            //                 AnalyticsController.OnAdNetworkVideoChange(enterType);
            //             }

            // #if TEST_MODE
            //             // 如果设置强制走视频
            //             if (DevPanelEnabler.IsDevForceToVideo || FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            //             {
            //                 // 开始加载视频
            //                 currentLoadCoroutine = StartCoroutine(LoadVideoCoroutine(targetfileNameId));
            //                 return;
            //             }
            // #endif
            //             // Debug.Log($"[ReleaseMode] adjust={enterType}, force={FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA()}");
            //             // // if (enterType == AdjustAnalyticsModule.AdjustOrganic || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())//"organic")
            //             // if (string.Equals(enterType, AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
            //             //     || FirebaseRemote.ServerRemoteMgr.Instance.Remote_ForceToA())
            //             if (!FirebaseRemote.ServerRemoteMgr.Instance.GetAB_VideoIsB())
            //             {

            //                 if (iMGBackgroundManager == null)
            //                 {
            //                     iMGBackgroundManager = gameObject.AddComponent<IMGBackgroundManager>();
            //                 }

            //                 iMGBackgroundManager.SetBackgroundByLevel(displayTarget);
            //                 OnVideoStarted?.Invoke(currentfileNameId);
            //             }
            //             else
            {
                // 开始加载视频
                currentLoadCoroutine = StartCoroutine(LoadVideoCoroutine(targetfileNameId));
            }
        }

        private IEnumerator LoadVideoCoroutine(string imageId)
        {
            // 停止当前播放并清理临时文件
            StopAndCleanup();

            Debug.Log($"{LOG_TAG} Loading video for level {imageId}");

            string decryptedVideoPath = null;
            string cachedEncryptedPath = GetCachedEncryptedPath(imageId);
            // 1. 检查是否有已解密的临时文件（来自之前的下载或缓存）
            string tempFile = GetTempVideoPath(imageId);
            if (File.Exists(tempFile))
            {
                decryptedVideoPath = currentDecryptedFilePath = tempFile;
            }
            else if (File.Exists(cachedEncryptedPath))
            {
                // 2. 检查是否有加密的缓存文件
                Debug.Log($"{LOG_TAG} Found encrypted cached video, decrypting...");
                yield return DecryptFromCache(imageId, cachedEncryptedPath);
                decryptedVideoPath = currentDecryptedFilePath;
            }
            else
            {
                // 3. 使用StreamingAssets中的本地视频并解密
                string streamingAssetPath = GetStreamingAssetsPath(GetVideoFileName(imageId));
                Debug.Log($"{LOG_TAG} Found video in StreamingAssets, copying and decrypting...");
                // yield return CopyToTempFromStreamingAssets(fileNameId, streamingAssetPath);
                // decryptedVideoPath = currentDecryptedFilePath;
                yield return SaveEncryptedToCache(imageId, streamingAssetPath);
                yield return DecryptFromCache(imageId, cachedEncryptedPath);
                decryptedVideoPath = currentDecryptedFilePath;

                if (currentDecryptedFilePath == null)
                {
                    // 4. 从远程下载

                    // string fileName = imageId + ".mp4";
                    // string url = VideoServerUrl.BASE_URL + fileName;
                    string url = ServerUtil.MakeMP4DownloadUrl(imageId);

                    PriorityDownloadManager.Instance.AddDownload(url, imageId, NotifyImageDownloaded, (str) =>
                    {
                        isWaitForDownload = DownloadWaitState.Fail;
                        UnityEngine.Debug.Log($"{str}:Image downlaod Fail");
                    });

                    isWaitForDownload = DownloadWaitState.Waitting;
                    while (isWaitForDownload == DownloadWaitState.Waitting)
                    {
                        yield return null;
                    }
                    // 出结果了
                    if (isWaitForDownload == DownloadWaitState.Complete)
                    {
                        yield return DecryptFromCache(imageId, cachedEncryptedPath);
                        decryptedVideoPath = currentDecryptedFilePath;
                    }
                    else if (isWaitForDownload == DownloadWaitState.Fail)
                    {
                        currentLoadCoroutine = null;
                        yield break;
                    }
                }
            }

            // 准备并播放解密的视频
            if (!string.IsNullOrEmpty(decryptedVideoPath) && File.Exists(decryptedVideoPath))
            {
                yield return PrepareAndPlayVideo(decryptedVideoPath, imageId);
            }
            else
            {
                Debug.LogError($"{LOG_TAG} Failed to get decrypted video for level {imageId}");
                OnVideoError?.Invoke(imageId, "Failed to get decrypted video");
            }

            currentLoadCoroutine = null;

            yield return null;
            yield return null;

            // 预加载后续关卡
        }

        /// <summary>
        /// 通知图片下载完成
        /// </summary>
        /// <param name="imageId">图片编号</param>
        /// <param name="texture">下载的纹理（如果下载失败则为null）</param>
        private void NotifyImageDownloaded(string imageId, byte[] textureByte)
        {
            isWaitForDownload = DownloadWaitState.Complete;
            try
            {
                if (textureByte != null)
                {
                    NetVideoDecodeAndSave(imageId, textureByte);
                    // byte[] processedData = Utility.OpenSSLCryptoHelper.DecryptBytes(textureByte, string.Concat(ServerUtil.keyBaseUrl));
                    // if (!ServerUtil.IsVideoFile(processedData))
                    //     throw new Exception($"Process Decode Fail {imageId}");

                    // byte[] fileData = ServerUtil.EncryptOrDecrypt(processedData, imageId);
                    // File.WriteAllBytes(GetCachedEncryptedPath(imageId), fileData);
                }
                else
                {
                    isWaitForDownload = DownloadWaitState.Fail;
                }
            }
            catch (Exception e)
            {
                isWaitForDownload = DownloadWaitState.Fail;
                Debug.LogError($"{imageId}:{e}");
            }
        }

        /// <summary>
        /// 从stream中拷贝，拷贝到缓存目录，方便下次直接从这里解密
        /// </summary>
        /// <param name="fileNameId"></param>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        private IEnumerator SaveEncryptedToCache(string fileNameId, string sourcePath)
        {
            string cachedPath = GetCachedEncryptedPath(fileNameId);

            // 如果缓存文件已存在，跳过
            if (File.Exists(cachedPath))
                yield break;

            Debug.Log($"{LOG_TAG} Saving encrypted video to cache: {cachedPath}");

#if UNITY_ANDROID && !UNITY_EDITOR
            // Android需要使用UnityWebRequest读取
            using (UnityWebRequest request = UnityWebRequest.Get(sourcePath))
            {
                yield return request.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    Debug.LogError($"{LOG_TAG} Failed to read source for caching: {request.error}");
                    yield break;
                }

                // int newfileNameId = CheckLevelFileCanSave(fileNameId);
                cachedPath = GetCachedEncryptedPath(fileNameId);

                byte[] originalData = request.downloadHandler.data;
                var DataTex = Utility.OpenSSLCryptoHelper.DecryptBytes(originalData, string.Concat(ServerUtil.keyBaseUrl));
                // byte[] encryptedData = EncryptOrDecrypt(originalData, fileNameId);
                byte[] encryptedData = ServerUtil.EncryptOrDecrypt(DataTex, fileNameId);
                File.WriteAllBytes(cachedPath, encryptedData);
            }
#else
            // int newfileNameId = CheckLevelFileCanSave(fileNameId);
            cachedPath = GetCachedEncryptedPath(fileNameId);

            if (!File.Exists(sourcePath)) yield break;

            byte[] originalData = File.ReadAllBytes(sourcePath);
            var DataTex = Utility.OpenSSLCryptoHelper.DecryptBytes(originalData, string.Concat(ServerUtil.keyBaseUrl));
            // byte[] encryptedData = EncryptOrDecrypt(originalData, newfileNameId);
            byte[] encryptedData = ServerUtil.EncryptOrDecrypt(DataTex, fileNameId);
            File.WriteAllBytes(cachedPath, encryptedData);
#endif
        }

        // private int CheckLevelFileCanSave(string fileNameId)
        // {
        //     int fileSuffix = 0;
        //     string cachedPath = GetCachedEncryptedPath(fileNameId);
        //     while (File.Exists(cachedPath))
        //     {
        //         fileSuffix++;
        //         cachedPath = GetCachedEncryptedPath(fileNameId + fileSuffix);
        //     }

        //     return fileNameId + fileSuffix;
        // }

        private IEnumerator PrepareAndPlayVideo(string videoPath, string fileNameId)
        {
            bool prepareFinished = false;
            bool prepareSucceeded = false;

            if (videoPlayer == null)
            {
                Debug.LogError($"{LOG_TAG} VideoPlayer is not initialized for level {fileNameId}");
                OnVideoError?.Invoke(fileNameId, "VideoPlayer not initialized");
                yield break;
            }

            videoPlayer.Stop();
            isPrepared = false;

            string playableUrl = ConvertToUrl(videoPath);
            videoPlayer.url = playableUrl;

            Debug.Log($"{LOG_TAG} Preparing video for level {fileNameId}: {videoPath}");

            void HandlePrepared(VideoPlayer source)
            {
                prepareFinished = true;
                prepareSucceeded = true;
                isPrepared = true;
                currentfileNameId = fileNameId;
                Debug.Log($"{LOG_TAG} Video prepared for level {fileNameId}");
            }

            void HandleError(VideoPlayer source, string message)
            {
                prepareFinished = true;
                prepareSucceeded = false;
                isPrepared = false;
                Debug.LogError($"{LOG_TAG} Prepare failed for level {fileNameId}: {message}");
                OnVideoError?.Invoke(fileNameId, message);
            }

            videoPlayer.prepareCompleted += HandlePrepared;
            videoPlayer.errorReceived += HandleError;
            videoPlayer.Prepare();

            while (!prepareFinished)
            {
                yield return null;
            }

            Debug.Log($"{LOG_TAG} Prepared video for level {fileNameId}: {videoPath}");

            videoPlayer.prepareCompleted -= HandlePrepared;
            videoPlayer.errorReceived -= HandleError;

            if (prepareSucceeded)
            {
                Debug.Log($"{LOG_TAG} BindVideoTexture Start");
                // yield return BindVideoTexture();
                Debug.Log($"{LOG_TAG} BindVideoTexture End");
                Play();
            }
        }

        // private IEnumerator BindVideoTexture()
        // {
        //     float timeout = 3.0f;
        //     float elapsed = 0.0f;

        //     while (elapsed < timeout)
        //     {
        //         if (videoPlayer != null && displayTarget != null && videoPlayer.texture != null)
        //         {
        //             displayTarget.texture = videoPlayer.texture;
        //             if (!displayTarget.enabled)
        //             {
        //                 displayTarget.enabled = true;
        //             }
        //             Debug.Log($"{LOG_TAG} Video texture bound");
        //             yield break;
        //         }

        //         elapsed += Time.unscaledDeltaTime;
        //         yield return null;
        //     }

        //     Debug.LogWarning($"{LOG_TAG} Texture binding timeout");
        // }

        private void Play()
        {
            if (!isPrepared || videoPlayer == null)
            {
                Debug.LogWarning($"{LOG_TAG} Cannot play: video not prepared");
                return;
            }

            if (displayTarget != null)
            {
                displayTarget.color = colorShow;
                displayTarget.enabled = true;
            }

            videoPlayer.Play();
            OnVideoStarted?.Invoke(currentfileNameId);
            Debug.Log($"{LOG_TAG} Playback started for level {currentfileNameId}");

            if (iaAutoPause)
            {
                // videoPlayer.Pause();
                StartCoroutine(PlayAutoPause());
            }
        }
        IEnumerator PlayAutoPause()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            videoPlayer.Pause();
        }

        public void Stop()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                isPrepared = false;
                OnVideoStopped?.Invoke();
                Debug.Log($"{LOG_TAG} Playback stopped");
            }
        }

        public void StopAndCleanup()
        {
            Stop();

            // 删除临时解密的视频文件
            if (!string.IsNullOrEmpty(currentDecryptedFilePath) && File.Exists(currentDecryptedFilePath))
            {
                try
                {
                    File.Delete(currentDecryptedFilePath);
                    Debug.Log($"{LOG_TAG} Deleted temp decrypt file: {currentDecryptedFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{LOG_TAG} Failed to delete temp file: {e.Message}");
                }
                currentDecryptedFilePath = null;
            }
        }

        /// <summary>
        /// 是否有任何音轨
        /// </summary>
        /// <returns></returns>
        public bool DoesVideoHaveActiveAudio()
        {
            // 1. 检查是否有任何音轨
            if (videoPlayer.audioTrackCount == 0)
            {
                return false; // 没有音轨，判定为无声音频
            }

            // 2. 遍历所有音轨，检查是否有任何一个处于启用状态
            for (ushort i = 0; i < videoPlayer.audioTrackCount; i++)
            {
                if (videoPlayer.IsAudioTrackEnabled(i))
                {
                    return true; // 有至少一个音轨是启用的，判定为有声音频
                }
            }

            return false; // 虽然有音轨，但全部被禁用了
        }

        public void Pause()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
                Debug.Log($"{LOG_TAG} Playback paused");
            }
        }

        public bool Resume()
        {
            if (videoPlayer != null && !videoPlayer.isPlaying && isPrepared)
            {
                videoPlayer.Play();
                Debug.Log($"{LOG_TAG} Playback resumed");
                return true;
            }
            else
            {
                return false;
            }
        }
        public void SetVolume(float volume)
        {
            if (videoPlayer != null)
            {
                videoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(volume));
            }
        }

        public void SetMute(bool mute)
        {
            if (videoPlayer != null)
            {
                videoPlayer.SetDirectAudioMute(0, mute);
            }
        }

        public void SetLooping(bool loop)
        {
            if (videoPlayer != null)
            {
                videoPlayer.isLooping = loop;
            }
        }

        private string GetStreamingAssetsPath(string fileName)
        {
            if (LocalFolderPath.Contains("Assets/StreamingAssets"))
            {
                string relativePath = LocalFolderPath.Substring("Assets/StreamingAssets".Length).TrimStart('/');
                return Path.Combine(Application.streamingAssetsPath, relativePath, fileName);
            }

            return Path.Combine(LocalFolderPath, fileName);
        }

        private string ConvertToUrl(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            return $"file://{path}";
        }

        public void Dispose()
        {
            StopAndCleanup();

            if (currentLoadCoroutine != null)
            {
                StopCoroutine(currentLoadCoroutine);
            }
            if (fallbackRetryCoroutine != null)
            {
                StopCoroutine(fallbackRetryCoroutine);
            }
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
                Destroy(videoPlayer.gameObject);
                videoPlayer = null;
            }

            if (videoRenderTexture != null)
            {
                Destroy(videoRenderTexture);
                videoRenderTexture = null;
            }
        }

        #region static 下载

        public static void DownlaodNetVideo(string imageId)
        {
            string url = ServerUtil.MakeMP4DownloadUrl(imageId);

            PriorityDownloadManager.Instance.AddDownload(url, imageId, StaticNetDownload, (str) =>
            {
                UnityEngine.Debug.Log($"{str}:Image downlaod Fail");
            });
        }

        private static void StaticNetDownload(string imageId, byte[] textureByte)
        {
            try
            {
                if (textureByte != null)
                {
                    NetVideoDecodeAndSave(imageId, textureByte);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{imageId}:{e}");
            }
        }

        private static void NetVideoDecodeAndSave(string imageId, byte[] textureByte)
        {
            string fullPath = GetCachedEncryptedPath(imageId);
            if (File.Exists(fullPath)) return;

            byte[] processedData = Utility.OpenSSLCryptoHelper.DecryptBytes(textureByte, string.Concat(ServerUtil.keyBaseUrl));

            if (!ServerUtil.IsVideoFile(processedData))
                throw new Exception($"Process Decode Fail {imageId}");

            byte[] fileData = ServerUtil.EncryptOrDecrypt(processedData, imageId);
            File.WriteAllBytes(fullPath, fileData);
        }

        #endregion
    }

    #endregion
}

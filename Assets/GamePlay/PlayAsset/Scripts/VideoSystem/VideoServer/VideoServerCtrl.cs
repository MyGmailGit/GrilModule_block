// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using Unity.VisualScripting;
// using UnityEngine;
// using UnityEngine.Networking;

// namespace VideoSystem
// {
//     public class VideoServerCtrl : MonoBehaviour
//     {
//         public static VideoServerCtrl Instance { get; private set; }

//         [SerializeField] private int preloadCount = 2;
//         [SerializeField] private int maxConcurrentDownloads = 2; // 最大并发下载数

//         private Queue<int> preloadQueue = new Queue<int>();
//         private Dictionary<int, Coroutine> activeDownloads = new Dictionary<int, Coroutine>();
//         private Dictionary<int, int> retryCounts = new Dictionary<int, int>();
//         private int currentDownloads = 0;

//         public static string[] keyBaseUrl = { "d", "V", "1", "S", "m", "x", "-", "M", "B", "J", "1", "c", "C", "S", "9", "0", "h", "d", "r", "3", "E", "-", "o", "P", "O", "v", "1", "k", "P", "d", "y", "g", "V", "W", "C", "c", "7", "w", "k", "N", "c", "q", "U" };

//         private const int MAX_RETRY = 3;

//         private void Awake()
//         {
//             if (Instance != null && Instance != this)
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }

//         private void Start()
//         {
//             StartCoroutine(PreloadRoutine());
//         }

//         private void OnDestroy()
//         {
//             // 取消所有正在进行的下载
//             foreach (var download in activeDownloads.Values)
//             {
//                 if (download != null)
//                     StopCoroutine(download);
//             }
//             activeDownloads.Clear();
//         }

//         /// <summary>
//         /// 发起下载，通过事件通知结果
//         /// </summary>
//         public void DownloadVideo(int levelId)
//         {
//             // 检查是否已下载
//             if (IsVideoDownloaded(levelId))
//             {
//                 string cachedPath = GetLocalVideoPath(levelId);
// #if TEST_MODE
//                 Debug.Log($"[VideoServerCtrl] Level {levelId} already cached");
// #endif
//                 EventBus.Publish(new VideoDownloadCompleteEvent
//                 {
//                     LevelId = levelId,
//                     Success = true,
//                     LocalPath = cachedPath,
//                     CompleteTime = DateTime.Now,
//                     Duration = TimeSpan.Zero
//                 });
//                 return;
//             }

//             // 检查是否正在下载
//             if (activeDownloads.ContainsKey(levelId))
//             {
// #if TEST_MODE
//                 Debug.Log($"[VideoServerCtrl] Level {levelId} is already downloading");
// #endif
//                 return;
//             }

//             if (Application.internetReachability == NetworkReachability.NotReachable)
//             {
//                 EventBus.Publish(new VideoDownloadAllServersFailedEvent
//                 {
//                     LevelId = levelId,
//                     TotalAttempts = 0,
//                     TotalServersAttempted = 0,
//                     LastErrorMessage = "no internet",
//                     FailedTime = DateTime.Now
//                 });
//                 return;
//             }
//             // 开始下载
//             StartCoroutine(DownloadWithRetry(levelId));
//         }

//         /// <summary>
//         /// 取消下载
//         /// </summary>
//         public void CancelDownload(int levelId)
//         {
//             if (activeDownloads.TryGetValue(levelId, out var coroutine))
//             {
//                 StopCoroutine(coroutine);
//                 activeDownloads.Remove(levelId);
//                 retryCounts.Remove(levelId);
//                 currentDownloads--;

//                 EventBus.Publish(new VideoDownloadCompleteEvent
//                 {
//                     LevelId = levelId,
//                     Success = false,
//                     ErrorMessage = "Download cancelled by user",
//                     CompleteTime = DateTime.Now
//                 });
// #if TEST_MODE
//                 Debug.Log($"[VideoServerCtrl] Cancelled download for level {levelId}");
// #endif
//             }
//         }

//         private IEnumerator DownloadWithRetry(int levelId)
//         {
//             activeDownloads[levelId] = null; // 占位
//             currentDownloads++;

//             if (!retryCounts.ContainsKey(levelId))
//                 retryCounts[levelId] = 0;

//             string localPath = null;
//             bool success = false;
//             string errorMessage = null;
//             DateTime startTime = DateTime.Now;

//             int levelIdFileSuffix = levelId;

//             // 记录尝试过的服务器
//             HashSet<VideoServerType> attemptedServers = new HashSet<VideoServerType>();
//             int totalAttempts = 0;

//             while (retryCounts[levelId] < MAX_RETRY && !success)
//             {
//                 retryCounts[levelId]++;
//                 totalAttempts++;

//                 // 获取下载地址
//                 VideoServerUrl.UrlData urlData = VideoServerUrl.GetVideoUrl();
//                 if (urlData == null || string.IsNullOrEmpty(urlData.Url))
//                 {
//                     errorMessage = $"Failed to get URL for level {levelId}";
// #if TEST_MODE
//                     Debug.LogError($"[VideoServerCtrl] {errorMessage}, retry {retryCounts[levelId]}/{MAX_RETRY}");
// #endif
//                     continue;
//                 }

//                 string url = urlData.Url;
//                 VideoServerType serverType = urlData.videoServerType;
//                 attemptedServers.Add(serverType);
// #if TEST_MODE
//                 Debug.Log($"[VideoServerCtrl] Downloading level {levelId} from {serverType}: {url}");
// #endif

//                 // 发布下载开始事件
//                 EventBus.Publish(new VideoDownloadStartEvent
//                 {
//                     LevelId = levelId,
//                     Url = url,
//                     ServerType = serverType,
//                     StartTime = startTime
//                 });

//                 using (UnityWebRequest request = UnityWebRequest.Get(url))
//                 {
//                     request.downloadHandler = new DownloadHandlerBuffer();

//                     var operation = request.SendWebRequest();

//                     // 监听进度
//                     while (!operation.isDone)
//                     {
//                         EventBus.Publish(new VideoDownloadProgressEvent
//                         {
//                             LevelId = levelId,
//                             Progress = request.downloadProgress,
//                             DownloadedBytes = (long)(request.downloadedBytes),
//                             TotalBytes = request.downloadHandler?.data?.Length ?? 0
//                         });
//                         yield return null;
//                     }

//                     if (request.result != UnityWebRequest.Result.Success)
//                     {
//                         errorMessage = request.error;
// #if TEST_MODE
//                         Debug.LogWarning($"[VideoServerCtrl] Download failed from {url}: {errorMessage}");
// #endif
//                         // 标记服务器失效
//                         VideoServerUrl.SetServerInvalid(serverType, true);

//                         Watermelon.AnalyticsController.OnHttpErrorChange(Watermelon.AnalyticsStr.httperror_downloadvideo, request.responseCode.ToString(), request.error);
//                         continue;
//                     }

//                     // 下载成功，处理数据
//                     try
//                     {
//                         byte[] rawData = request.downloadHandler.data;
//                         byte[] processedData = ProcessByType(serverType, rawData);

//                         // 检查编码是否是视频
//                         if (!ServerUtil.IsVideoFile(processedData))
//                             throw new Exception($"Process Decode Fail {serverType}");

//                         string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
//                         if (!Directory.Exists(cacheDir))
//                             Directory.CreateDirectory(cacheDir);

//                         int fileSuffix = 0;
//                         string fileName = ServerUtil.GetVideoFileName(levelId + fileSuffix);
//                         localPath = Path.Combine(cacheDir, fileName);
//                         while (File.Exists(localPath))
//                         {
//                             fileSuffix++;
//                             fileName = ServerUtil.GetVideoFileName(levelId + fileSuffix);
//                             localPath = Path.Combine(cacheDir, fileName);
//                         }

//                         levelIdFileSuffix = levelId + fileSuffix;

//                         byte[] key = ServerUtil.GetEncryptionKey(levelIdFileSuffix);
//                         byte[] encryptedData = Utility.CryptoHelper.XorQuick(processedData, key);

//                         File.WriteAllBytes(localPath, encryptedData);

//                         success = true;
//                         errorMessage = null;
// #if TEST_MODE
//                         Debug.Log($"[VideoServerCtrl] Download success for level {levelIdFileSuffix}");
// #endif
//                     }
//                     catch (Exception e)
//                     {
// #if TEST_MODE
//                         errorMessage = $"Processing error: {e.Message}";
//                         Debug.LogError($"[VideoServerCtrl] {errorMessage}");
// #endif
//                         continue;
//                     }
//                 }
//             }

//             // 清理
//             activeDownloads.Remove(levelId);
//             retryCounts.Remove(levelId);
//             currentDownloads--;

//             var duration = DateTime.Now - startTime;

//             // 发布下载完成事件
//             EventBus.Publish(new VideoDownloadCompleteEvent
//             {
//                 LevelId = levelId,
//                 Success = success,
//                 LocalPath = localPath,
//                 ErrorMessage = errorMessage,
//                 CompleteTime = DateTime.Now,
//                 Duration = duration
//             });

//             // 如果所有服务器都失败了，发布专用事件
//             if (!success)
//             {
//                 EventBus.Publish(new VideoDownloadAllServersFailedEvent
//                 {
//                     LevelId = levelId,
//                     TotalAttempts = totalAttempts,
//                     TotalServersAttempted = attemptedServers.Count,
//                     LastErrorMessage = errorMessage,
//                     FailedTime = DateTime.Now
//                 });
// #if TEST_MODE
//                 Debug.LogWarning($"[VideoServerCtrl] All servers failed for level {levelId}. " +
//                               $"Attempted {attemptedServers.Count} different servers, " +
//                               $"total {totalAttempts} retries. Last error: {errorMessage}");
// #endif
//             }
//         }

//         private byte[] ProcessByType(VideoServerType serverType, byte[] data)
//         {
//             switch (serverType)
//             {
//                 case VideoServerType.Joy:
//                     return ProcessJoyVideo(data);
//                 case VideoServerType.Odhikltd:
//                     return ProcessOdhiVideo(data);
//                 case VideoServerType.ShinyScrews:
//                     return ProcessShinyVideo(data);
//                 case VideoServerType.BaseUrl:
//                     return ProcessBaseVideo(data);
//                 default:
//                     return data;
//             }
//         }

//         private byte[] ProcessJoyVideo(byte[] data)
//         {
//             // Joy 服务器的特殊处理
//             int headerLength = 32;
//             return data.Skip(headerLength).ToArray();
//         }

//         private byte[] ProcessOdhiVideo(byte[] data)
//         {
//             // Odhi 服务器的特殊处理
//             return data;
//         }

//         private byte[] ProcessShinyVideo(byte[] data)
//         {
//             // Shiny 服务器的特殊处理
//             return data;
//         }

//         private byte[] ProcessBaseVideo(byte[] data)
//         {

//             return Utility.OpenSSLCryptoHelper.DecryptBytes(data, string.Concat(keyBaseUrl));
//         }

//         /// <summary>
//         /// 预加载资源
//         /// </summary>
//         public void EnqueuePreload(int levelId)
//         {
//             if (!preloadQueue.Contains(levelId) && !IsVideoDownloaded(levelId) && !activeDownloads.ContainsKey(levelId))
//             {
//                 preloadQueue.Enqueue(levelId);
//                 EventBus.Publish(new VideoPreloadEvent { LevelId = levelId, IsStart = true });
//                 Debug.Log($"[VideoServerCtrl] Enqueued preload for level {levelId}");
//             }
//         }

//         public void PreloadLevels(List<int> levelIds)
//         {
//             foreach (int levelId in levelIds)
//             {
//                 EnqueuePreload(levelId);
//             }
//         }

//         private IEnumerator PreloadRoutine()
//         {
//             while (true)
//             {
//                 // 控制并发数
//                 if (currentDownloads < maxConcurrentDownloads && preloadQueue.Count > 0)
//                 {
//                     int nextId = preloadQueue.Dequeue();
//                     if (!IsVideoDownloaded(nextId) && !activeDownloads.ContainsKey(nextId))
//                     {
//                         DownloadVideo(nextId);
//                         yield return new WaitForSeconds(0.5f); // 避免瞬间发起太多请求
//                     }
//                 }
//                 yield return new WaitForSeconds(0.5f);
//             }
//         }

//         /// <summary>
//         /// 获取本地视频路径
//         /// </summary>
//         public string GetLocalVideoPath(int levelId)
//         {
//             string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
//             string fileName = ServerUtil.GetVideoFileName(levelId);
//             string path = Path.Combine(cacheDir, fileName);

//             return File.Exists(path) ? path : null;
//         }

//         /// <summary>
//         /// 检查视频是否已下载
//         /// </summary>
//         public bool IsVideoDownloaded(int levelId)
//         {
//             return GetLocalVideoPath(levelId) != null;
//         }

//         /// <summary>
//         /// 删除指定视频缓存
//         /// </summary>
//         public void DeleteVideoCache(int levelId)
//         {
//             string path = GetVideoFilePath(levelId);
//             if (File.Exists(path))
//             {
//                 long fileSize = new FileInfo(path).Length;
//                 File.Delete(path);

//                 EventBus.Publish(new VideoCacheClearedEvent
//                 {
//                     LevelId = levelId,
//                     FreedBytes = fileSize
//                 });

// #if TEST_MODE
//                 Debug.Log($"[VideoServerCtrl] Deleted cache for level {levelId}, freed {fileSize} bytes");
// #endif
//             }
//         }

//         /// <summary>
//         /// 清除所有缓存
//         /// </summary>
//         public void ClearAllCache()
//         {
//             string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
//             long totalSize = 0;

//             if (Directory.Exists(cacheDir))
//             {
//                 totalSize = GetDirectorySize(cacheDir);
//                 Directory.Delete(cacheDir, true);
//             }

//             EventBus.Publish(new VideoCacheClearedEvent
//             {
//                 LevelId = -1,
//                 FreedBytes = totalSize
//             });

//             Debug.Log($"[VideoServerCtrl] All cache cleared, freed {totalSize} bytes");
//         }

//         /// <summary>
//         /// 获取缓存大小（MB）
//         /// </summary>
//         public float GetCacheSizeMB()
//         {
//             string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
//             if (!Directory.Exists(cacheDir))
//                 return 0;

//             long totalBytes = GetDirectorySize(cacheDir);
//             return totalBytes / (1024f * 1024f);
//         }

//         /// <summary>
//         /// 获取已下载的视频数量
//         /// </summary>
//         public int GetDownloadedCount()
//         {
//             string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
//             if (!Directory.Exists(cacheDir))
//                 return 0;

//             return Directory.GetFiles(cacheDir, "*.dat").Length;
//         }

//         /// <summary>
//         /// 获取所有已下载的视频ID列表
//         /// </summary>
//         public List<int> GetAllDownloadedLevelIds()
//         {
//             List<int> downloadedIds = new List<int>();
//             string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);

//             if (!Directory.Exists(cacheDir))
//                 return downloadedIds;

//             foreach (string file in Directory.GetFiles(cacheDir, "*.dat"))
//             {
//                 string fileName = Path.GetFileNameWithoutExtension(file);
//                 if (int.TryParse(fileName, out int levelId))
//                 {
//                     downloadedIds.Add(levelId);
//                 }
//             }

//             return downloadedIds;
//         }

//         private string GetVideoFilePath(int levelId)
//         {
//             string cacheDir = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
//             string fileName = ServerUtil.GetVideoFileName(levelId);
//             return Path.Combine(cacheDir, fileName);
//         }

//         private long GetDirectorySize(string path)
//         {
//             if (!Directory.Exists(path))
//                 return 0;

//             long size = 0;
//             var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
//             foreach (var file in files)
//             {
//                 size += new FileInfo(file).Length;
//             }
//             return size;
//         }
//     }

//     /// <summary>
//     /// 下载进度处理器
//     /// </summary>
//     public class DownloadProgressHandler : DownloadHandlerScript
//     {
//         private int levelId;

//         public DownloadProgressHandler(UnityWebRequest request, int levelId) : base()
//         {
//             this.levelId = levelId;
//         }
//     }
// }
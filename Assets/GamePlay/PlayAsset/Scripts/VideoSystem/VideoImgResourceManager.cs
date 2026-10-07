using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
namespace VideoSystem
{
    /// <summary>
    /// 图片资源管理类 - 只负责加载、缓存、管理图片，不处理下载逻辑
    /// </summary>
    public class VideoImgResourceManager : MonoBehaviour
    {
        private static VideoImgResourceManager _instance;
        public static VideoImgResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[VideoImgResourceManager]");
                    _instance = go.AddComponent<VideoImgResourceManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // LRU缓存项
        private class LRUCacheItem
        {
            public Texture2D Texture;
            public int LastAccessTime;
            public string ImageKey;
            public bool isLoading;
        }

        // 缓存相关
        private Dictionary<string, LRUCacheItem> _imageCache = new Dictionary<string, LRUCacheItem>();
        private SortedDictionary<int, string> _accessTimeMap = new SortedDictionary<int, string>();
        private int _currentTime = 0;
        private const int MAX_CACHE_SIZE = 32;

        // 并发加载延时，避免大量同时加载卡顿
        private const int DELAY_THRESHOLD = 3;
        private const float BASE_LOAD_DELAY_SECONDS = 0.01f;
        private const float MAX_LOAD_DELAY_SECONDS = 0.4f;

        // 等待回调的请求
        private class PendingRequest
        {
            public Dictionary<int, Action<Texture2D>> Callbacks = new Dictionary<int, Action<Texture2D>>();
            public bool IsLoading = false; // 是否正在加载中
        }

        private Dictionary<string, PendingRequest> _pendingRequests = new Dictionary<string, PendingRequest>();
        private int _nextRequestId = 0;

        // 下载事件（由外部触发）
        public event Action<string> OnImageDownloadRequest; // 请求下载图片


        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // VideoImgResourceManager.Instance.InitLocalImg();
        public void InitLocalImg()
        {
            string[] strImgName = {
                "6000106013",
                "6000154013",
                "6000204013",
                "6000242013",};

            foreach (var it in strImgName)
            {
                if (IsPersistentFileExists(it)) continue;
                StartCoroutine(InitToLoadStreamImg(it));
            }
        }
        private IEnumerator InitToLoadStreamImg(string imageId)
        {
            yield return LoadFromStreamingAssetsPath(imageId, (imgId, textur) =>
            {
                if (textur != null)
                {
                    AddToCache(imgId, textur);
                }
            });
        }


        #region  pin 钉住逻辑
        // 主要为了解决，如果图片正好在总数一半的位置，并且这个位置还是在显示的，那么往上滚动再往下滚动就会导致其他数据都被更新了，唯独这张图没更新，这样就会导致下一次获取图，这张正在显示的图被删除
        // private readonly HashSet<string> _pinnedImages = new();
        private readonly Dictionary<string, int> _pinnedImageRefCounts = new();
        public void Pin(string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
                return;

            // _pinnedImages.Add(imageId);
            if (_pinnedImageRefCounts.ContainsKey(imageId))
                _pinnedImageRefCounts[imageId]++;
            else
                _pinnedImageRefCounts[imageId] = 1;
        }
        public void UnPin(string imageId)
        {
            if (string.IsNullOrEmpty(imageId))
                return;

            // _pinnedImages.Remove(imageId);
            if (_pinnedImageRefCounts.ContainsKey(imageId))
            {
                _pinnedImageRefCounts[imageId]--;
                if (_pinnedImageRefCounts[imageId] <= 0)
                    _pinnedImageRefCounts.Remove(imageId);
            }
        }

        private bool IsPinned(string imageId)
        {
            // return _pinnedImages.Contains(imageId);
            return _pinnedImageRefCounts.ContainsKey(imageId);
        }
        public int GetPinCount(string imageId)
        {
            return _pinnedImageRefCounts.TryGetValue(imageId, out int count) ? count : 0;
        }
        #endregion

        /// <summary>
        /// 通过编号获取图片
        /// </summary>
        /// <param name="imageId">图片编号</param>
        /// <param name="onComplete">加载完成回调</param>
        /// <returns>请求ID，用于取消加载</returns>
        public int GetImage(string imageId, Action<Texture2D> onComplete)
        {
            int requestId = _nextRequestId++;
            Debug.Log($"[VideoImgResourceManager] getImg: {imageId}");
            // 1. 检查内存缓存
            if (_imageCache.TryGetValue(imageId, out LRUCacheItem cacheItem))
            {
                if (!cacheItem.isLoading)
                {
                    UpdateAccessTime(imageId, cacheItem);
                    onComplete?.Invoke(cacheItem.Texture);
                    return requestId;
                }
            }

            // 2. 开始加载本地图片
            StartCoroutine(LoadImageNoStream(imageId, onComplete, requestId));

            return requestId;
        }

        /// <summary>
        /// 通过编号获取图片（带附加数据，用于验证回调有效性）
        /// </summary>
        // public int GetImage<T>(string imageId, T attachedData, Action<Texture2D, T> onComplete)
        // {
        //     int requestId = _nextRequestId++;

        //     // 检查内存缓存
        //     if (_imageCache.TryGetValue(imageId, out LRUCacheItem cacheItem))
        //     {
        //         if (!cacheItem.isLoading)
        //         {
        //             UpdateAccessTime(imageId, cacheItem);
        //             onComplete?.Invoke(cacheItem.Texture, attachedData);
        //             return requestId;
        //         }
        //     }

        //     // 包装回调
        //     Action<Texture2D> wrappedCallback = (texture) =>
        //     {
        //         onComplete?.Invoke(texture, attachedData);
        //     };

        //     // 开始加载本地图片
        //     StartCoroutine(LoadImage(imageId, wrappedCallback, requestId));

        //     return requestId;
        // }

        // /// <summary>
        // /// 加载图片（从本地）
        // /// </summary>
        // private IEnumerator LoadImage(string imageId, Action<Texture2D> onComplete, int requestId)
        // {
        //     // 检查是否已经在加载中
        //     if (!_pendingRequests.ContainsKey(imageId))
        //     {
        //         _pendingRequests[imageId] = new PendingRequest();
        //     }

        //     var pendingRequest = _pendingRequests[imageId];
        //     pendingRequest.Callbacks[requestId] = onComplete;

        //     // 如果已经在加载中，直接返回，等待加载完成
        //     if (pendingRequest.IsLoading)
        //     {
        //         yield break;
        //     }

        //     // 标记开始加载
        //     pendingRequest.IsLoading = true;

        //     // 尝试从本地加载
        //     Texture2D texture = null;

        //     yield return null;
        //     // 1. 如果StreamingAssets没有，检查PersistentDataPath
        //     if (texture == null)
        //     {
        //         texture = LoadFromPersistentDataPath(imageId);
        //     }

        //     // 2. 检查 StreamingAssetsPath
        //     if (texture == null)
        //     {
        //         // 依据当前等待请求数添加动态延时，避免大量同时加载卡顿
        //         float waitTime = GetDynamicLoadDelay();
        //         if (waitTime > 0f)
        //         {
        //             yield return new WaitForSeconds(waitTime);
        //         }

        //         yield return LoadFromStreamingAssetsPath(imageId, (imgid, textur) =>
        //         {
        //             texture = textur;
        //         });
        //     }

        //     // 3. 如果本地都没有，请求下载
        //     if (texture == null)
        //     {
        //         // 触发下载请求事件，让外部处理下载
        //         // OnImageDownloadRequest?.Invoke(imageId);
        //         // todo Download
        //         AddToCache(imageId, null, true);
        //         string fileName = imageId + ".png";
        //         string url = VideoServerUrl.BASE_URL + fileName;

        //         PriorityDownloadManager.Instance.AddDownload(url, imageId, NotifyImageDownloaded, (str) =>
        //         {
        //             UnityEngine.Debug.Log($"{str}:Image downlaod Fail");
        //         });
        //         yield break;
        //     }

        //     // 本地找到了，加入缓存并回调
        //     if (texture != null)
        //     {
        //         AddToCache(imageId, texture);
        //         ExecuteCallbacks(imageId, texture);
        //     }
        //     else
        //     {
        //         // 加载失败，回调null
        //         ExecuteCallbacks(imageId, null);
        //     }

        //     // 清理请求记录（如果没有等待下载的请求）
        //     if (_pendingRequests.ContainsKey(imageId) && _pendingRequests[imageId].Callbacks.Count == 0)
        //     {
        //         _pendingRequests.Remove(imageId);
        //     }
        // }
        private IEnumerator LoadImageNoStream(string imageId, Action<Texture2D> onComplete, int requestId)
        {
            // 检查是否已经在加载中
            if (!_pendingRequests.ContainsKey(imageId))
            {
                _pendingRequests[imageId] = new PendingRequest();
            }

            var pendingRequest = _pendingRequests[imageId];
            pendingRequest.Callbacks[requestId] = onComplete;

            // 如果已经在加载中，直接返回，等待加载完成
            if (pendingRequest.IsLoading)
            {
                yield break;
            }

            // 标记开始加载
            pendingRequest.IsLoading = true;

            // 尝试从本地加载
            Texture2D texture = null;

            yield return null;
            // 1. 检查PersistentDataPath
            if (texture == null)
            {
                texture = LoadFromPersistentDataPath(imageId);
            }

            // 2.检查 StreamingAssetsPath
            if (texture == null)
            {
                // 依据当前等待请求数添加动态延时，避免大量同时加载卡顿
                float waitTime = GetDynamicLoadDelay();
                if (waitTime > 0f)
                {
                    yield return new WaitForSeconds(waitTime);
                }

                yield return LoadFromStreamingAssetsPath(imageId, (imgid, textur) =>
                {
                    texture = textur;
                });
            }

            // 3. 如果本地都没有，请求下载
            if (texture == null)
            {
                // 触发下载请求事件，让外部处理下载
                // OnImageDownloadRequest?.Invoke(imageId);
                // todo Download
                AddToCache(imageId, null, true);
                // string fileName = imageId + ".png";
                // string url = VideoServerUrl.BASE_URL + fileName;

                PriorityDownloadManager.Instance.AddDownload(MakePngDownloadUrl(imageId), imageId, NotifyImageDownloaded, (str) =>
                {
                    UnityEngine.Debug.Log($"{str}:Image downlaod Fail");
                }, DownloadProgress);
                yield break;
            }

            // 本地找到了，加入缓存并回调
            if (texture != null)
            {
                AddToCache(imageId, texture);
                ExecuteCallbacks(imageId, texture);
            }
            else
            {
                // 加载失败，回调null
                ExecuteCallbacks(imageId, null);
            }

            // 清理请求记录（如果没有等待下载的请求）
            if (_pendingRequests.ContainsKey(imageId) && _pendingRequests[imageId].Callbacks.Count == 0)
            {
                _pendingRequests.Remove(imageId);
            }
        }

        public string MakePngDownloadUrl(string fileId)
        {
            string fileName = fileId + ".png";
            return VideoServerUrl.BASE_URL + fileName;
        }
        /// <summary>
        /// 从StreamingAssetsPath加载图片（支持Android）
        /// </summary>
        private IEnumerator LoadFromStreamingAssetsPath(string imageId, Action<string, Texture2D> callback)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, ServerUtil.NativePngPath, imageId + ".png");
            Texture2D texture = null;

            byte[] fileData = null;

#if UNITY_EDITOR || UNITY_IOS
            if (File.Exists(streamingPath))
            {
                fileData = File.ReadAllBytes(streamingPath);
            }
#elif UNITY_ANDROID
            using (UnityWebRequest request = UnityWebRequest.Get(streamingPath))
            {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    // texture = DownloadHandlerTexture.GetContent(request);
                    fileData = request.downloadHandler.data;
                }
            }
#endif
            yield return null;
            if (fileData != null)
            {
                var DataTex = Utility.OpenSSLCryptoHelper.DecryptBytes(fileData, string.Concat(ServerUtil.keyBaseUrl));

                texture = new Texture2D(2, 2);
                texture.LoadImage(DataTex);

                // texture = ResizeTexture(texture);

                // yield return StartCoroutine(ResizeTexture(texture, imageId, (callTex, imgId) =>
                // {
                SaveToPersistentDataPath(imageId, texture);
                callback.Invoke(imageId, texture);
                // }));

                yield return null;

            }
            yield return null;
        }

        private float GetDynamicLoadDelay()
        {
            int pendingCount = _pendingRequests.Count;
            if (pendingCount <= DELAY_THRESHOLD)
            {
                return 0f;
            }

            float extra = (pendingCount - DELAY_THRESHOLD) * 0.1f;
            return Mathf.Min(MAX_LOAD_DELAY_SECONDS, BASE_LOAD_DELAY_SECONDS + extra);
        }

        /// <summary>
        /// 从PersistentDataPath加载图片
        /// </summary>
        private Texture2D LoadFromPersistentDataPath(string imageId)
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME, imageId + ".png");

            if (File.Exists(persistentPath))
            {
                ///解密并且显示
                byte[] fileData = ServerUtil.EncryptOrDecrypt(File.ReadAllBytes(persistentPath), imageId);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                return texture;
            }

            return null;
        }
        private bool IsPersistentFileExists(string imageId)
        {
            string persistentPath = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME, imageId + ".png");

            if (File.Exists(persistentPath))
            {
                return true;
            }
            return false;
        }


        private void DownloadProgress(string fileId, float progress)
        {

        }

        /// <summary>
        /// 外部调用：通知图片下载完成
        /// </summary>
        /// <param name="imageId">图片编号</param>
        /// <param name="texture">下载的纹理（如果下载失败则为null）</param>
        public void NotifyImageDownloaded(string imageId, byte[] textureByte)
        {
            try
            {
                if (textureByte != null)
                {
                    var DataTex = Utility.OpenSSLCryptoHelper.DecryptBytes(textureByte, string.Concat(ServerUtil.keyBaseUrl));
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(DataTex);

                    StartCoroutine(ResizeTexture(texture, imageId, (callTex, imgId) =>
                    {
                        // 保存到PersistentDataPath
                        SaveToPersistentDataPath(imgId, callTex);
                        // 添加到缓存
                        UpdateCache(imgId, callTex);
                        // 执行所有等待的回调
                        ExecuteCallbacks(imgId, callTex);

                        // 清理请求记录
                        if (_pendingRequests.ContainsKey(imgId))
                        {
                            _pendingRequests.Remove(imgId);
                        }
                    }));
                }
                else
                {
                    // 清理请求记录
                    if (_pendingRequests.ContainsKey(imageId))
                    {
                        _pendingRequests.Remove(imageId);
                    }
                }

            }
            catch (Exception e)
            {
                Debug.LogError($"{imageId}:{e}");
            }
        }

        /// <summary>
        /// 执行指定图片的所有回调
        /// </summary>
        private void ExecuteCallbacks(string imageId, Texture2D texture)
        {
            if (_pendingRequests.TryGetValue(imageId, out PendingRequest pendingRequest))
            {
                // 复制回调列表，避免遍历时被修改
                var callbacks = new Dictionary<int, Action<Texture2D>>(pendingRequest.Callbacks);

                foreach (var callback in callbacks.Values)
                {
                    try
                    {
                        callback?.Invoke(texture);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"执行图片回调时出错: {imageId}, 错误: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 保存图片到PersistentDataPath
        /// </summary>
        private void SaveToPersistentDataPath(string imageId, Texture2D texture)
        {
            try
            {
                string directory = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string filePath = Path.Combine(directory, imageId + ".png");

                byte[] pngData = texture.EncodeToPNG();
                byte[] fileData = ServerUtil.EncryptOrDecrypt(pngData, imageId);
                File.WriteAllBytes(filePath, fileData);

                Debug.Log($"图片已保存: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"保存图片失败: {imageId}, 错误: {e.Message}");
            }
        }
        // private Texture2D ResizeTexture(Texture2D source)
        // {
        //     int width = source.width / 3;
        //     int height = source.height / 3;

        //     RenderTexture rt = RenderTexture.GetTemporary(
        //         width,
        //         height,
        //         0,
        //         RenderTextureFormat.ARGB32);

        //     Graphics.Blit(source, rt);

        //     RenderTexture prev = RenderTexture.active;
        //     RenderTexture.active = rt;

        //     Texture2D result = new Texture2D(
        //         width,
        //         height,
        //         TextureFormat.RGBA32,
        //         false);

        //     result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //     result.Apply();

        //     RenderTexture.active = prev;
        //     RenderTexture.ReleaseTemporary(rt);

        //     return result;
        // }
        public static IEnumerator ResizeTexture(Texture2D source, string imgId, Action<Texture2D, string> callback)
        {
            yield return null;
            int width = source.width / 3;
            int height = source.height / 3;

            RenderTexture rt = RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32);

            Graphics.Blit(source, rt);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D result = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false);

            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();

            yield return null;

            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);

            callback.Invoke(result, imgId);
            // return result;
        }

        /// <summary>
        /// 取消指定图片的指定加载请求（只是取消回调，不取消下载）
        /// </summary>
        public void CancelLoad(string imageId, int requestId)
        {
            if (_pendingRequests.TryGetValue(imageId, out PendingRequest pendingRequest))
            {
                if (pendingRequest.Callbacks.Remove(requestId))
                {
                    Debug.Log($"已取消加载请求: {imageId}, RequestId: {requestId}");

                    // 如果没有回调等待了，清理请求记录
                    if (pendingRequest.Callbacks.Count == 0)
                    {
                        _pendingRequests.Remove(imageId);
                    }
                }
            }
        }

        /// <summary>
        /// 取消指定图片的所有加载请求
        /// </summary>
        public void CancelAllLoads(string imageId)
        {
            if (_pendingRequests.Remove(imageId))
            {
                Debug.Log($"已取消所有加载请求: {imageId}");
            }
        }

        /// <summary>
        /// 添加到缓存，处理LRU逻辑
        /// </summary>
        private void AddToCache(string imageId, Texture2D texture, bool isLoadingArg = false)
        {
            // 如果已存在，先移除旧的
            if (_imageCache.ContainsKey(imageId))
            {
                RemoveFromCache(imageId);
            }

            // 如果超过最大容量，移除最不常用的
            if (_imageCache.Count >= MAX_CACHE_SIZE)
            {
                RemoveLeastUsed();
            }

            // 添加新项
            var cacheItem = new LRUCacheItem
            {
                Texture = texture,
                LastAccessTime = ++_currentTime,
                ImageKey = imageId,
                isLoading = isLoadingArg
            };

            _imageCache[imageId] = cacheItem;
            _accessTimeMap[cacheItem.LastAccessTime] = imageId;

            Debug.Log($"图片已加入缓存: {imageId}, 当前缓存数量: {_imageCache.Count}");
        }
        private void UpdateCache(string imageId, Texture2D texture)
        {
            // 如果已存在，先移除旧的
            if (_imageCache.ContainsKey(imageId))
            {
                _imageCache[imageId].Texture = texture;
                _imageCache[imageId].isLoading = false;
                Debug.Log($"图片已刷新缓存: {imageId}, 当前缓存数量: {_imageCache.Count}");
                return;
            }

            // // 如果超过最大容量，移除最不常用的
            // if (_imageCache.Count >= MAX_CACHE_SIZE)
            // {
            //     RemoveLeastUsed();
            // }
            // // 添加新项
            // var cacheItem = new LRUCacheItem
            // {
            //     Texture = texture,
            //     LastAccessTime = ++_currentTime,
            //     ImageKey = imageId,
            //     isLoading = false,
            // };

            // _imageCache[imageId] = cacheItem;
            // _accessTimeMap[cacheItem.LastAccessTime] = imageId;


        }

        /// <summary>
        /// 更新访问时间
        /// </summary>
        private void UpdateAccessTime(string imageId, LRUCacheItem cacheItem)
        {
            _accessTimeMap.Remove(cacheItem.LastAccessTime);
            cacheItem.LastAccessTime = ++_currentTime;
            _accessTimeMap[cacheItem.LastAccessTime] = imageId;
        }

        /// <summary>
        /// 从缓存中移除
        /// </summary>
        private void RemoveFromCache(string imageId)
        {
            if (_imageCache.TryGetValue(imageId, out LRUCacheItem cacheItem))
            {
                _accessTimeMap.Remove(cacheItem.LastAccessTime);
                _imageCache.Remove(imageId);

                if (cacheItem.Texture != null)
                {
                    Destroy(cacheItem.Texture);
                }

                Debug.Log($"图片已从缓存移除: {imageId}");
            }
        }

        /// <summary>
        /// 移除最不常用的图片
        /// </summary>
        private void RemoveLeastUsed()
        {
            /*
            if (_accessTimeMap.Count > 0)
            {
                var first = _accessTimeMap.First();
                string imageId = first.Value;
                RemoveFromCache(imageId);
                Debug.Log($"移除最不常用图片: {imageId}");
            }
            */
            foreach (var pair in _accessTimeMap)
            {
                string imageId = pair.Value;

                if (IsPinned(imageId))
                    continue;

                // if (_imageCache.TryGetValue(imageId, out var item))
                // {
                //     if (item.isLoading)
                //         continue;
                // }

                RemoveFromCache(imageId);

                Debug.Log($"LRU Remove : {imageId}");

                return;
            }

            Debug.LogWarning("No removable image found.");
        }

        /// <summary>
        /// 检查图片是否在缓存中
        /// </summary>
        public bool IsImageCached(string imageId)
        {
            return _imageCache.ContainsKey(imageId);
        }

        /// <summary>
        /// 从缓存直接获取图片（不会触发加载）
        /// </summary>
        public Texture2D GetCachedImage(string imageId)
        {
            if (_imageCache.TryGetValue(imageId, out LRUCacheItem cacheItem))
            {
                UpdateAccessTime(imageId, cacheItem);
                return cacheItem.Texture;
            }
            return null;
        }

        /// <summary>
        /// 手动清理特定图片的缓存
        /// </summary>
        public void ClearCache(string imageId)
        {
            RemoveFromCache(imageId);
        }

        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearAllCache()
        {
            foreach (var item in _imageCache.Values)
            {
                if (item.Texture != null)
                {
                    Destroy(item.Texture);
                }
            }
            _imageCache.Clear();
            _accessTimeMap.Clear();
            Debug.Log("已清空所有图片缓存");
        }

        /// <summary>
        /// 完全释放所有资源（切换场景时调用）
        /// </summary>
        public void ReleaseAllResources()
        {
            ClearAllCache(); // 复用原有方法

            // 额外清理状态
            _pinnedImageRefCounts.Clear();

            foreach (var pending in _pendingRequests.Values)
            {
                pending.Callbacks.Clear();
            }
            _pendingRequests.Clear();

            _currentTime = 0;

            Debug.Log("已释放所有资源");
        }

        /// <summary>
        /// 释放所有纹理资源，但保留Pin状态（不常用）
        /// </summary>
        public void ReleaseTexturesOnly()
        {
            foreach (var item in _imageCache.Values)
            {
                if (item.Texture != null)
                {
                    Destroy(item.Texture);
                }
            }
            _imageCache.Clear();
            _accessTimeMap.Clear();
            Debug.Log("已释放所有纹理，保留Pin状态");
        }

        /// <summary>
        /// 释放指定图片的资源
        /// </summary>
        public void ReleaseImage(string imageId)
        {
            RemoveFromCache(imageId);
            if (_pendingRequests.ContainsKey(imageId))
            {
                _pendingRequests[imageId].Callbacks.Clear();
                _pendingRequests.Remove(imageId);
            }
            if (_pinnedImageRefCounts.ContainsKey(imageId))
            {
                _pinnedImageRefCounts.Remove(imageId);
            }
        }


        /// <summary>
        /// 获取当前缓存数量
        /// </summary>
        public int GetCacheCount()
        {
            return _imageCache.Count;
        }

        /// <summary>
        /// 检查图片本地是否存在
        /// </summary>
        public bool IsImageLocalExists(string imageId)
        {
            string streamingPath = Path.Combine(Application.streamingAssetsPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME, imageId + ".png");
            string persistentPath = Path.Combine(Application.persistentDataPath, ServerUtil.MENU_VIDEO_CACHE_FOLDER_NAME, imageId + ".png");

#if UNITY_ANDROID
            // Android无法直接检查StreamingAssets，这里简单返回false
            return File.Exists(persistentPath);
#else
            return File.Exists(streamingPath) || File.Exists(persistentPath);
#endif
        }

        private void OnEnable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            // 场景切换时自动释放所有资源
            ReleaseAllResources();
        }


        private void OnDestroy()
        {
            ReleaseAllResources();
        }
    }
}
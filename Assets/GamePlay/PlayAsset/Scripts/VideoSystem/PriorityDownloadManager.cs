using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace VideoSystem
{
    public class PriorityDownloadManager : MonoBehaviour
    {
        private static PriorityDownloadManager _instance;
        public static PriorityDownloadManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("[PriorityDownloadManager]");
                    _instance = go.AddComponent<PriorityDownloadManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 下载请求数据结构
        private class DownloadRequest
        {
            public string url;
            public string fileId;
            public List<Action<string, byte[]>> onSuccessCallbacks = new List<Action<string, byte[]>>();
            public List<Action<string>> onErrorCallbacks = new List<Action<string>>();
            public List<Action<string, float>> onProgressCallbacks = new List<Action<string, float>>();
            public float progress;
            public bool isPriority;
            public int failCount;
            public UnityWebRequest webRequest;
            public Coroutine coroutine;
        }

        // 等待栈和状态
        private List<DownloadRequest> waitingStack = new List<DownloadRequest>();
        private List<DownloadRequest> failedQueue = new List<DownloadRequest>();
        private List<DownloadRequest> downloadingList = new List<DownloadRequest>();

        // 去重相关
        private HashSet<string> downloadingUrls = new HashSet<string>();
        private HashSet<string> waitingUrls = new HashSet<string>();
        private HashSet<string> failedUrls = new HashSet<string>();
        private Dictionary<string, DownloadRequest> urlToRequestMap = new Dictionary<string, DownloadRequest>();

        private int maxConcurrent = 2;
        private int maxRetryCount = 1;
        private bool isProcessingFailedQueue = false;

        private void Update()
        {
            TryStartNextDownloads();
            TryProcessFailedQueue();
        }

        // 对外接口：添加下载任务（普通优先级）
        public void AddDownload(string url, string fileId, Action<string, byte[]> onSuccess, Action<string> onError)
        {
            AddDownload(url, fileId, onSuccess, onError, null, false);
        }

        // 对外接口：添加下载任务（可指定优先级）
        public void AddDownload(string url, string fileId, Action<string, byte[]> onSuccess, Action<string> onError, bool isPriority)
        {
            AddDownload(url, fileId, onSuccess, onError, null, isPriority);
        }

        // 对外接口：添加下载任务（带进度回调）
        public void AddDownload(string url, string fileId, Action<string, byte[]> onSuccess, Action<string> onError, Action<string, float> onProgress)
        {
            AddDownload(url, fileId, onSuccess, onError, onProgress, false);
        }

        // 对外接口：添加下载任务（完整参数）
        public void AddDownload(string url, string fileId, Action<string, byte[]> onSuccess, Action<string> onError, Action<string, float> onProgress, bool isPriority)
        {
            // 检查是否已存在该URL的请求
            if (urlToRequestMap.TryGetValue(url, out DownloadRequest existingReq))
            {
                // 如果已存在，添加回调
                if (onSuccess != null) existingReq.onSuccessCallbacks.Add(onSuccess);
                if (onError != null) existingReq.onErrorCallbacks.Add(onError);
                if (onProgress != null) existingReq.onProgressCallbacks.Add(onProgress);

                // 如果该请求正在下载中且是优先级请求，可以提升优先级
                if (isPriority && downloadingList.Contains(existingReq))
                {
                    existingReq.isPriority = true;
                }
                return;
            }

            DownloadRequest newRequest = new DownloadRequest
            {
                url = url,
                fileId = fileId,
                progress = 0f,
                isPriority = isPriority,
                failCount = 0
            };

            if (onSuccess != null) newRequest.onSuccessCallbacks.Add(onSuccess);
            if (onError != null) newRequest.onErrorCallbacks.Add(onError);
            if (onProgress != null) newRequest.onProgressCallbacks.Add(onProgress);

            // 添加到映射
            urlToRequestMap[url] = newRequest;

            if (isPriority)
            {
                HandlePriorityRequest(newRequest);
            }
            else
            {
                waitingStack.Add(newRequest);
                waitingUrls.Add(url);
            }

            TryStartNextDownloads();
        }

        // 移除回调函数
        public void RemoveCallbacks(string url, Action<string, byte[]> onSuccess = null, Action<string> onError = null, Action<string, float> onProgress = null)
        {
            if (urlToRequestMap.TryGetValue(url, out DownloadRequest req))
            {
                if (onSuccess != null) req.onSuccessCallbacks.Remove(onSuccess);
                if (onError != null) req.onErrorCallbacks.Remove(onError);
                if (onProgress != null) req.onProgressCallbacks.Remove(onProgress);
            }
        }

        // 移除所有回调
        public void RemoveAllCallbacks(string url)
        {
            if (urlToRequestMap.TryGetValue(url, out DownloadRequest req))
            {
                req.onSuccessCallbacks.Clear();
                req.onErrorCallbacks.Clear();
                req.onProgressCallbacks.Clear();
            }
        }

        // 处理优先级请求：打断一个正在下载且进度最小的任务
        private void HandlePriorityRequest(DownloadRequest priorityReq)
        {
            if (downloadingList.Count == 0)
            {
                waitingStack.Add(priorityReq);
                waitingUrls.Add(priorityReq.url);
                return;
            }

            // 找出进度最小的下载任务（用于取消）
            DownloadRequest toCancel = null;
            float minProgress = 1f;
            foreach (var req in downloadingList)
            {
                if (req.progress < minProgress)
                {
                    minProgress = req.progress;
                    toCancel = req;
                }
            }

            if (toCancel != null)
            {
                // 取消该任务
                CancelDownload(toCancel, putBackToWaitQueue: true);
            }

            // 将优先级任务推入等待栈顶
            waitingStack.Add(priorityReq);
            waitingUrls.Add(priorityReq.url);
        }

        // 取消下载并可选择是否放回等待队列
        private void CancelDownload(DownloadRequest req, bool putBackToWaitQueue)
        {
            if (req.coroutine != null)
                StopCoroutine(req.coroutine);
            if (req.webRequest != null)
            {
                req.webRequest.Abort();
                req.webRequest.Dispose();
                req.webRequest = null;
            }
            downloadingList.Remove(req);
            downloadingUrls.Remove(req.url);

            if (putBackToWaitQueue)
            {
                // 放回等待栈顶（不增加失败计数）
                waitingStack.Add(req);
                waitingUrls.Add(req.url);
                // 保留在映射中
            }
            else
            {
                // 彻底移除
                urlToRequestMap.Remove(req.url);
                waitingUrls.Remove(req.url);
                failedUrls.Remove(req.url);
            }
        }

        // 尝试启动新下载
        private void TryStartNextDownloads()
        {
            while (downloadingList.Count < maxConcurrent && waitingStack.Count > 0)
            {
                DownloadRequest next = waitingStack[waitingStack.Count - 1];
                waitingStack.RemoveAt(waitingStack.Count - 1);
                waitingUrls.Remove(next.url);
                StartCoroutine(DownloadCoroutine(next));
            }
        }

        // 失败队列处理
        private void TryProcessFailedQueue()
        {
            if (isProcessingFailedQueue) return;
            if (downloadingList.Count >= maxConcurrent) return;
            if (failedQueue.Count == 0) return;

            StartCoroutine(ProcessFailedQueueCoroutine());
        }

        private IEnumerator ProcessFailedQueueCoroutine()
        {
            isProcessingFailedQueue = true;

            // 按失败次数少优先
            failedQueue.Sort((a, b) => a.failCount.CompareTo(b.failCount));

            List<DownloadRequest> toRetry = new List<DownloadRequest>(failedQueue);
            failedQueue.Clear();

            foreach (var req in toRetry)
            {
                // 等待有空位
                while (downloadingList.Count >= maxConcurrent)
                    yield return null;

                req.failCount++;
                failedUrls.Remove(req.url);
                StartCoroutine(DownloadCoroutine(req));
            }

            isProcessingFailedQueue = false;
        }

        // 实际下载协程
        private IEnumerator DownloadCoroutine(DownloadRequest req)
        {
            downloadingList.Add(req);
            downloadingUrls.Add(req.url);
            waitingUrls.Remove(req.url);
            failedUrls.Remove(req.url);

            using (UnityWebRequest uwr = UnityWebRequest.Get(req.url))
            {
                req.webRequest = uwr;
                var operation = uwr.SendWebRequest();

                // 监听进度
                while (!operation.isDone)
                {
                    req.progress = uwr.downloadProgress;
                    // 触发进度回调
                    foreach (var callback in req.onProgressCallbacks)
                    {
                        callback?.Invoke(req.fileId, req.progress);
                    }
                    yield return null;
                }

                req.progress = 1f;
                // 最后一次进度回调
                foreach (var callback in req.onProgressCallbacks)
                {
                    callback?.Invoke(req.fileId, req.progress);
                }

                // 从映射中移除
                urlToRequestMap.Remove(req.url);
                downloadingUrls.Remove(req.url);

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = uwr.downloadHandler.data;
                    // 触发所有成功回调
                    foreach (var callback in req.onSuccessCallbacks)
                    {
                        callback?.Invoke(req.fileId, data);
                    }
                }
                else
                {
                    string errorMsg = $"下载失败 [{req.url}] : {uwr.error}";
                    Debug.LogError(errorMsg);

                    // 失败处理：检查重试次数
                    if (req.failCount < maxRetryCount)
                    {
                        // 放入失败队列等待重试
                        req.webRequest = null;
                        req.coroutine = null;
                        failedQueue.Add(req);
                        failedUrls.Add(req.url);
                        // 重新添加到映射（保留回调）
                        urlToRequestMap[req.url] = req;
                    }
                    else
                    {
                        // 超过重试次数，触发所有错误回调
                        foreach (var callback in req.onErrorCallbacks)
                        {
                            callback?.Invoke(errorMsg);
                        }
                        // 彻底移除
                        urlToRequestMap.Remove(req.url);
                        failedUrls.Remove(req.url);
                        waitingUrls.Remove(req.url);
                    }
                }
            }

            req.webRequest = null;
            downloadingList.Remove(req);
            req.coroutine = null;

            // 尝试继续下载
            TryStartNextDownloads();
            TryProcessFailedQueue();
        }

        // 清空所有队列
        public void ClearAll()
        {
            foreach (var req in downloadingList)
            {
                if (req.webRequest != null)
                    req.webRequest.Abort();
                if (req.coroutine != null)
                    StopCoroutine(req.coroutine);
            }
            waitingStack.Clear();
            failedQueue.Clear();
            downloadingList.Clear();
            downloadingUrls.Clear();
            waitingUrls.Clear();
            failedUrls.Clear();
            urlToRequestMap.Clear();
        }

        // 获取下载进度
        public float GetProgress(string url)
        {
            if (urlToRequestMap.TryGetValue(url, out DownloadRequest req))
            {
                return req.progress;
            }
            return -1f;
        }

        // 检查是否正在下载
        public bool IsDownloading(string url)
        {
            return downloadingUrls.Contains(url);
        }

        // 检查是否在队列中
        public bool IsInQueue(string url)
        {
            return urlToRequestMap.ContainsKey(url);
        }

        // 取消特定下载
        public void CancelDownload(string url)
        {
            if (urlToRequestMap.TryGetValue(url, out DownloadRequest req))
            {
                if (downloadingList.Contains(req))
                {
                    CancelDownload(req, putBackToWaitQueue: false);
                }
                else
                {
                    // 从等待栈或失败队列中移除
                    waitingStack.Remove(req);
                    failedQueue.Remove(req);
                    waitingUrls.Remove(url);
                    failedUrls.Remove(url);
                    urlToRequestMap.Remove(url);
                }
            }
        }

        // 设置最大并发数
        public void SetMaxConcurrent(int count)
        {
            maxConcurrent = Mathf.Max(1, count);
        }

        // 设置最大重试次数
        public void SetMaxRetryCount(int count)
        {
            maxRetryCount = Mathf.Max(0, count);
        }
    }
}


// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;
// namespace VideoSystem
// {
//     public class PriorityDownloadManager : MonoBehaviour
//     {
//         private static PriorityDownloadManager _instance;
//         public static PriorityDownloadManager Instance
//         {
//             get
//             {
//                 if (_instance == null)
//                 {
//                     GameObject go = new GameObject("[PriorityDownloadManager]");
//                     _instance = go.AddComponent<PriorityDownloadManager>();
//                     DontDestroyOnLoad(go);
//                 }
//                 return _instance;
//             }
//         }


//         // 下载请求数据结构
//         private class DownloadRequest
//         {
//             public string url;
//             public string fileId;
//             public Action<string, byte[]> onSuccess;
//             public Action<string> onError;
//             public float progress;
//             public bool isPriority;
//             public int failCount;
//             public UnityWebRequest webRequest;
//             public Coroutine coroutine;
//         }

//         // 等待栈和状态
//         private Stack<DownloadRequest> waitingStack = new Stack<DownloadRequest>();      // 等待栈（最新进入的请求最先执行）
//         private List<DownloadRequest> failedQueue = new List<DownloadRequest>();         // 失败队列（等待空闲重试）
//         private List<DownloadRequest> downloadingList = new List<DownloadRequest>();     // 正在下载中的请求
//         private int maxConcurrent = 2;          // 最大并发数
//         private int maxRetryCount = 1;           // 最大重试次数

//         private bool isProcessingFailedQueue = false;

//         private void Update()
//         {
//             // 尝试启动新的下载（在 Update 中定时检查，避免阻塞协程）
//             TryStartNextDownloads();
//             // 检查失败队列的重试
//             TryProcessFailedQueue();
//         }

//         // 对外接口：添加下载任务（普通优先级）
//         public void AddDownload(string url, string fileId, Action<string, byte[]> onSuccess, Action<string> onError)
//         {
//             AddDownload(url, fileId, onSuccess, onError, isPriority: false);
//         }

//         // 对外接口：添加下载任务（可指定优先级）
//         public void AddDownload(string url, string fileId, Action<string, byte[]> onSuccess, Action<string> onError, bool isPriority)
//         {
//             DownloadRequest newRequest = new DownloadRequest
//             {
//                 url = url,
//                 fileId = fileId,
//                 onSuccess = onSuccess,
//                 onError = onError,
//                 progress = 0f,
//                 isPriority = isPriority,
//                 failCount = 0
//             };

//             if (isPriority)
//             {
//                 // 特殊优先级：打断当前下载（策略：取消进度最小的一个下载）
//                 HandlePriorityRequest(newRequest);
//             }
//             else
//             {
//                 // 普通请求放入等待栈栈顶
//                 waitingStack.Push(newRequest);
//             }

//             TryStartNextDownloads();
//         }

//         // 处理优先级请求：打断一个正在下载且进度最小的任务
//         private void HandlePriorityRequest(DownloadRequest priorityReq)
//         {
//             if (downloadingList.Count == 0)
//             {
//                 waitingStack.Push(priorityReq);
//                 return;
//             }

//             // 找出进度最小的下载任务（用于取消）
//             DownloadRequest toCancel = null;
//             float minProgress = 1f;
//             foreach (var req in downloadingList)
//             {
//                 if (req.progress < minProgress)
//                 {
//                     minProgress = req.progress;
//                     toCancel = req;
//                 }
//             }

//             if (toCancel != null)
//             {
//                 // 取消该任务
//                 CancelDownload(toCancel, putBackToWaitQueue: true);
//             }

//             // 将优先级任务推入等待栈顶（确保下一个被执行）
//             waitingStack.Push(priorityReq);
//         }

//         // 取消下载并可选择是否放回等待队列（保留失败计数和重试逻辑）
//         private void CancelDownload(DownloadRequest req, bool putBackToWaitQueue)
//         {
//             if (req.coroutine != null)
//                 StopCoroutine(req.coroutine);
//             if (req.webRequest != null)
//             {
//                 req.webRequest.Abort();
//                 req.webRequest.Dispose();
//                 req.webRequest = null;
//             }
//             downloadingList.Remove(req);

//             if (putBackToWaitQueue)
//             {
//                 // 放回等待栈顶（不增加失败计数）
//                 waitingStack.Push(req);
//             }
//         }

//         // 尝试启动新下载（根据并发上限）
//         private void TryStartNextDownloads()
//         {
//             while (downloadingList.Count < maxConcurrent && waitingStack.Count > 0)
//             {
//                 DownloadRequest next = waitingStack.Pop();
//                 StartCoroutine(DownloadCoroutine(next));
//             }
//         }

//         // 失败队列处理（空闲时自动重试）
//         private void TryProcessFailedQueue()
//         {
//             if (isProcessingFailedQueue) return;
//             if (downloadingList.Count >= maxConcurrent) return;
//             if (failedQueue.Count == 0) return;

//             StartCoroutine(ProcessFailedQueueCoroutine());
//         }

//         private IEnumerator ProcessFailedQueueCoroutine()
//         {
//             isProcessingFailedQueue = true;

//             // 按失败次数少优先（可选，也可按原顺序）
//             failedQueue.Sort((a, b) => a.failCount.CompareTo(b.failCount));

//             List<DownloadRequest> toRetry = new List<DownloadRequest>(failedQueue);
//             failedQueue.Clear();

//             foreach (var req in toRetry)
//             {
//                 // 等待有空位（while循环不阻塞主线程，但协程内等待一帧）
//                 while (downloadingList.Count >= maxConcurrent)
//                     yield return null;

//                 req.failCount++;  // 标记本次为重试
//                 StartCoroutine(DownloadCoroutine(req));
//             }

//             isProcessingFailedQueue = false;
//         }

//         // 实际下载协程
//         private IEnumerator DownloadCoroutine(DownloadRequest req)
//         {
//             downloadingList.Add(req);
//             using (UnityWebRequest uwr = UnityWebRequest.Get(req.url))
//             {
//                 req.webRequest = uwr;
//                 var operation = uwr.SendWebRequest();

//                 // 监听进度
//                 while (!operation.isDone)
//                 {
//                     req.progress = uwr.downloadProgress;
//                     yield return null;
//                 }

//                 req.progress = 1f;

//                 if (uwr.result == UnityWebRequest.Result.Success)
//                 {
//                     byte[] data = uwr.downloadHandler.data;
//                     req.onSuccess?.Invoke(req.fileId, data);
//                 }
//                 else
//                 {
//                     string errorMsg = $"下载失败 [{req.url}] : {uwr.error}";
//                     Debug.LogError(errorMsg);

//                     // 失败处理：检查重试次数
//                     if (req.failCount < maxRetryCount)
//                     {
//                         // 放入失败队列等待重试
//                         req.webRequest = null; // 避免被 Dispose 重复
//                         req.coroutine = null;
//                         failedQueue.Add(req);
//                     }
//                     else
//                     {
//                         // 超过重试次数，彻底失败回调
//                         req.onError?.Invoke(errorMsg);
//                     }
//                 }
//             }

//             req.webRequest = null;
//             downloadingList.Remove(req);
//             req.coroutine = null;

//             // 尝试继续下载
//             TryStartNextDownloads();
//             TryProcessFailedQueue();
//         }

//         // 可选：清空所有队列（场景卸载时调用）
//         public void ClearAll()
//         {
//             foreach (var req in downloadingList)
//             {
//                 if (req.webRequest != null)
//                     req.webRequest.Abort();
//                 if (req.coroutine != null)
//                     StopCoroutine(req.coroutine);
//             }
//             waitingStack.Clear();
//             failedQueue.Clear();
//             downloadingList.Clear();
//         }
//     }
// }
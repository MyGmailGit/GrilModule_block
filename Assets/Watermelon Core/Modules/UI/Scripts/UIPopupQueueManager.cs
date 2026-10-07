using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    /// <summary>
    /// 弹窗队列管理器 - 按加入顺序依次展示弹窗
    /// </summary>
    public class UIPopupQueueManager : MonoBehaviour
    {
        private static UIPopupQueueManager instance;
        public static UIPopupQueueManager Instance => instance;

        [Header("队列设置")]
        [SerializeField] private bool autoShowNext = true;
        // [SerializeField] private bool clearOnSceneChange = false;

        // 等待展示的弹窗队列
        private Queue<PopupRequest> popupQueue = new Queue<PopupRequest>();

        // 当前正在展示的弹窗
        private IPopupWindow currentPopup;

        // 是否正在展示弹窗
        private bool isShowingPopup = false;

        // 防止重复入队检查
        private HashSet<Type> pendingPopupTypes = new HashSet<Type>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                // if (!clearOnSceneChange)
                //     DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // 监听弹窗关闭事件
            UIController.PopupClosed += OnPopupClosed;
        }

        private void OnDisable()
        {
            UIController.PopupClosed -= OnPopupClosed;
        }

        /// <summary>
        /// 将弹窗加入队列（泛型版本）
        /// </summary>
        public void EnqueuePopup<T>(Action<UIPage> onShow = null, Action onClose = null, bool highPriority = false)
            where T : UIPage, IPopupWindow
        {
            Type popupType = typeof(T);
            EnqueuePopup(popupType, onShow, onClose, highPriority);
        }

        /// <summary>
        /// 将弹窗加入队列（Type版本）
        /// </summary>
        public void EnqueuePopup(Type popupType, Action<UIPage> onShow = null, Action onClose = null, bool highPriority = false)
        {
            if (popupType == null)
            {
                Debug.LogError("[PopupQueueManager] 弹窗类型不能为空");
                return;
            }

            // 检查是否已在队列中或正在显示
            if (pendingPopupTypes.Contains(popupType))
            {
                Debug.LogWarning($"[PopupQueueManager] 弹窗 {popupType.Name} 已在队列中或正在显示，跳过重复入队");
                return;
            }

            var request = new PopupRequest
            {
                PopupType = popupType,
                OnShow = onShow,
                OnClose = onClose
            };

            if (highPriority && popupQueue.Count > 0)
            {
                // 高优先级插入到队首
                var tempQueue = new Queue<PopupRequest>();
                tempQueue.Enqueue(request);
                while (popupQueue.Count > 0)
                {
                    tempQueue.Enqueue(popupQueue.Dequeue());
                }
                popupQueue = tempQueue;
            }
            else
            {
                popupQueue.Enqueue(request);
            }

            pendingPopupTypes.Add(popupType);

            Debug.Log($"[PopupQueueManager] 弹窗 {popupType.Name} 已加入队列，当前队列长度: {popupQueue.Count}");

            // 尝试显示下一个弹窗
            if (autoShowNext)
            {
                TryShowNextPopup();
            }
        }

        /// <summary>
        /// 尝试显示队列中的下一个弹窗
        /// </summary>
        private void TryShowNextPopup()
        {
            if (isShowingPopup || popupQueue.Count == 0)
                return;

            // 检查是否有任何弹窗正在显示
            if (UIController.IsPopupOpened)
                return;

            PopupRequest request = popupQueue.Dequeue();
            pendingPopupTypes.Remove(request.PopupType);

            ShowPopup(request);
        }

        /// <summary>
        /// 显示弹窗
        /// </summary>
        private void ShowPopup(PopupRequest request)
        {
            try
            {
                isShowingPopup = true;

                // 获取或创建弹窗页面
                UIPage page = UIController.GetPage(request.PopupType) as UIPage;
                if (page == null)
                {
                    Debug.LogError($"[PopupQueueManager] 无法获取弹窗 {request.PopupType.Name}");
                    isShowingPopup = false;
                    TryShowNextPopup();
                    return;
                }

                // 确保页面是IPopupWindow
                if (!(page is IPopupWindow popupWindow))
                {
                    Debug.LogError($"[PopupQueueManager] 页面 {request.PopupType.Name} 未实现 IPopupWindow 接口");
                    isShowingPopup = false;
                    TryShowNextPopup();
                    return;
                }

                currentPopup = popupWindow;

                // 触发显示回调
                request.OnShow?.Invoke(page);

                // 显示页面
                UIController.ShowPage(page);

                Debug.Log($"[PopupQueueManager] 显示弹窗 {request.PopupType.Name}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[PopupQueueManager] 显示弹窗时发生错误: {e.Message}");
                isShowingPopup = false;
                TryShowNextPopup();
            }
        }

        /// <summary>
        /// 弹窗关闭事件处理
        /// </summary>
        private void OnPopupClosed(IPopupWindow popupWindow, bool state)
        {
            if (!state) // 关闭状态
            {
                // 查找对应的请求执行关闭回调
                if (currentPopup == popupWindow)
                {
                    // 这里我们无法直接获取请求，可以通过其他方式处理
                    // 建议在弹窗关闭时触发回调
                }

                currentPopup = null;
                isShowingPopup = false;

                Debug.Log("[PopupQueueManager] 弹窗已关闭");

                // 显示下一个弹窗
                if (autoShowNext)
                {
                    TryShowNextPopup();
                }
            }
        }

        /// <summary>
        /// 清空队列（可选项：是否关闭当前弹窗）
        /// </summary>
        public void ClearQueue(bool closeCurrent = false)
        {
            popupQueue.Clear();
            pendingPopupTypes.Clear();

            if (closeCurrent && currentPopup != null)
            {
                // 关闭当前弹窗
                var page = currentPopup as UIPage;
                if (page != null && page.IsPageDisplayed)
                {
                    UIController.HidePage(page);
                }
                currentPopup = null;
                isShowingPopup = false;
            }

            Debug.Log("[PopupQueueManager] 队列已清空");
        }

        /// <summary>
        /// 获取队列长度
        /// </summary>
        public int GetQueueLength() => popupQueue.Count;

        /// <summary>
        /// 检查是否正在显示弹窗
        /// </summary>
        public bool IsShowingPopup() => isShowingPopup;

        /// <summary>
        /// 检查队列是否为空
        /// </summary>
        public bool IsQueueEmpty() => popupQueue.Count == 0;

        /// <summary>
        /// 跳过当前弹窗，直接显示下一个
        /// </summary>
        public void SkipCurrentPopup()
        {
            if (currentPopup != null)
            {
                var page = currentPopup as UIPage;
                if (page != null && page.IsPageDisplayed)
                {
                    UIController.HidePage(page);
                }
            }
        }

        /// <summary>
        /// 弹窗请求数据结构
        /// </summary>
        private class PopupRequest
        {
            public Type PopupType { get; set; }
            public Action<UIPage> OnShow { get; set; }
            public Action OnClose { get; set; }
        }

        private void OnDestroy()
        {
            ClearQueue(true);
            instance = null;
        }
    }
}
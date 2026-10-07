using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
// using DG.Tweening;

namespace UIUtil.UI
{
    public class PageView : ScrollView
    {
        /********************************序列化成员********************************/

        //[SerializeField]
        private float m_minlimitDragTime = 0.03f;   //滑动时间判定阈值最小值，触摸滑动的时长dragTime低于此值时，认为是误操作

        //[SerializeField]
        private float m_maxLimitDragTime = 0.5f;   //滑动时间判定阈值最大值，触摸滑动的时长dragTime低于此值时，直接按滑动方向滚动，高于此值时，会根据中轴校正滚动

        //[SerializeField]
        private float m_limitDragDistance = 100f;    //滑动距离判定阈值，触摸时长较短时，或滑动距离低于此值，则不生效

        //********************************序列化成员********************************//

        /********************************非序列化成员********************************/

        private float moveTime = 0.3f;
        private IScrollViewItemHandler m_mainItem = null;
        private IScrollViewItemHandler m_backupItem = null;

        //private List<IScrollViewItemDataHandler> m_waitAddDataList = new List<IScrollViewItemDataHandler>();

        //********************************非序列化成员********************************//

        /********************************get set********************************/

        public IScrollViewItemHandler mainItem
        {
            private set
            {
                m_mainItem = value;
                if (m_mainItem != null)
                {
                    m_mainItem.isMain = true;
                }
            }
            get { return m_mainItem; }
        }

        public IScrollViewItemHandler backupItem
        {
            private set
            {
                m_backupItem = value;
                if (m_backupItem != null)
                {
                    m_backupItem.isMain = false;
                }
            }
            get { return m_backupItem; }
        }

        //********************************get set********************************//

        /********************************Action********************************/

        public Action<IScrollViewItemHandler, bool> OnBeginScrollToItem;
        public Action<IScrollViewItemHandler, bool> OnFinishScrollToItem;

        //********************************Action********************************//

        /********************************Unity生命周期函数********************************/

        //********************************Unity生命周期函数********************************//

        /********************************public函数********************************/

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (!m_isCanDrag)
            {
                return;
            }
            if (m_isTweening)
            {
                return;
            }
            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!m_isCanDrag)
            {
                return;
            }
            if (m_isTweening)
            {
                return;
            }
            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isCanDrag)
            {
                return;
            }
            if (m_isTweening)
            {
                return;
            }
            base.OnEndDrag(eventData);
            __CheckIsAutoAdjust(eventData.position - eventData.pressPosition, m_dragEndTime - m_dragStartTime);
        }

        public override void Clear()
        {
            base.Clear();
            //m_waitAddDataList.Clear();
        }

        public override void Release()
        {
            base.Release();
            //m_waitAddDataList.Clear();
        }

        public override void ForeachItem(Action<IScrollViewItemHandler> handler)
        {
            if (mainItem != null)
            {
                handler(mainItem);
            }
            if (backupItem != null)
            {
                handler(backupItem);
            }
        }

        public override void RefreshItem()
        {
            if (mainItem != null)
            {
                mainItem.Refresh();
            }
            if (backupItem != null)
            {
                backupItem.Refresh();
            }
        }

        public override void RecycleAllItem()
        {
            if (mainItem != null)
            {
                __ReadyRecycleItem(mainItem);
            }
            if (backupItem != null)
            {
                __ReadyRecycleItem(backupItem);
            }

            mainItem = null;
            backupItem = null;
        }

        public override void AddData(IScrollViewItemDataHandler itemData)
        {
            if (itemData == null)
            {
                return;
            }

            __AddData(itemData);
        }

        public override void ScrollToItem(int index, Action onFinish = null)
        {
            if (m_isTweening)
            {
                return;
            }
            if (0 <= index && index < m_dataList.Count)
            {
                __ScrollToItem(index, moveTime, false, onFinish);
            }
        }

        public override void MoveToItem(int index)
        {
            if (m_isTweening)
            {
                return;
            }
            if (0 <= index && index < m_dataList.Count)
            {
                __MoveToItem(index);
            }
        }

        //********************************public函数********************************//

        /********************************protected函数********************************/

        protected override void OnInit()
        {
            base.OnInit();
            //Log("size = " + m_prefabList[0].rectTransform.rect.size);
        }

        protected override void OnCheckInit()
        {
            if (!isInit)
            {
                LogError("PageView is not Init");
            }
        }

        protected override void OnCalculateItemPos()
        {

        }

        protected override void OnRefresh()
        {
            if (m_dataList == null || m_dataList.Count == 0)
            {
                LogError("PageView Refresh failed, data is wrong");
                return;
            }

            if (mainItem == null)
            {
                InitItem(0);
                return;
            }

            int mainIndex = mainItem.dataIndex;
            if (mainIndex >= m_dataList.Count)
            {
                InitItem(m_dataList.Count - 1);
                return;
            }

            IScrollViewItemDataHandler mainItemData = m_dataList[mainIndex];
            __VerifyMainItem(mainItemData);
            __RefreshItem(mainItem, mainItemData);

            if (backupItem != null)
            {
                int backIndex = backupItem.dataIndex;
                if (backIndex < m_dataList.Count)
                {
                    IScrollViewItemDataHandler backItemData = m_dataList[backIndex];
                    __VerifyBackupItem(backItemData);
                    backupItem.dataIndex = backIndex;
                    __LinkItem(mainItem, backupItem);
                    __RefreshItem(backupItem, backItemData);
                    SetItemAnchoredPositionByLinkItem(backupItem);
                }
                else
                {
                    __ReadyRecycleItem(backupItem);
                }
            }

            //Layout();
        }

        protected override void OnInsertData(int index, IScrollViewItemDataHandler itemData)
        {
            LogError("PageView没有InsertData的功能");
        }

        protected override void OnDeleteData(int index)
        {
            LogError("PageView没有DeleteData的功能");
        }

        protected override void OnBeforeLayout()
        {
            base.OnBeforeLayout();
            UpdateContentSize();
        }

        protected override void OnLayout()
        {
            base.OnLayout();
            if (mainItem == null)
            {
                return;
            }

            if (mainItem.dataIndex == 0)
            {
                mainItem.SetAnchoredPositionByLeftTopPos(new Vector2(paddingLeft, -paddingTop));
            }
            else if (mainItem.dataIndex == m_dataList.Count - 1)
            {
                if (horizontal)
                {
                    mainItem.SetAnchoredPositionByRightTopPos(new Vector2(content.rect.width - paddingRight, -paddingTop));
                }
                else if (vertical)
                {
                    mainItem.SetAnchoredPositionByLeftBottomPos(new Vector2(paddingLeft, paddingBottom - content.rect.height));
                }
            }
            else
            {
                if (horizontal)
                {
                    float x = content.rect.width * 0.5f;
                    float y = paddingTop;
                    mainItem.SetAnchoredPositionByTopMiddlePos(new Vector2(x, y));
                }
                else if (vertical)
                {
                    float x = paddingLeft;
                    float y = 0 - content.rect.height * 0.5f;
                    mainItem.SetAnchoredPositionByLeftMiddlePos(new Vector2(x, y));
                }
            }

            __DoMoveToItem(mainItem);
        }

        protected override void OnAfterLayout()
        {
            base.OnAfterLayout();
        }

        protected override void UpdateContentSize()
        {
            Vector2 size = Vector2.zero;
            if (mainItem != null)
            {
                size = mainItem.rectTransform.rect.size;
            }
            else
            {
                size = viewport.rect.size;
            }

            int count = Mathf.Clamp(m_dataList.Count, 1, 3);

            if (horizontal)
            {
                float x = size.x * count + spaceX * 2 + paddingLeft + paddingRight;
                SetContentWidth(x);
            }
            else if (vertical)
            {
                float y = size.y * count + spaceY * 2 + paddingTop + paddingBottom;
                SetContentHeight(y);
            }
        }

        protected override void OnLateUpdate()
        {
            __CheckItem();
            base.OnLateUpdate();

            if (isAutoScrolling)
            {
                return;
            }
            //__CheckDataIsNeedAdd();
        }

        protected override void __OnScrollUp(bool isCheck)
        {
            if (OnScrollUp != null)
            {
                OnScrollUp(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected override void __OnScrollDown(bool isCheck)
        {
            if (OnScrollDown != null)
            {
                OnScrollDown(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected override void __OnScrollLeft(bool isCheck)
        {
            if (OnScrollLeft != null)
            {
                OnScrollLeft(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected override void __OnScrollRight(bool isCheck)
        {
            if (OnScrollRight != null)
            {
                OnScrollRight(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected void SetItemAnchoredPositionByLinkItem(IScrollViewItemHandler item)
        {
            if (item.preItem != null)
            {
                SetItemAnchoredPositionByPreItem(item);
                return;
            }

            if (item.nextItem != null)
            {
                SetItemAnchoredPositionByNextItem(item);
                return;
            }
        }

        //********************************protected函数********************************//

        /********************************private函数********************************/

        private void InitItem(int startIndex)
        {
            InitItem(startIndex, new Vector2(paddingLeft, -paddingTop));
        }

        private void InitItem()
        {
            InitItem(0, new Vector2(paddingLeft, -paddingTop));
        }

        private void InitItem(int startIndex, Vector2 refPos)
        {
            CheckInit();
            if (m_dataList == null || m_dataList.Count == 0)
            {
                LogError("PageView InitItem failed, data is wrong");
                return;
            }
            if (isAutoScrolling)
            {
                LogWarning("Warning: PageView在滑动中InitItem");
            }
            RecycleAllItem();

            IScrollViewItemDataHandler itemData = m_dataList[startIndex];
            mainItem = __GetItem(itemData.GetPrefabName());
            __ActivateItem(mainItem);
            __RefreshItem(mainItem, itemData);

            Layout();
        }

        private void __VerifyMainItem(IScrollViewItemDataHandler itemData)
        {
            if (mainItem == null)
            {
                mainItem = __GetItem(itemData.GetPrefabName());
                __ActivateItem(mainItem);
            }
            else
            {
                if (mainItem.originName != itemData.GetPrefabName())
                {
                    __ReadyRecycleItem(mainItem);
                    mainItem = __GetItem(itemData.GetPrefabName());
                    __ActivateItem(mainItem);
                }
            }
        }

        private void __VerifyBackupItem(IScrollViewItemDataHandler itemData)
        {
            if (backupItem == null)
            {
                backupItem = __GetItem(itemData.GetPrefabName());
                __ActivateItem(backupItem);
            }
            else
            {
                if (backupItem.originName != itemData.GetPrefabName())
                {
                    __ReadyRecycleItem(backupItem);
                    backupItem = __GetItem(itemData.GetPrefabName());
                    __ActivateItem(backupItem);
                }
            }
        }

        private void __CheckItem()
        {
            if (mainItem == null)
            {
                return;
            }

            Bounds bounds = mainItem.GetViewportBounds();
            bool isAddForward = false;
            bool isAddBack = false;
            bool isCanAddForward = mainItem.dataIndex > 0 && (mainItem.preItem == null || mainItem.preItem != backupItem);
            bool isCanAddBack = mainItem.dataIndex < m_dataList.Count - 1 && (mainItem.nextItem == null || mainItem.nextItem != backupItem);
            if (horizontal)
            {
                float dis = bounds.min.x - m_viewportBounds.min.x;
                if (dis > 1f)
                {
                    isAddForward = isCanAddForward;
                }
                else if (dis < -1f)
                {
                    isAddBack = isCanAddBack;
                }
            }
            else if (vertical)
            {
                float dis = bounds.max.y - m_viewportBounds.max.y;
                if (dis < -1f)
                {
                    isAddForward = isCanAddForward;
                }
                else if (dis > 1f)
                {
                    isAddBack = isCanAddBack;
                }
            }

            int index = 0;
            if (isAddForward)
            {
                index = mainItem.dataIndex - 1;
            }
            else if (isAddBack)
            {
                index = mainItem.dataIndex + 1;
            }
            else
            {
                return;
            }

            IScrollViewItemDataHandler itemData = m_dataList[index];
            __VerifyBackupItem(itemData);
            backupItem.dataIndex = itemData.index;
            __LinkItem(mainItem, backupItem);
            __RefreshItem(backupItem, itemData);
            SetItemAnchoredPositionByLinkItem(backupItem);
        }

        private void __CheckIsAutoAdjust(Vector2 dis, float dragTime)
        {
            //Log("dis =" + dis);

            if (horizontal)
            {
                __HorizontalAutoAdjust(dis, dragTime);
            }
            else if (vertical)
            {
                __VerticalAutoAdjust(dis, dragTime);
            }
        }

        private void __HorizontalAutoAdjust(Vector2 dis, float dragTime)
        {
            bool isScrollRight = dis.x > 1f;
            bool isScrollLeft = dis.x < 1f;
            if (isScrollRight && mainItem.dataIndex <= 0)
            {
                if (backupItem != null)
                {
                    __ReadyRecycleItem(backupItem);
                    backupItem = null;
                }
                return;
            }
            if (isScrollLeft && mainItem.dataIndex >= m_dataList.Count - 1)
            {
                if (backupItem != null)
                {
                    __ReadyRecycleItem(backupItem);
                    backupItem = null;
                }
                return;
            }

            if (dragTime <= m_maxLimitDragTime)
            {
                if (dragTime < m_minlimitDragTime || Mathf.Abs(dis.x) < m_limitDragDistance)
                {
                    //滑动时间超短或者滑动距离也超短，则被认为是误操作
                    __DoAutoScrollToItem(mainItem, dragTime, true);
                }
                else
                {
                    if (isScrollRight)
                    {
                        //右滑
                        __ScrollToItem(mainItem.dataIndex - 1, dragTime, true);
                    }
                    else if (isScrollLeft)
                    {
                        //左滑
                        __ScrollToItem(mainItem.dataIndex + 1, dragTime, true);
                    }
                }
            }
            else
            {
                //滑动时间较长时，根据滑动方向和中轴线校正移动
                Bounds bounds = mainItem.GetViewportBounds();
                if (isScrollRight)
                {
                    //右滑
                    if (bounds.min.x < m_viewportBounds.center.x)
                    {
                        //不用切换
                        __DoAutoScrollToItem(mainItem, moveTime, true);
                    }
                    else
                    {
                        __ScrollToItem(mainItem.dataIndex - 1, moveTime, true);
                    }
                }
                else if (isScrollLeft)
                {
                    //左滑
                    if (bounds.max.x > m_viewportBounds.center.x)
                    {
                        //不用切换
                        __DoAutoScrollToItem(mainItem, moveTime, true);
                    }
                    else
                    {
                        __ScrollToItem(mainItem.dataIndex + 1, moveTime, true);
                    }
                }
            }
        }

        private void __VerticalAutoAdjust(Vector2 dis, float dragTime)
        {
            bool isScrollUp = dis.y > 1f;
            bool isScrollDown = dis.y < 1f;
            if (isScrollDown && mainItem.dataIndex <= 0)
            {
                return;
            }
            if (isScrollUp && mainItem.dataIndex >= m_dataList.Count - 1)
            {
                return;
            }

            if (dragTime <= m_maxLimitDragTime)
            {
                if (dragTime < m_minlimitDragTime || Mathf.Abs(dis.y) < m_limitDragDistance)
                {
                    //滑动时间超短或者滑动距离也超短，则被认为是误操作
                    __DoAutoScrollToItem(mainItem, dragTime, true);
                }
                else
                {
                    if (isScrollDown)
                    {
                        //下滑
                        __ScrollToItem(mainItem.dataIndex - 1, dragTime, true);
                    }
                    else if (isScrollUp)
                    {
                        //上滑
                        __ScrollToItem(mainItem.dataIndex + 1, dragTime, true);
                    }
                }
            }
            else
            {
                //滑动时间较长时，根据滑动方向和中轴线校正移动
                Bounds bounds = mainItem.GetViewportBounds();
                if (isScrollDown)
                {
                    //下滑
                    if (bounds.max.y > m_viewportBounds.center.y)
                    {
                        //不用切换
                        __DoAutoScrollToItem(mainItem, moveTime, true);
                    }
                    else
                    {
                        __ScrollToItem(mainItem.dataIndex - 1, moveTime, true);
                    }
                }
                else if (isScrollUp)
                {
                    //上滑
                    if (bounds.min.y < m_viewportBounds.center.y)
                    {
                        //不用切换
                        __DoAutoScrollToItem(mainItem, moveTime, true);
                    }
                    else
                    {
                        __ScrollToItem(mainItem.dataIndex + 1, moveTime, true);
                    }
                }
            }
        }

        private void __ScrollToItem(int index, float time, bool isAuto, Action onFinish = null)
        {
            if (mainItem == null)
            {
                LogError("__ScrollToItem, mainItem 意外的为 null");
                return;
            }

            if (mainItem.dataIndex == index)
            {
                __DoAutoScrollToItem(mainItem, time, isAuto, onFinish);
                return;
            }

            if (backupItem != null && backupItem.dataIndex == index)
            {
                __DoAutoScrollToItem(backupItem, time, isAuto, onFinish);
                return;
            }
            else
            {
                IScrollViewItemDataHandler itemData = m_dataList[index];
                __VerifyBackupItem(itemData);
                backupItem.dataIndex = itemData.index;
                __LinkItem(mainItem, backupItem);
                __RefreshItem(backupItem, itemData);
                SetItemAnchoredPositionByLinkItem(backupItem);
                __DoAutoScrollToItem(backupItem, time, isAuto, onFinish);
            }
        }

        private void __DoAutoScrollToItem(IScrollViewItemHandler item, float time, bool isAuto, Action onFinish = null)
        {
            if (item == null)
            {
                return;
            }

            StopMovement();

            Bounds bounds = item.GetBounds();
            Vector2 pos = content.anchoredPosition;
            if (horizontal)
            {
                pos.x = 0 - bounds.min.x;
            }
            else if (vertical)
            {
                pos.y = 0 - bounds.max.y;
            }

            if (OnBeginScrollToItem != null)
            {
                OnBeginScrollToItem(item, isAuto);
            }
            time = Mathf.Clamp(time, 0.2f, 0.5f);
            __DoContentScroll(pos, time, () =>
            {
                if (backupItem == item)
                {
                    __ReadyRecycleItem(mainItem);
                    mainItem = backupItem;
                    backupItem = null;
                }
                else
                {
                    if (backupItem != null)
                    {
                        __ReadyRecycleItem(backupItem);
                        backupItem = null;
                    }
                }
                __CutItem(mainItem);
                Layout();

                if (OnFinishScrollToItem != null)
                {
                    OnFinishScrollToItem(mainItem, isAuto);
                }

                if (onFinish != null)
                {
                    onFinish();
                }
            });
        }

        private void __MoveToItem(int index)
        {
            if (mainItem != null && mainItem.dataIndex == index)
            {
                __DoMoveToItem(mainItem);
                return;
            }

            if (backupItem != null && backupItem.dataIndex == index)
            {
                __DoMoveToItem(backupItem);
                return;
            }
            else
            {
                IScrollViewItemDataHandler itemData = m_dataList[index];
                if (mainItem == null)
                {
                    mainItem = __GetItem(itemData.GetPrefabName());
                    __ActivateItem(mainItem);
                }
                else
                {
                    if (mainItem.originName != itemData.GetPrefabName())
                    {
                        __ReadyRecycleItem(mainItem);
                        mainItem = __GetItem(itemData.GetPrefabName());
                        __ActivateItem(mainItem);
                    }
                }
                __ReadyRecycleItem(backupItem);
                __RefreshItem(mainItem, itemData);
                Layout();
            }
        }

        private void __DoMoveToItem(IScrollViewItemHandler item)
        {
            Bounds bounds = item.GetBounds();
            Vector2 pos = content.anchoredPosition;
            if (horizontal)
            {
                pos.x = 0 - bounds.min.x;
            }
            else if (vertical)
            {
                pos.y = 0 - bounds.max.y;
            }

            SetContentAnchoredPosition(pos);
        }

        //private void __CheckDataIsNeedAdd()
        //{
        //    for (int i = 0; i < m_waitAddDataList.Count; ++i)
        //    {
        //        __AddData(m_waitAddDataList[i]);
        //    }
        //    m_waitAddDataList.Clear();
        //}

        private void __LinkItem(IScrollViewItemHandler item1, IScrollViewItemHandler item2)
        {
            if (item1 == null || item2 == null)
            {
                LogWarning("Pageview.cs => __LinkItem failed , item is null");
                return;
            }
            if (item1 == item2 || item1.dataIndex == item2.dataIndex)
            {
                LogWarning("Pageview.cs => __LinkItem failed");
                return;
            }
            __CutItem(item1);
            __CutItem(item2);

            if (item1.dataIndex > item2.dataIndex)
            {
                item1.preItem = item2;
                item2.nextItem = item1;
            }
            else if (item1.dataIndex < item2.dataIndex)
            {
                item1.nextItem = item2;
                item2.preItem = item1;
            }
        }

        private IScrollViewItemHandler __FindItem(int index)
        {
            if (mainItem != null && mainItem.dataIndex == index)
            {
                return mainItem;
            }
            else if (backupItem != null && backupItem.dataIndex == index)
            {
                return backupItem;
            }

            return null;
        }

        //********************************private函数********************************//

        /********************************Animation********************************/

        //********************************Animation********************************//
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PageView))]
    public class PageViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using Watermelon;
using DG.Tweening;

namespace UIUtil.UI
{
    public class ScrollView : ScrollRect
    {
        /********************************序列化成员********************************/

        [Space(5)]
        [Header("以下是新增属性")]
        [Space(5)]

        [SerializeField]
        protected bool m_debugLog = false;

        [SerializeField]
        protected bool m_isCanDrag = true;

        [SerializeField]
        protected RectTransform m_subContent;

        public RectTransform rect_prefabPool;
        public RectTransform rect_itemPool;

        [SerializeField]
        protected List<GameObject> m_prefabList;

        [SerializeField]
        protected float m_dragX = 0;

        [SerializeField]
        protected float m_dragY = 0;

        [SerializeField]
        protected RectOffset m_padding = new RectOffset();

        [SerializeField]
        protected Vector2 m_space;

        //********************************序列化成员********************************//

        /********************************非序列化protected成员********************************/

        protected bool m_isTriggerScroll;

        protected bool m_isTriggerParentScroll;

        protected bool m_isDrag = false;

        protected bool m_isTweening = false;

        protected float m_dragStartTime = 0;

        protected float m_dragEndTime = 0;

        protected Vector2 m_preTouchPos;

        protected Vector2 m_contentPreAnchoredPosition;

        protected List<IScrollViewItemHandler> m_waitRecycleItemList = new List<IScrollViewItemHandler>();

        protected List<IScrollViewItemHandler> m_itemPoolList = new List<IScrollViewItemHandler>();

        protected List<IScrollViewItemDataHandler> m_dataList = new List<IScrollViewItemDataHandler>();

        protected Bounds m_viewportBounds;

        protected Canvas m_rootCanvas;

        protected Vector2 m_canvasSize;

        protected Vector2 m_scale_Canvas_Screen;

        protected IScrollViewItemHandler m_firstItem;     //第一个Item

        protected IScrollViewItemHandler m_lastItem;      //最后一个Item

        protected Tween m_seq;

        //********************************非序列化protected成员********************************//

        /********************************非序列化private成员********************************/

        private bool m_isInit = false;

        //********************************非序列化private成员********************************//

        /********************************get set********************************/

        public bool isInit
        {
            get { return m_isInit; }
        }

        public bool isTriggerScroll
        {
            get { return m_isTriggerScroll; }
        }

        public bool isTriggerParentScroll
        {
            get { return m_isTriggerParentScroll; }
        }

        public virtual bool isAutoScrolling
        {
            get { return velocity != Vector2.zero || m_isTweening; }
        }

        public bool isTweening
        {
            get { return m_isTweening; }
        }

        public bool isDrag
        {
            get { return m_isDrag; }
        }

        public bool isCanDrag
        {
            set { m_isCanDrag = value; }
            get { return m_isCanDrag; }
        }

        public RectTransform itemPool
        {
            get { return rect_itemPool; }
        }

        public float dragX
        {
            set { m_dragX = value; }
            get { return m_dragX; }
        }

        public float dragY
        {
            set { m_dragY = value; }
            get { return m_dragY; }
        }

        public virtual int DataCount
        {
            get
            {
                if (m_dataList == null)
                {
                    return 0;
                }
                return m_dataList.Count;
            }
        }

        public virtual RectOffset padding
        {
            set { m_padding = value; }
            get { return m_padding; }
        }

        public virtual int paddingLeft
        {
            set { m_padding.left = value; }
            get { return m_padding.left; }
        }

        public virtual int paddingTop
        {
            set { m_padding.top = value; }
            get { return m_padding.top; }
        }

        public virtual int paddingRight
        {
            set { m_padding.right = value; }
            get { return m_padding.right; }
        }

        public virtual int paddingBottom
        {
            set { m_padding.bottom = value; }
            get { return m_padding.bottom; }
        }

        public virtual Vector2 space
        {
            set { m_space = value; }
            get { return m_space; }
        }

        public virtual float spaceX
        {
            set { m_space.x = value; }
            get { return m_space.x; }
        }

        public virtual float spaceY
        {
            set { m_space.y = value; }
            get { return m_space.y; }
        }

        public virtual IScrollViewItemHandler FirstItem
        {
            get { return m_firstItem; }
            protected set { m_firstItem = value; }
        }

        public virtual IScrollViewItemHandler LastItem
        {
            get { return m_lastItem; }
            protected set { m_lastItem = value; }
        }

        public virtual RectTransform SubContent
        {
            get { return m_subContent; }
        }

        //********************************get set********************************//

        /********************************Action********************************/

        public Action<Vector2, PointerEventData> OnTouchScrollBegin;              //参数1是触摸点开始始的位置
        public Action<Vector2, Vector2, PointerEventData> OnTouchScroll;          //参数1是上一个触摸点的位置，参数2是当前触摸点的位置
        public Action<Vector2, Vector2, PointerEventData> OnTouchScrollEnd;       //参数1是上一个触摸点的位置，参数2是当前触摸点的位置
        public Action<bool, Vector2, Vector2> OnScrolling;            //触摸滑动或自动滚动时触发，参数1:true(触摸滚动), false(自动滚动)
        public Action<Vector2, Vector2> OnValueChanged;         //参数1是content上一次的anchorPosition，参数2是content当前anchorPosition
        public Action<Vector2, Vector2> OnScrollUp;             //参数1是content上一次的anchorPosition，参数2是content当前anchorPosition
        public Action<Vector2, Vector2> OnScrollDown;           //参数1是content上一次的anchorPosition，参数2是content当前anchorPosition
        public Action<Vector2, Vector2> OnScrollLeft;           //参数1是content上一次的anchorPosition，参数2是content当前anchorPosition
        public Action<Vector2, Vector2> OnScrollRight;          //参数1是content上一次的anchorPosition，参数2是content当前anchorPosition
        public Action<IScrollViewItemHandler> OnActivateItem;
        public Action<IScrollViewItemHandler> OnRecycleItem;
        public Action<IScrollViewItemHandler> OnBeforeRefreshItem;
        public Action<IScrollViewItemHandler> OnAfterRefreshItem;
        public Action<MoveDirection> OnReuseItem;

        //********************************Action********************************//

        /********************************Unity生命周期函数********************************/

        protected override void Awake()
        {
            base.Awake();
        }

        protected sealed override void LateUpdate()
        {
            base.LateUpdate();
            OnLateUpdate();
            __CheckWaitRecycleItem();
        }

        //此函数非Unity生命周期函数
        protected virtual void OnLateUpdate()
        {
            if (content == null)
            {
                return;
            }

            Vector2 dis = content.anchoredPosition - m_contentPreAnchoredPosition;
            bool isCheck = false;

            if (vertical)
            {
                isCheck = Mathf.Abs(dis.y) < viewport.rect.height;
                if (!isCheck)
                {
                    //位移的跨度过大时，直接Refresh
                    Refresh();
                }
                if (dis.y > 0.1f)
                {
                    __OnScrollUp(isCheck);
                }
                else if (dis.y < -0.1f)
                {
                    __OnScrollDown(isCheck);
                }
            }

            if (horizontal)
            {
                isCheck = Mathf.Abs(dis.x) < viewport.rect.width;
                if (!isCheck)
                {
                    //位移的跨度过大时，直接Refresh
                    Refresh();
                }
                if (dis.x > 0.1f)
                {
                    __OnScrollRight(isCheck);
                }
                else if (dis.x < -0.1f)
                {
                    __OnScrollLeft(isCheck);
                }
            }

            if (m_subContent != null && m_subContent.gameObject.activeSelf)
            {
                m_subContent.anchoredPosition = content.anchoredPosition;
            }

            if (OnValueChanged != null)
            {
                OnValueChanged(m_contentPreAnchoredPosition, content.anchoredPosition);
            }

            //if (m_isTweening)
            //{
            //    float x = m_contentPreAnchoredPosition.x - content.anchoredPosition.x;
            //    Debug.Log(string.Format("pos = {0} , pos = {1}, dis = {2}", m_contentPreAnchoredPosition, content.anchoredPosition, -x));
            //}

            if (isDrag || isAutoScrolling)
            {
                if (OnScrolling != null)
                {
                    OnScrolling(isDrag, m_contentPreAnchoredPosition, content.anchoredPosition);
                }
            }

            m_contentPreAnchoredPosition = content.anchoredPosition;
        }

        //********************************Unity生命周期函数********************************//

        /********************************public函数********************************/

        #region 滑动事件

        protected void DoBaseOnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
        }

        protected void DoBaseOnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
        }

        protected void DoBaseOnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
        }

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
            m_isDrag = true;
            m_dragStartTime = Time.time;
            base.OnBeginDrag(eventData);
            TouchScrollBegin(eventData.pressPosition, eventData);
            m_preTouchPos = eventData.pressPosition;

            __DoParentBeginDrag(eventData);
            m_isTriggerScroll = false;
            m_isTriggerParentScroll = false;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!m_isDrag)
            {
                return;
            }

            if (m_isTweening)
            {
                return;
            }

            Vector2 drag = TransformScreenVectorToCanvasVector(eventData.position - eventData.pressPosition);

            drag.x = Mathf.Abs(drag.x);
            drag.y = Mathf.Abs(drag.y);

            bool isVertical = drag.y > drag.x;
            if (m_isTriggerScroll)
            {
                base.OnDrag(eventData);
                TouchScroll(m_preTouchPos, eventData.position, eventData);
            }
            else if (m_isTriggerParentScroll)
            {
                __DoParentDrag(eventData);
            }
            else
            {
                if (isVertical && vertical)
                {
                    if (drag.y >= m_dragY)
                    {
                        //判定上下滑动还是左右滑动
                        base.OnDrag(eventData);
                        TouchScroll(m_preTouchPos, eventData.position, eventData);
                        m_isTriggerScroll = true;
                    }
                }
                else if (!isVertical && horizontal)
                {
                    if (drag.x >= m_dragX)
                    {
                        //判定上下滑动还是左右滑动
                        base.OnDrag(eventData);
                        TouchScroll(m_preTouchPos, eventData.position, eventData);
                        m_isTriggerScroll = true;
                    }
                }
                else
                {
                    __DoParentDrag(eventData);
                    __CheckParentScroll();
                }
            }

            m_preTouchPos = eventData.position;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!m_isDrag)
            {
                return;
            }

            if (m_isTweening)
            {
                return;
            }

            m_dragEndTime = Time.time;
            base.OnEndDrag(eventData);
            TouchScrollEnd(m_preTouchPos, eventData.position, eventData);
            m_preTouchPos = eventData.position;
            m_isDrag = false;

            if (!m_isTriggerScroll)
            {
                __DoParentEndDrag(eventData);
            }
            m_isTriggerScroll = false;
            m_isTriggerParentScroll = false;
        }

        #endregion

        public void Init()
        {
            if (m_isInit)
            {
                return;
            }
            OnInit();
            m_isInit = true;
        }

        public void CheckInit()
        {
            OnCheckInit();
        }

        public void Refresh(bool isClamp = true)
        {
            CheckInit();
            OnCalculateItemPos();
            if (isClamp)
            {
                ClampContentPosition();
            }
            OnRefresh();
        }

        public void ClampContentPosition()
        {
            Vector2 pos = GetClampContentPosition(content.anchoredPosition);
            SetContentAnchoredPosition(pos);
        }

        public Vector2 GetClampContentPosition(Vector2 pos)
        {
            if (horizontal)
            {
                if (pos.x > 0)
                {
                    pos.x = 0;
                }
                else if (pos.x < viewport.rect.width - content.rect.width)
                {
                    pos.x = viewport.rect.width - content.rect.width;
                }
            }
            else if (vertical)
            {
                if (pos.y < 0)
                {
                    pos.y = 0;
                }
                else if (pos.y > content.rect.height - viewport.rect.height)
                {
                    pos.y = content.rect.height - viewport.rect.height;
                }
            }
            return pos;
        }

        //计算所有Item位置
        public void CalculateItemPos()
        {
            OnCalculateItemPos();
        }

        public void InsertData(int index, IScrollViewItemDataHandler itemData)
        {
            OnInsertData(index, itemData);
            Refresh();
        }

        public void DeleteData(int index)
        {
            OnDeleteData(index);
            Refresh();
        }

        public virtual IScrollViewItemDataHandler GetDataByIndex(int index)
        {
            if (index >= 0 && index < m_dataList.Count)
            {
                return m_dataList[index];
            }
            return null;
        }

        public void ResetContentAnchoredPosition()
        {
            SetContentAnchoredPosition(Vector2.zero);
        }

        public virtual void SetContentPosition(Vector2 pos, bool isClamp = true)
        {
            if (isClamp)
            {
                pos = GetClampContentPosition(pos);
                SetContentAnchoredPosition(pos);
            }
            else
            {
                content.anchoredPosition = pos;
            }
        }

        public virtual void ForeachItem(Action<IScrollViewItemHandler> handler)
        {
            IScrollViewItemHandler item = m_firstItem;

            while (item != null)
            {
                handler(item);
                item = item.nextItem;
            }
        }

        public virtual IScrollViewItemHandler FindItem(object para)
        {
            IScrollViewItemHandler item = m_firstItem;

            while (item != null)
            {
                if (item.IsMatch(para))
                {
                    return item;
                }
                item = item.nextItem;
            }
            return null;
        }
        public virtual IScrollViewItemHandler FindItem(int index)
        {
            IScrollViewItemHandler item = m_firstItem;
            while (item != null)
            {
                if (item.dataIndex == index)
                {
                    return item;
                }
                item = item.nextItem;
            }
            return null;
        }

        public virtual void RefreshItem()
        {
            IScrollViewItemHandler item = m_firstItem;

            while (item != null)
            {
                item.Refresh();
                item = item.nextItem;
            }
        }

        public void ReleasePool()
        {
            foreach (var item in m_itemPoolList)
            {
                item.Release();
            }
            m_itemPoolList.Clear();

            foreach (var item in m_waitRecycleItemList)
            {
                item.Release();
            }
            m_waitRecycleItemList.Clear();
        }

        public virtual void ClearData()
        {
            m_dataList.Clear();
        }

        public virtual void Clear()
        {
            RecycleAllItem();
            m_dataList.Clear();
        }

        public virtual void Release()
        {
            RecycleAllItem();
            ReleasePool();
            m_dataList.Clear();
        }

        public virtual void RecycleAllItem()
        {
            IScrollViewItemHandler item = m_firstItem;
            IScrollViewItemHandler nextItem = null;

            while (item != null)
            {
                nextItem = item.nextItem;
                __ReadyRecycleItem(item);
                item = nextItem;
            }

            m_firstItem = null;
            m_lastItem = null;
        }

        //获取第一项的左上角的位置
        public Vector2 GetFirstItemLeftTopPos()
        {
            if (m_firstItem != null)
            {
                Bounds bounds = m_firstItem.GetBounds();
                return new Vector2(bounds.min.x, bounds.max.y);
            }
            return new Vector2(paddingLeft, -paddingTop);
        }

        public int GetFirstItemIndex()
        {
            if (m_firstItem != null)
            {
                return m_firstItem.dataIndex;
            }
            return 0;
        }

        public virtual void AddData(IScrollViewItemDataHandler itemData)
        {
            __AddData(itemData);
        }

        public virtual void StopScroll(bool isInvokeComplete)
        {
            __StopScrollTweening(isInvokeComplete);
        }

        public virtual void ScrollToItem(int index, Action onFinish = null)
        {
            ScrollToItem(index, 0.5f, onFinish);
        }

        public virtual void ScrollToItem(int index, float time, Action onFinish = null)
        {
            if (m_isTweening)
            {
                return;
            }
            if (0 <= index && index < m_dataList.Count)
            {
                __ScrollToItem(m_dataList[index], time, onFinish);
            }
        }

        public virtual void ScrollToPos(Vector2 pos, float time, Action onFinish = null)
        {
            if (m_isTweening)
            {
                return;
            }

            if (horizontal)
            {
                __DoContentScroll(new Vector2(pos.x, content.anchoredPosition.y), time, onFinish);
            }
            else if (vertical)
            {
                __DoContentScroll(new Vector2(content.anchoredPosition.x, pos.y), time, onFinish);
            }
        }

        public virtual void MoveToItem(int index)
        {
            if (m_isTweening)
            {
                return;
            }
            if (0 <= index && index < m_dataList.Count)
            {
                __MoveToItem(m_dataList[index]);
            }
        }

        public virtual void InitContent()
        {
            //设置anchorMin和anchorMax会使rect产生变化，所以这里保存一下
            Vector2 size = content.rect.size;

            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;
            content.pivot = Vector2.up;
            content.anchoredPosition = Vector2.zero;

            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public virtual void InitViewport()
        {
            Vector2 size = new Vector2(viewport.rect.width, viewport.rect.height);
            Vector2 center = viewport.anchoredPosition;
            center.x += size.x / 2;
            center.y -= size.y / 2;

            m_viewportBounds = new Bounds(center, size);
        }

        public virtual void SetContentWidth(float width)
        {
            width = Mathf.Max(width, viewport.rect.width);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            if (m_subContent != null)
            {
                m_subContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
        }

        public virtual void SetContentHeight(float height)
        {
            height = Mathf.Max(height, viewport.rect.height);
            content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            if (m_subContent != null)
            {
                m_subContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            }
        }

        public void Layout()
        {
            OnBeforeLayout();
            OnLayout();
            OnAfterLayout();
        }

        public void RefreshDataIndex()
        {
            if (m_dataList == null)
            {
                return;
            }
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                m_dataList[i].index = i;
            }
        }

        //********************************public函数********************************//

        /********************************protected函数********************************/

        protected virtual void OnInit()
        {
            if (horizontal && vertical)
            {
                LogError("ScrollView can't have double direction, please set single direction");
            }
            this.CheckCanvas();
            this.InitViewport();
            this.InitContent();
            this.CheckPrefab();
            //this.CheckSubContent();   //不需要CheckSubContent
        }

        protected virtual void OnCheckInit()
        {
            if (!isInit)
            {
                LogError("ScrollView is not Init");
            }
        }

        protected virtual void CheckCanvas()
        {
            Canvas canvas = null;
            Transform p = this.transform;
            while (p != null)
            {
                canvas = p.GetComponent<Canvas>();
                if (canvas != null)
                {
                    m_rootCanvas = canvas.rootCanvas;
                    break;
                }
                p = p.parent;
            }

            if (m_rootCanvas != null)
            {
                Rect rect = m_rootCanvas.pixelRect;
                float width = rect.width / m_rootCanvas.scaleFactor;
                float height = rect.height / m_rootCanvas.scaleFactor;
                m_canvasSize = new Vector2(width, height);
                m_scale_Canvas_Screen.x = m_canvasSize.x / Screen.width;
                m_scale_Canvas_Screen.y = m_canvasSize.y / Screen.height;
            }
            else
            {
                m_canvasSize = new Vector2(Screen.width, Screen.height);
                m_scale_Canvas_Screen = Vector2.one;
            }
        }

        protected virtual void CheckPrefab()
        {
            if (m_prefabList.Count == 0)
            {
                for (int i = 0; i < rect_prefabPool.childCount; ++i)
                {
                    Transform child = rect_prefabPool.GetChild(i);
                    if (child != null)
                    {
                        m_prefabList.Add(child.gameObject);
                    }
                }
                rect_prefabPool.gameObject.SetActive(false);
            }

            if (m_prefabList.Count == 0)
            {
                LogWarning("ScrollView => prefabList is empty");
            }
        }

        //protected virtual void CheckSubContent()
        //{
        //    if (m_subContent != null)
        //    {
        //        m_subContent.anchorMin = Vector2.up;
        //        m_subContent.anchorMax = Vector2.up;
        //        m_subContent.pivot = Vector2.up;
        //        m_subContent.anchoredPosition = Vector2.zero;
        //    }
        //}

        protected virtual void OnBeforeLayout()
        {

        }

        protected virtual void OnLayout()
        {
            if (content.childCount <= 0)
            {
                return;
            }
        }

        protected virtual void OnAfterLayout() { }

        protected virtual void UpdateContentSize()
        {
            if (m_dataList == null || m_dataList.Count == 0)
            {
                return;
            }

            IScrollViewItemDataHandler itemData = m_dataList[m_dataList.Count - 1];
            Vector2 leftTopPos = itemData.presetPos;
            Vector2 size = __GetSize(itemData);
            if (horizontal)
            {
                float x = leftTopPos.x + size.x + paddingRight;
                SetContentWidth(x);
            }
            else if (vertical)
            {
                float y = Mathf.Abs(leftTopPos.y - size.y) + paddingBottom;
                SetContentHeight(y);
            }
        }

        protected virtual void TouchScrollBegin(Vector2 curTouchPos, PointerEventData pointerEvent)
        {
            if (OnTouchScrollBegin != null)
            {
                OnTouchScrollBegin(curTouchPos, pointerEvent);
            }
        }

        protected virtual void TouchScroll(Vector2 preTouchPos, Vector2 curTouchPos, PointerEventData pointerEvent)
        {
            if (OnTouchScroll != null)
            {
                OnTouchScroll(preTouchPos, curTouchPos, pointerEvent);
            }
        }

        protected virtual void TouchScrollEnd(Vector2 preTouchPos, Vector2 curTouchPos, PointerEventData pointerEvent)
        {
            if (OnTouchScrollEnd != null)
            {
                OnTouchScrollEnd(preTouchPos, curTouchPos, pointerEvent);
            }
        }

        protected virtual void __OnScrollUp(bool isCheck)
        {
            if (isCheck)
            {
                __CheckFirstItemIsNeedRecycle();
                bool isReuse = __CheckLastItemIsNeedUpdate();
                if (isReuse)
                {
                    if (OnReuseItem != null)
                    {
                        OnReuseItem(MoveDirection.Up);
                    }
                }
            }

            if (OnScrollUp != null)
            {
                OnScrollUp(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected virtual void __OnScrollDown(bool isCheck)
        {
            if (isCheck)
            {
                __CheckLastItemIsNeedRecycle();
                bool isReuse = __CheckFirstItemIsNeedUpdate();
                if (isReuse)
                {
                    if (OnReuseItem != null)
                    {
                        OnReuseItem(MoveDirection.Down);
                    }
                }
            }

            if (OnScrollDown != null)
            {
                OnScrollDown(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected virtual void __OnScrollLeft(bool isCheck)
        {
            if (isCheck)
            {
                __CheckFirstItemIsNeedRecycle();
                bool isReuse = __CheckLastItemIsNeedUpdate();
                if (isReuse)
                {
                    if (OnReuseItem != null)
                    {
                        OnReuseItem(MoveDirection.Left);
                    }
                }
            }

            if (OnScrollLeft != null)
            {
                OnScrollLeft(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected virtual void __OnScrollRight(bool isCheck)
        {
            if (isCheck)
            {
                __CheckLastItemIsNeedRecycle();
                bool isReuse = __CheckFirstItemIsNeedUpdate();
                if (isReuse)
                {
                    if (OnReuseItem != null)
                    {
                        OnReuseItem(MoveDirection.Right);
                    }
                }
            }

            if (OnScrollRight != null)
            {
                OnScrollRight(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }

        protected void SetItemAnchoredPositionByPreItem(IScrollViewItemHandler item)
        {
            if (item.preItem == null)
            {
                return;
            }

            Bounds bounds = item.preItem.GetBounds();

            if (horizontal)
            {
                float x = bounds.max.x + spaceX;
                float y = bounds.max.y;
                item.SetAnchoredPositionByLeftTopPos(new Vector2(x, y));
            }
            else if (vertical)
            {
                float x = bounds.min.x;
                float y = bounds.min.y - spaceY;
                item.SetAnchoredPositionByLeftTopPos(new Vector2(x, y));
            }
        }

        protected void SetItemAnchoredPositionByNextItem(IScrollViewItemHandler item)
        {
            if (item.nextItem == null)
            {
                return;
            }

            Bounds bounds = item.nextItem.GetBounds();
            if (horizontal)
            {
                float x = bounds.min.x - spaceX;
                float y = bounds.max.y;
                item.SetAnchoredPositionByRightTopPos(new Vector2(x, y));
            }
            else if (vertical)
            {
                float x = bounds.min.x;
                float y = bounds.max.y + spaceY;
                item.SetAnchoredPositionByLeftBottomPos(new Vector2(x, y));
            }
        }

        protected virtual void OnCalculateItemPos()
        {
            if (m_dataList == null || m_dataList.Count == 0)
            {
                return;
            }

            if (horizontal)
            {
                OnCalculateItemPosByHorizontal();
            }
            else if (vertical)
            {
                OnCalculateItemPosByVertical();
            }

            UpdateContentSize();
        }

        protected virtual void OnCalculateItemPosByHorizontal()
        {
            Vector2 curPos = new Vector2(paddingLeft, 0 - paddingTop);
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                IScrollViewItemDataHandler data = m_dataList[i];
                data.presetPos = curPos;
                Vector2 size = __GetSize(data);
                curPos.x += size.x + spaceX;
            }
        }

        protected virtual void OnCalculateItemPosByVertical()
        {
            Vector2 curPos = new Vector2(paddingLeft, 0 - paddingTop);
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                IScrollViewItemDataHandler data = m_dataList[i];
                data.presetPos = curPos;
                Vector2 size = __GetSize(data);
                curPos.y -= size.y + spaceY;
            }
        }

        protected virtual void OnRefresh()
        {
            RecycleAllItem();

            if (m_dataList == null)
            {
                LogError("ScrollView => __Refresh fail, data is wrong");
                return;
            }

            if (m_dataList.Count == 0)
            {
                LogError("ScrollView => __Refresh fail, data length is 0");
                return;
            }

            Rect viewRect = new Rect(new Vector2(m_viewportBounds.min.x, m_viewportBounds.min.y), m_viewportBounds.size);
            Vector2 startPos = new Vector2(paddingLeft, 0 - paddingTop);
            Vector2 curPos = startPos;
            Vector2 contentAnchorPos = content.anchoredPosition;
            Vector2 size, viewLeftTopPos, viewRightBottomPos, temp;

            IScrollViewItemHandler item;
            IScrollViewItemDataHandler itemData;

            for (int i = 0; i < m_dataList.Count; ++i)
            {
                itemData = m_dataList[i];
                size = __GetSize(itemData);

                if (itemData.IsPresetPosValid())
                {
                    curPos = itemData.presetPos;
                }

                viewLeftTopPos = curPos + contentAnchorPos;
                viewRightBottomPos.x = viewLeftTopPos.x + size.x;
                viewRightBottomPos.y = viewLeftTopPos.y - size.y;

                if (__IsRectCrossRect(viewLeftTopPos, viewRightBottomPos, viewRect))
                {
                    item = __GetItem(itemData.GetPrefabName());
                    __ActivateItem(item);
                    __RefreshItem(item, m_dataList[i]);

                    item.SetAnchoredPositionByLeftTopPos(curPos);

                    if (m_firstItem == null)
                    {
                        m_firstItem = item;
                    }

                    if (m_lastItem != null)
                    {
                        __AppendLastItem(item);
                    }
                    else
                    {
                        m_lastItem = item;
                    }
                }

                if (horizontal)
                {
                    curPos.x += size.x + space.x;
                    temp.x = curPos.x + contentAnchorPos.x;
                    if (temp.x > viewRect.max.x)
                    {
                        break;
                    }
                }
                else if (vertical)
                {
                    curPos.y -= size.y + space.y;
                    temp.y = curPos.y + contentAnchorPos.y;
                    if (temp.y < viewRect.min.y)
                    {
                        break;
                    }
                }
            }
        }

        protected IScrollViewItemHandler __CreateItem(string prefabName)
        {
            IScrollViewItemHandler item = null;
            foreach (var prefab in m_prefabList)
            {
                if (prefab.name == prefabName)
                {
                    GameObject go = Instantiate(prefab, rect_itemPool);
                    item = go.GetComponent<IScrollViewItemHandler>();
                    if (item == null)
                    {
                        break;
                    }
                    item.Create(prefabName);
                    item.Init();
                    return item;
                }
            }

            if (item == null)
            {
                LogError(string.Format("ScrollView 创建 Item 失败, 没有找到名称为{0}并且挂载了IScrollViewItemHandler组件的prefab", prefabName));
            }

            return item;
        }

        protected IScrollViewItemHandler __GetItem(string prefabName)
        {
            IScrollViewItemHandler item = null;

            for (int i = 0; i < m_waitRecycleItemList.Count; ++i)
            {
                if (m_waitRecycleItemList[i].originName == prefabName)
                {
                    item = m_waitRecycleItemList[i];
                    m_waitRecycleItemList.RemoveAt(i);
                    break;
                }
            }

            if (item == null)
            {
                for (int i = 0; i < m_itemPoolList.Count; ++i)
                {
                    if (m_itemPoolList[i].originName == prefabName)
                    {
                        item = m_itemPoolList[i];
                        m_itemPoolList.RemoveAt(i);
                        break;
                    }
                }
            }

            if (item == null)
            {
                item = __CreateItem(prefabName);
            }

            return item;
        }

        protected void __ActivateItem(IScrollViewItemHandler item, int siblingIndex = -1)
        {
            if (item == null)
            {
                return;
            }

            item.status = ItemStatus.Using;
            if (item.rectTransform.parent != content)
            {
                item.rectTransform.SetParent(content);
            }

            if (siblingIndex >= 0)
            {
                item.rectTransform.SetSiblingIndex(siblingIndex);
            }
            else
            {
                item.rectTransform.SetSiblingIndex(content.childCount);
            }

            item.Activate();

            if (OnActivateItem != null)
            {
                OnActivateItem(item);
            }
        }

        protected void __ReadyRecycleItem(IScrollViewItemHandler item)
        {
            if (item == null)
            {
                return;
            }

            if (rect_itemPool == null)
            {
                LogError("ScrollView itemPool is null");
            }

            __CutItem(item);
            item.status = ItemStatus.WaitRecycle;
            item.MarkDirty();
            m_waitRecycleItemList.Add(item);
        }

        protected void __CheckWaitRecycleItem()
        {
            for (int i = 0; i < m_waitRecycleItemList.Count; ++i)
            {
                m_waitRecycleItemList[i].status = ItemStatus.Standby;
                m_waitRecycleItemList[i].objName = m_waitRecycleItemList[i].originName;
                m_waitRecycleItemList[i].rectTransform.SetParent(rect_itemPool);
                m_waitRecycleItemList[i].Recycle();
                m_itemPoolList.Add(m_waitRecycleItemList[i]);

                if (OnRecycleItem != null)
                {
                    OnRecycleItem(m_waitRecycleItemList[i]);
                }
            }
            m_waitRecycleItemList.Clear();
        }

        protected void __CutItem(IScrollViewItemHandler item)
        {
            if (item == null)
            {
                return;
            }

            if (item.preItem != null)
            {
                item.preItem.nextItem = item.nextItem;
            }

            if (item.nextItem != null)
            {
                item.nextItem.preItem = item.preItem;
            }

            item.preItem = null;
            item.nextItem = null;
        }

        protected void __RefreshItem(IScrollViewItemHandler item, IScrollViewItemDataHandler itemData)
        {
            if (OnBeforeRefreshItem != null)
            {
                OnBeforeRefreshItem(item);
            }

            if (itemData.IsSizeValid())
            {
                item.SetSize(itemData.size);
            }
            item.RefreshData(itemData.index, itemData.GetData());

            if (OnAfterRefreshItem != null)
            {
                OnAfterRefreshItem(item);
            }
        }

        protected Vector2 __GetSize(int dataIndex)
        {
            if (m_dataList == null)
            {
                return Vector2.zero;
            }

            if (dataIndex >= 0 && dataIndex < m_dataList.Count)
            {
                return __GetSize(m_dataList[dataIndex]);
            }
            return Vector2.zero;
        }

        protected Vector2 __GetSize(IScrollViewItemDataHandler itemData)
        {
            if (itemData == null)
            {
                return Vector2.zero;
            }

            if (itemData.IsSizeValid())
            {
                return itemData.size;
            }

            foreach (var prefab in m_prefabList)
            {
                if (prefab.name == itemData.GetPrefabName())
                {
                    RectTransform rectTransform = prefab.transform as RectTransform;
                    itemData.size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
                    return itemData.size;
                }
            }

            return Vector2.zero;
        }

        protected void __AddData(IScrollViewItemDataHandler itemData)
        {
            if (itemData == null)
            {
                LogError("ScrollView.cs => __AddData, data is null");
                return;
            }

            if (itemData != null)
            {
                itemData.index = m_dataList.Count;
                m_dataList.Add(itemData);
            }
        }

        protected virtual void OnInsertData(int index, IScrollViewItemDataHandler itemData)
        {
            if (itemData == null)
            {
                return;
            }

            if (m_dataList == null)
            {
                return;
            }

            if (index < 0 || index > m_dataList.Count)
            {
                return;
            }

            m_dataList.Insert(index, itemData);
            RefreshDataIndex();
        }

        protected virtual void OnDeleteData(int index)
        {
            if (m_dataList == null)
            {
                return;
            }

            if (index < 0 || index >= m_dataList.Count)
            {
                return;
            }

            m_dataList.RemoveAt(index);
            RefreshDataIndex();
        }

        //********************************protected函数********************************//

        /********************************private函数********************************/

        protected void __InsertFirstItem(IScrollViewItemHandler item)
        {
            if (m_firstItem != null)
                m_firstItem.preItem = item;
            item.nextItem = m_firstItem;
            m_firstItem = item;
        }

        protected void __AppendLastItem(IScrollViewItemHandler item)
        {
            m_lastItem.nextItem = item;
            item.preItem = m_lastItem;
            m_lastItem = item;
        }

        //检查第一项是否需要被回收
        protected virtual void __CheckFirstItemIsNeedRecycle()
        {
            int n = 0;
            while (true)
            {
                if (m_firstItem == null)
                {
                    break;
                }
                if (m_firstItem.isStandby || m_firstItem.isWaitRecycle)
                {
                    break;
                }

                Bounds bounds = m_firstItem.GetViewportBounds();
                if (horizontal && bounds.max.x >= m_viewportBounds.min.x)
                {
                    break;
                }
                else if (vertical && bounds.min.y <= m_viewportBounds.max.y)
                {
                    break;
                }

                var nextItem = m_firstItem.nextItem;
                __ReadyRecycleItem(m_firstItem);
                m_firstItem = nextItem;

                n++;
                if (n == 10)
                {
                    LogError("__CheckFirstItemIsNeedRecycle, 循环超过了10次，请检查代码");
                    break;
                }
            }
        }

        //检查第一项是否需要被刷新
        protected virtual bool __CheckFirstItemIsNeedUpdate()
        {
            int n = 0;
            bool isReuseItem = false;
            while (true)
            {
                var firstIndex = 1;
                if (m_firstItem != null)
                {
                    if (m_firstItem.isStandby || m_firstItem.isWaitRecycle)
                    {
                        break;
                    }
                    if (m_firstItem.dataIndex <= 0)
                    {
                        break;
                    }
                    Bounds bounds = m_firstItem.GetViewportBounds();
                    if (horizontal && bounds.min.x <= m_viewportBounds.min.x)
                    {
                        break;
                    }
                    else if (vertical && bounds.max.y >= m_viewportBounds.max.y)
                    {
                        break;
                    }
                    firstIndex = m_firstItem.dataIndex;
                }
                if (firstIndex > m_dataList.Count)
                    break;
                IScrollViewItemDataHandler itemData = m_dataList[firstIndex - 1];
                IScrollViewItemHandler item = __GetItem(itemData.GetPrefabName());
                __ActivateItem(item, 0);
                __RefreshItem(item, itemData);
                __InsertFirstItem(item);

                if (itemData.IsPresetPosValid())
                {
                    item.SetAnchoredPositionByLeftTopPos(itemData.presetPos);
                    Log("使用预设位置");
                }
                else
                {
                    SetItemAnchoredPositionByNextItem(item);
                    Log("使用Item位置");
                }

                isReuseItem = true;

                n++;
                if (n == 10)
                {
                    LogError("__CheckFirstItemIsNeedUpdate, 循环超过了10次，请检查代码");
                    break;
                }
            }

            return isReuseItem;
        }

        //检查最后一项是否需要被回收
        protected virtual void __CheckLastItemIsNeedRecycle()
        {
            int n = 0;
            while (true)
            {
                if (m_lastItem == null)
                {
                    break;
                }
                if (m_lastItem.isStandby || m_lastItem.isWaitRecycle)
                {
                    break;
                }

                Bounds bounds = m_lastItem.GetViewportBounds();
                if (horizontal && bounds.min.x <= m_viewportBounds.max.x)
                {
                    break;
                }
                else if (vertical && bounds.max.y >= m_viewportBounds.min.y)
                {
                    break;
                }

                var preItem = m_lastItem.preItem;
                __ReadyRecycleItem(m_lastItem);
                //__UpdateContentSize();
                m_lastItem = preItem;

                n++;
                if (n == 10)
                {
                    LogError("__CheckLastItemIsNeedRecycle, 循环超过了10次，请检查代码");
                    break;
                }
            }
        }

        //检查最后一项是否需要被刷新
        protected virtual bool __CheckLastItemIsNeedUpdate()
        {
            int n = 0;
            bool isReuseItem = false;
            while (true)
            {
                if (m_lastItem == null)
                {
                    break;
                }
                if (m_lastItem.isStandby || m_lastItem.isWaitRecycle)
                {
                    break;
                }
                if (m_lastItem.dataIndex >= m_dataList.Count - 1)
                {
                    break;
                }
                Bounds bounds = m_lastItem.GetViewportBounds();
                if (horizontal && bounds.max.x >= m_viewportBounds.max.x)
                {
                    break;
                }
                else if (vertical && bounds.min.y <= m_viewportBounds.min.y)
                {
                    break;
                }
                var lastIndex = m_lastItem.dataIndex;
                IScrollViewItemDataHandler itemData = m_dataList[lastIndex + 1];
                IScrollViewItemHandler item = __GetItem(itemData.GetPrefabName());
                __ActivateItem(item);
                __RefreshItem(item, itemData);
                __AppendLastItem(item);

                if (itemData.IsPresetPosValid())
                {
                    item.SetAnchoredPositionByLeftTopPos(itemData.presetPos);
                    Log("使用预设位置");
                }
                else
                {
                    SetItemAnchoredPositionByPreItem(item);
                    Log("使用Item位置");
                }

                isReuseItem = true;

                //UpdateContentSize();

                n++;
                if (n == 10)
                {
                    LogError("__CheckLastItemIsNeedUpdate, 循环超过了10次，请检查代码");
                    break;
                }
            }

            return isReuseItem;
        }

        protected Vector2 CalculateContentTargetPosByItem(IScrollViewItemDataHandler itemDataHandler)
        {
            Vector2 halfSize = new Vector2(viewport.rect.width / 2, viewport.rect.height / 2);
            Vector2 size = __GetSize(itemDataHandler);
            Vector2 pos = itemDataHandler.presetPos;
            Vector2 targetPos = content.anchoredPosition;
            if (horizontal)
            {
                targetPos.x = halfSize.x - pos.x - size.x / 2;
                float leftBorder = viewport.rect.width - content.rect.width;
                float rightBorder = 0;
                targetPos.x = Mathf.Clamp(targetPos.x, leftBorder, rightBorder);
            }
            else if (vertical)
            {
                targetPos.y = -halfSize.y - pos.y + size.y / 2;
                float topBorder = content.rect.height - viewport.rect.height;
                float bottomBorder = 0;
                targetPos.y = Mathf.Clamp(targetPos.y, bottomBorder, topBorder);
            }
            return targetPos;
        }

        protected void __ScrollToItem(IScrollViewItemDataHandler itemDataHandler, float time, Action onFinish)
        {
            if (itemDataHandler == null || !itemDataHandler.IsPresetPosValid())
            {
                return;
            }
            Vector2 targetPos = CalculateContentTargetPosByItem(itemDataHandler);
            __DoContentScroll(targetPos, time, onFinish);
        }

        protected void __StopScrollTweening(bool isInvokeComplete)
        {
            if (m_seq != null)
            {
                m_seq.Kill();
                m_seq = null;
            }
            m_isTweening = false;
        }

        protected void __DoContentScroll(Vector2 pos, float time, Action onFinish)
        {
            __StopScrollTweening(false);
            m_isTweening = true;
            StopMovement();
            m_seq = content.DOAnchorPos(pos, time).SetEase(Ease.Linear).OnComplete(() =>
            {
                m_isTweening = false;
                m_seq = null;
                if (onFinish != null)
                {
                    onFinish();
                }
            });

            // __StopScrollTweening(false);
            // m_isTweening = true;
            // StopMovement();
            // m_seq = DOTween.Sequence();
            // m_seq.Append(content.DOAnchorPos(pos, time));
            // m_seq.SetEase(Ease.Linear);
            // m_seq.onComplete = () =>
            // {
            //     m_isTweening = false;
            //     m_seq = null;
            //     if (onFinish != null)
            //     {
            //         onFinish();
            //     }
            // };
        }

        protected void __MoveToItem(IScrollViewItemDataHandler itemDataHandler)
        {
            Vector2 targetPos = CalculateContentTargetPosByItem(itemDataHandler);
            SetContentAnchoredPosition(targetPos);
        }

        protected bool __IsRectCrossRect(Vector2 leftTopPos, Vector2 rightBottomPos, Rect rect)
        {
            if (rightBottomPos.x < rect.min.x)
            {
                return false;
            }

            if (leftTopPos.x > rect.max.x)
            {
                return false;
            }

            if (leftTopPos.y < rect.min.y)
            {
                return false;
            }

            if (rightBottomPos.y > rect.max.y)
            {
                return false;
            }

            return true;
        }

        protected void Log(string log)
        {
            if (m_debugLog)
            {
                Debug.Log(log);
            }
        }

        protected void LogWarning(string log)
        {
            if (m_debugLog)
            {
                Debug.LogWarning(log);
            }
        }

        protected void LogError(string log)
        {
            Debug.LogError(log);
        }

        #region 传递滑动事件
        protected void __DoParentBeginDrag(PointerEventData data)
        {
            Transform t = this.transform.parent;
            while (t != null)
            {
                ScrollRect scrollRect = t.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.OnBeginDrag(data);
                    break;
                }
                t = t.parent;
            }
        }

        protected void __DoParentDrag(PointerEventData data)
        {
            Transform t = this.transform.parent;
            while (t != null)
            {
                ScrollRect scrollRect = t.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.OnDrag(data);
                    break;
                }
                t = t.parent;
            }
        }

        protected void __CheckParentScroll()
        {
            Transform t = this.transform.parent;
            while (t != null)
            {
                ScrollView scrollView = t.GetComponent<ScrollView>();
                if (scrollView != null)
                {
                    m_isTriggerParentScroll = scrollView.m_isTriggerScroll || scrollView.m_isTriggerParentScroll;
                    break;
                }
                t = t.parent;
            }
        }

        protected void __DoParentEndDrag(PointerEventData data)
        {
            Transform t = this.transform.parent;
            while (t != null)
            {
                ScrollRect scrollRect = t.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    scrollRect.OnEndDrag(data);
                    break;
                }
                t = t.parent;
            }
        }

        protected Vector2 TransformScreenVectorToCanvasVector(Vector2 screenVec)
        {
            return new Vector2(screenVec.x * m_scale_Canvas_Screen.x, screenVec.y * m_scale_Canvas_Screen.y);
        }
        #endregion

        public enum ItemStatus
        {
            Standby,    //回收中，准备中
            WaitRecycle,    //等待回收
            Using,      //使用中
        }

        //********************************private函数********************************//
    }




#if UNITY_EDITOR
    [CustomEditor(typeof(ScrollView))]
    public class ScrollViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}
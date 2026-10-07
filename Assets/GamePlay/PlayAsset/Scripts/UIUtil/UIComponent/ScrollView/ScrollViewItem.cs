using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIUtil.UI
{
    public class ScrollViewItem : MonoBehaviour, IScrollViewItemHandler
    {
        private IScrollViewItemHandler m_preItem = null;
        public IScrollViewItemHandler preItem
        {
            get { return m_preItem; }
            set { m_preItem = value; }
        }

        private IScrollViewItemHandler m_nextItem = null;
        public IScrollViewItemHandler nextItem
        {
            get { return m_nextItem; }
            set { m_nextItem = value; }
        }

        //此属性仅供PageView使用
        private bool m_isMain;
        public virtual bool isMain
        {
            set { m_isMain = value; }
            get { return m_isMain; }
        }

        private bool m_isDirty;
        public bool isDirty
        {
            get { return m_isDirty; }
        }

        private ScrollView.ItemStatus m_status;
        public ScrollView.ItemStatus status
        {
            set { m_status = value; }
            get { return m_status; }
        }

        public bool isUsing
        {
            get { return m_status == ScrollView.ItemStatus.Using; }
        }

        public bool isStandby
        {
            get { return m_status == ScrollView.ItemStatus.Standby; }
        }

        public bool isWaitRecycle
        {
            get { return m_status == ScrollView.ItemStatus.WaitRecycle; }
        }

        private RectTransform m_rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_rectTransform == null)
                {
                    m_rectTransform = transform as RectTransform;
                }
                return m_rectTransform;
            }
        }

        //item处于List中的下标，一般不用
        private int m_suffix;
        public int suffix
        {
            set { m_suffix = value; }
            get { return m_suffix; }
        }

        //item的数据index
        private int m_index;
        public int dataIndex
        {
            set { m_index = value; }
            get { return m_index; }
        }

        private string m_originName = null;
        public string originName
        {
            get { return m_originName; }
        }

        private string m_name;
        public string itemName
        {
            get { return m_name; }
        }

        public string objName
        {
            get { return name; }
            set { name = value; }
        }

        private object m_data;
        public object myData
        {
            get { return m_data; }
        }

        private Action<IScrollViewItemHandler> m_clickHandler;
        public Action<IScrollViewItemHandler> clickHandler
        {
            get { return m_clickHandler; }
            set { m_clickHandler = value; }
        }

        private bool m_isInit = false;
        public void Init()
        {
            if (m_isInit)
            {
                return;
            }
            OnInit();
            m_isInit = true;
        }
        protected virtual void OnInit()
        {
            m_name = m_originName;
            //this.rectTransform.pivot = Vector2.up;
        }

        public void Create(string originName) //invoke by ScrollView
        {
            m_originName = originName;
            Vector2 size = rectTransform.rect.size;
            rectTransform.anchorMin = Vector2.up;
            rectTransform.anchorMax = Vector2.up;
            SetSize(size);
            m_isDirty = true;

            OnCreate();
        }

        protected virtual void OnCreate() { }

        public void MarkDirty()
        {
            m_isDirty = true;
        }

        public void Refresh()
        {
            OnBeforeRefresh();
            OnRefresh();
            OnAfterRefresh();
        }
        protected virtual void OnBeforeRefresh() { }
        protected virtual void OnRefresh() { }
        protected virtual void OnAfterRefresh() { }

        public void RefreshData(int index, object data)
        {
            OnBeforeRefreshData(index, data);
            m_index = index;
            m_data = data;
            objName = m_name + m_index;
            OnRefreshData(index, data);
            OnAfterRefreshData(index, data);
        }
        protected virtual void OnBeforeRefreshData(int index, object data = null) { }
        protected virtual void OnRefreshData(int index, object data = null) { }
        protected virtual void OnAfterRefreshData(int index, object data = null) { m_isDirty = false; }

        public void ClearData()
        {
            OnClearData();
            m_data = null;
        }
        public virtual void OnClearData() { }

        public void Release()
        {
            OnRelease();
            transform.SetParent(null);
            Destroy(gameObject);
        }
        protected virtual void OnRelease() { }

        private bool m_isActivated = false;
        public void Activate()  //invoke by ScrollView
        {
            if (m_isActivated)
            {
                return;
            }
            m_isActivated = true;
            OnActivate();
        }
        protected virtual void OnActivate() { }

        public void Recycle()    //invoke by ScrollView
        {
            if (m_isActivated)
            {
                OnRecycle();
                m_isActivated = false;
                return;
            }
        }
        protected virtual void OnRecycle() { }

        public virtual bool IsMatch(object para) { return false; }

        public void SetSize(Vector2 size)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public void SetSize(float width, float height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public void SetWidth(float width)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        public void SetHeight(float height)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public void SetAnchoredPositionByPivotPos(Vector2 pos, Vector2 pivot)
        {
            float x = pos.x + rectTransform.rect.width * (rectTransform.pivot.x - pivot.x);
            float y = pos.y + rectTransform.rect.height * (rectTransform.pivot.y - pivot.y);
            rectTransform.anchoredPosition = new Vector2(x, y);
        }

        //根据左下的位置设置锚点位置(传入的参数是rect对齐后的左下位置)
        public void SetAnchoredPositionByLeftBottomPos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.zero);
        }

        //根据左中的位置设置锚点位置(传入的参数是rect对齐后的左中位置)
        public void SetAnchoredPositionByLeftMiddlePos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.up * 0.5f);
        }

        //根据左上的位置设置锚点位置(传入的参数是rect对齐后的左上位置)
        public void SetAnchoredPositionByLeftTopPos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.up);
        }

        //根据上中的位置设置锚点位置(传入的参数是rect对齐后的上中位置)
        public void SetAnchoredPositionByTopMiddlePos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, new Vector2(0.5f, 1f));
        }

        //根据右上的位置设置锚点位置(传入的参数是rect对齐后的右上位置)
        public void SetAnchoredPositionByRightTopPos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.one);
        }

        //根据右中的位置设置锚点位置(传入的参数是rect对齐后的右中位置)
        public void SetAnchoredPositionByRightMiddlePos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, new Vector2(1f, 0.5f));
        }

        //根据右下的位置设置锚点位置(传入的参数是rect对齐后的右下位置)
        public void SetAnchoredPositionByRightBottomPos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.right);
        }

        //根据下中的位置设置锚点位置(传入的参数是rect对齐后的下中位置)
        public void SetAnchoredPositionByBottomMiddlePos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.right * 0.5f);
        }

        //根据中心的位置设置锚点位置(传入的参数是rect对齐后的左下角位置)
        public void SetAnchoredPositionByCenterPos(Vector2 pos)
        {
            SetAnchoredPositionByPivotPos(pos, Vector2.one * 0.5f);
        }

        //获取包围盒
        public Bounds GetBounds()
        {
            Vector2 center = Vector2.zero;
            Vector2 size = new Vector2(this.rectTransform.rect.width, this.rectTransform.rect.height);
            center.x = this.rectTransform.anchoredPosition.x + (0.5f - this.rectTransform.pivot.x) * size.x;
            center.y = this.rectTransform.anchoredPosition.y + (0.5f - this.rectTransform.pivot.y) * size.y;

            return new Bounds(center, size);
        }

        //获取在视口区域下的包围盒
        public Bounds GetViewportBounds()
        {
            Bounds bounds = GetBounds();
            Vector2 center = bounds.center;
            RectTransform parent = this.rectTransform.parent as RectTransform;
            center += parent.anchoredPosition;

            return new Bounds(center, bounds.size);
        }
    }
}
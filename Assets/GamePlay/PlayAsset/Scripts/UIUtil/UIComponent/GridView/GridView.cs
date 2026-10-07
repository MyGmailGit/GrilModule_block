using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

namespace UIUtil.UI
{
    public class GridView : ScrollView
    {
        /********************************序列化成员********************************/
        #region 序列化成员

        [SerializeField]
        public int straightCount = 1;   //一行或一列的数量
        /// <summary>
        /// 开始位置的层级
        /// </summary>
        public int startSiblingIndex = 0;

        #endregion
        //********************************序列化成员********************************//

        /********************************非序列化成员********************************/
        #region 非序列化成员

        #endregion
        //********************************非序列化成员********************************//

        /********************************get set********************************/
        #region get set

        #endregion
        //********************************get set********************************//

        /********************************Action********************************/
        #region Action

        #endregion
        //********************************Action********************************//

        /********************************Unity生命周期函数********************************/
        #region Unity生命周期函数

        #endregion
        //********************************Unity生命周期函数********************************//

        /********************************public函数********************************/
        #region public函数

        #endregion
        //********************************public函数********************************//

        /********************************protected函数********************************/
        #region protected函数

        protected override void OnCalculateItemPos()
        {
            if (straightCount < 1)
            {
                return;
            }

            base.OnCalculateItemPos();
        }

        protected override void OnCalculateItemPosByHorizontal()
        {
            float startX = paddingLeft;
            float startY = 0 - paddingTop;
            Vector2 curPos = new Vector2(startX, startY);
            Vector2 lastCellSize = Vector2.zero;

            int num = 0;
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                IScrollViewItemDataHandler data = m_dataList[i];
                data.presetPos = curPos;
                Vector2 size = __GetSize(data);

                if (data.IsSpaceItem())
                {
                    //SpaceItem
                    if (num != 0)
                    {
                        curPos.x += lastCellSize.x + startX;
                    }
                    curPos.y = startY;
                    data.presetPos = curPos;
                    curPos.x += size.x + startX;
                    num = 0;
                }
                else
                {
                    //普通Item
                    num++;
                    if (num >= straightCount)
                    {
                        num = 0;
                        curPos.x += size.x + spaceX;
                        curPos.y = startY;
                    }
                    else
                    {
                        curPos.y -= size.y + spaceY;
                    }
                }

                lastCellSize = size;
            }
        }

        protected override void OnCalculateItemPosByVertical()
        {
            float startX = paddingLeft;
            float startY = 0 - paddingTop;
            Vector2 curPos = new Vector2(startX, startY);
            Vector2 lastCellSize = Vector2.zero;

            int num = 0;
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                IScrollViewItemDataHandler data = m_dataList[i];
                data.presetPos = curPos;
                Vector2 size = __GetSize(data);

                if (data.IsSpaceItem())
                {
                    //SpaceItem
                    if (num != 0)
                    {
                        curPos.y -= lastCellSize.y + spaceY;
                    }
                    curPos.x = startX;
                    data.presetPos = curPos;
                    curPos.y -= size.y + spaceY;
                    num = 0;
                }
                else
                {
                    //普通Item
                    num++;
                    if (num >= straightCount)
                    {
                        num = 0;
                        curPos.y -= size.y + spaceY;
                        curPos.x = startX;
                    }
                    else
                    {
                        curPos.x += size.x + spaceX;
                    }
                }

                lastCellSize = size;
            }
        }

        protected override void OnRefresh()
        {
            RecycleAllItem();

            if (m_dataList == null)
            {
                LogError("GridView => __Refresh fail, data is wrong");
                return;
            }

            Rect viewRect = new Rect(new Vector2(m_viewportBounds.min.x, m_viewportBounds.min.y), m_viewportBounds.size);
            Vector2 curPos = new Vector2(paddingLeft, 0 - paddingTop);
            Vector2 contentAnchorPos = content.anchoredPosition;
            Vector2 size, viewLeftTopPos, viewRightBottomPos;

            bool isView = false;
            IScrollViewItemHandler item;
            IScrollViewItemDataHandler itemData;

            m_firstItem = null;
            m_lastItem = null;

            for (int i = 0; i < m_dataList.Count; ++i)
            {
                itemData = m_dataList[i];
                size = __GetSize(itemData);

                curPos = itemData.presetPos;
                viewLeftTopPos = curPos + contentAnchorPos;
                viewRightBottomPos.x = viewLeftTopPos.x + size.x;
                viewRightBottomPos.y = viewLeftTopPos.y - size.y;

                if (__IsRectCrossRect(viewLeftTopPos, viewRightBottomPos, viewRect))
                {
                    item = __GetItem(itemData.GetPrefabName());
                    __ActivateItem(item);
                    __RefreshItem(item, m_dataList[i]);
                    item.SetAnchoredPositionByLeftTopPos(curPos);
                    isView = true;
                    if (m_firstItem == null)
                    {
                        m_firstItem = item;
                    }

                    if (m_lastItem != null)
                    {
                        m_lastItem.nextItem = item;
                        item.preItem = m_lastItem;
                    }
                    m_lastItem = item;
                }
                else if (isView)
                {
                    break;
                }
            }
        }

        protected override void __OnScrollUp(bool isCheck)
        {
            if (isCheck)
            {
                __CheckFirstStrandIsNeedRecycle();
                __CheckLastStrandIsNeedUpdate();
            }

            if (OnScrollUp != null)
            {
                OnScrollUp(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected override void __OnScrollDown(bool isCheck)
        {
            if (isCheck)
            {
                __CheckLastStrandIsNeedRecycle();
                __CheckFirstStrandIsNeedUpdate();
            }

            if (OnScrollDown != null)
            {
                OnScrollDown(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected override void __OnScrollLeft(bool isCheck)
        {
            if (isCheck)
            {
                __CheckFirstStrandIsNeedRecycle();
                __CheckLastStrandIsNeedUpdate();
            }

            if (OnScrollLeft != null)
            {
                OnScrollLeft(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }
        protected override void __OnScrollRight(bool isCheck)
        {
            if (isCheck)
            {
                __CheckLastStrandIsNeedRecycle();
                __CheckFirstStrandIsNeedUpdate();
            }

            if (OnScrollRight != null)
            {
                OnScrollRight(m_contentPreAnchoredPosition, content.anchoredPosition);
            }
        }

        #endregion
        //********************************protected函数********************************//

        /********************************private函数********************************/
        #region private函数

        //检查第一行(列)是否需要被回收
        private void __CheckFirstStrandIsNeedRecycle()
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

        //检查第一行(列)是否需要被刷新
        private void __CheckFirstStrandIsNeedUpdate()
        {
            var preIndex = m_firstItem == null ? 0 : m_firstItem.dataIndex - 1;
            if (preIndex < 0)
            {
                return;
            }
            int n = 0;
            Vector2 leftTopPos = Vector2.zero;
            Vector2 rightBottomPos = Vector2.zero;
            Vector2 contentAnchorPos = content.anchoredPosition;
            IScrollViewItemDataHandler itemData = null;

            while (true)
            {
                if (preIndex < 0 || preIndex >= m_dataList.Count)
                {
                    break;
                }

                itemData = m_dataList[preIndex];
                leftTopPos = itemData.presetPos + contentAnchorPos;
                rightBottomPos.x = leftTopPos.x + itemData.size.x;
                rightBottomPos.y = leftTopPos.y - itemData.size.y;
                if (horizontal && rightBottomPos.x < m_viewportBounds.min.x)
                {
                    break;
                }
                else if (vertical && rightBottomPos.y > m_viewportBounds.max.y)
                {
                    break;
                }

                IScrollViewItemHandler item = __GetItem(itemData.GetPrefabName());
                __ActivateItem(item, startSiblingIndex);
                __RefreshItem(item, itemData);
                __InsertFirstItem(item);
                item.SetAnchoredPositionByLeftTopPos(itemData.presetPos);
                if (OnReuseItem != null)
                {
                    OnReuseItem(vertical ? MoveDirection.Down : MoveDirection.Right);
                }
                preIndex--;

                n++;
                if (n == 10)
                {
                    LogError("__CheckFirstItemIsNeedUpdate, 循环超过了10次，请检查代码");
                    break;
                }
            }
        }

        //检查最后一行(列)是否需要被回收
        private void __CheckLastStrandIsNeedRecycle()
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
                m_lastItem = preItem;

                n++;
                if (n == 10)
                {
                    LogError("__CheckLastItemIsNeedRecycle, 循环超过了10次，请检查代码");
                    break;
                }
            }
        }

        //检查最后一行(列)是否需要被刷新
        private void __CheckLastStrandIsNeedUpdate()
        {
            if (m_lastItem == null)
            {
                return;
            }
            var nextIndex = m_lastItem.dataIndex + 1;
            if (nextIndex >= m_dataList.Count)
            {
                return;
            }
            int n = 0;
            Vector2 leftTopPos = Vector2.zero;
            Vector2 contentAnchorPos = content.anchoredPosition;
            IScrollViewItemDataHandler itemData = null;

            while (true)
            {
                if (nextIndex >= m_dataList.Count)
                {
                    break;
                }
                itemData = m_dataList[nextIndex];
                leftTopPos = itemData.presetPos + contentAnchorPos;
                if (horizontal && leftTopPos.x > m_viewportBounds.max.x)
                {
                    break;
                }
                else if (vertical && leftTopPos.y < m_viewportBounds.min.y)
                {
                    break;
                }

                IScrollViewItemHandler item = __GetItem(itemData.GetPrefabName());
                __ActivateItem(item);
                __RefreshItem(item, itemData);
                __AppendLastItem(item);

                if (OnReuseItem != null)
                {
                    OnReuseItem(vertical ? MoveDirection.Up : MoveDirection.Left);
                }
                item.SetAnchoredPositionByLeftTopPos(itemData.presetPos);

                nextIndex++;

                n++;
                if (n == 10)
                {
                    LogError("__CheckLastItemIsNeedUpdate, 循环超过了10次，请检查代码");
                    break;
                }
            }
        }

        #endregion
        //********************************private函数********************************//

        /********************************Animation********************************/
        #region Animation

        #endregion
        //********************************Animation********************************//

    }



#if UNITY_EDITOR
    [CustomEditor(typeof(GridView))]
    public class GridViewEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}
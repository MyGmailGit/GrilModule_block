using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIUtil.UI
{

    public interface IScrollViewItemHandler
    {
        RectTransform rectTransform { get; }

        IScrollViewItemHandler preItem { get; set; }
        IScrollViewItemHandler nextItem { get; set; }

        ScrollView.ItemStatus status { get; set; }

        Action<IScrollViewItemHandler> clickHandler { get; set; }

        bool isMain { get; set; }
        bool isDirty { get; }
        bool isUsing { get; }
        bool isStandby { get; }
        bool isWaitRecycle { get; }

        int suffix { get; set; }
        int dataIndex { get; set; }

        string originName { get; }
        string itemName { get; }
        string objName { get; set; }

        object myData { get; }

        //需要关注的
        void Create(string originName);
        void Init();
        void Refresh();
        void RefreshData(int index, object data);
        void MarkDirty();
        void ClearData();
        void Release();
        void Activate();
        void Recycle();
        bool IsMatch(object para);

        //不需要关注的
        void SetSize(Vector2 size);
        void SetSize(float width, float height);
        void SetWidth(float width);
        void SetHeight(float height);

        void SetAnchoredPositionByPivotPos(Vector2 pos, Vector2 pivot);
        void SetAnchoredPositionByLeftBottomPos(Vector2 pos);
        void SetAnchoredPositionByLeftMiddlePos(Vector2 pos);
        void SetAnchoredPositionByLeftTopPos(Vector2 pos);
        void SetAnchoredPositionByTopMiddlePos(Vector2 pos);
        void SetAnchoredPositionByRightTopPos(Vector2 pos);
        void SetAnchoredPositionByRightMiddlePos(Vector2 pos);
        void SetAnchoredPositionByRightBottomPos(Vector2 pos);
        void SetAnchoredPositionByBottomMiddlePos(Vector2 pos);
        void SetAnchoredPositionByCenterPos(Vector2 pos);

        Bounds GetBounds();
        Bounds GetViewportBounds();
    }
}


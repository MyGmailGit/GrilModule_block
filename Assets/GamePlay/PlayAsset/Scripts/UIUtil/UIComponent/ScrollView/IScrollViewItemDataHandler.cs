using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIUtil.UI
{
    public interface IScrollViewItemDataHandler
    {
        int index { get; set; }
        Vector2 size { get; set; }
        Vector2 presetPos { get; set; } //pivot为(0,1)的位置
        string GetPrefabName();
        object GetData();
        bool IsSizeValid();
        bool IsPresetPosValid();
        bool IsSpaceItem();
    }
}
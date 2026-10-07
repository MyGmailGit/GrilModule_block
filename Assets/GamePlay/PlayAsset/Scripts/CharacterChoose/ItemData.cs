/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

namespace FancyScrollView
{
    class ItemData
    {
        public string PicId { get; }
        public string MainId { get; }

        // 是否第一次进游戏
        public bool isFirst = false;

        public ItemData(string mainId, string message, bool isf)
        {
            MainId = mainId;
            PicId = message;
            isFirst = isf;
        }
    }
}

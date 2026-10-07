/*
 * FancyScrollView (https://github.com/setchi/FancyScrollView)
 * Copyright (c) 2020 setchi
 * Licensed under MIT (https://github.com/setchi/FancyScrollView/blob/master/LICENSE)
 */

using UnityEngine;
using System.Collections.Generic;
using System;

namespace FancyScrollView
{
    class Context
    {
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }

    class ScrollView : FancyScrollView<ItemData, Context>
    {
        [SerializeField] Scroller scroller = default;
        [SerializeField] GameObject cellPrefab = default;

        protected override GameObject CellPrefab => cellPrefab;

        public event Action<int> onSelectionIndex;

        protected override void Initialize()
        {
            base.Initialize();
            scroller.OnValueChanged(UpdatePosition);
            scroller.OnSelectionChanged(UpdateSelection);
        }

        public void UpdateData(IList<ItemData> items)
        {
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
        }

        void UpdateSelection(int index)
        {
            // if (Context.SelectedIndex == index)
            // {
            //     return;
            // }

            // Context.SelectedIndex = index;
            onSelectionIndex?.Invoke(index);
            Refresh();
        }
    }
}

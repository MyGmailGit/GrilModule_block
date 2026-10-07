using System.Collections.Generic;
using UnityEngine;
using Data;
using Watermelon;
using UnityEngine.UI;
using Game.RedDot;

namespace GameUI
{
    public class UITreasure : Watermelon.UIPage
    {
        private TreasureDataList treasureDataList = TreasureDataList.Data;

        [SerializeField] private List<RectTransform> treasureItemsPath = new();
        [SerializeField] private List<TreasureItem> treasureItems = new();

        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject getAllTip;

        private bool isAnimating = false;
        private TreasureSave save;

        bool isSubscribe = false;

        // Circular list tracking
        private int currentStartIndex = 0; // Index in data list that represents the first displayed item

        public override void Init()
        {
            isSubscribe = IAPManager.IsSubscribed();

            getAllTip.gameObject.SetActive(false);
            save = SaveController.GetSaveObject<TreasureSave>("TreasureSave");

            // REMOVED: CheckLevel() call

            RefreshAllItems();
            BindFirstItem();

            closeButton.onClick.AddListener(() =>
            {
                if (isAnimating)
                    return;
                UIController.HidePage(this);
            });

            CheckAllGet();
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            isSubscribe = IAPManager.IsSubscribed();

            RefreshAllItems();
        }

        // REMOVED: CheckLevel() method entirely

        public void OnClickFirstItem()
        {
            StartCoroutine(PlayGetAnim());
        }

        private System.Collections.IEnumerator PlayGetAnim()
        {
            if (isAnimating || treasureItems.Count == 0)
                yield break;

            isAnimating = true;

            TreasureItem firstItem = treasureItems[0];

            firstItem.PlayDisappearAnim();
            yield return new WaitForSeconds(0.2f);

            // Update circular index - move to next treasure
            currentStartIndex = (currentStartIndex + 1) % treasureDataList.TreasureDataListData.Count;
            save.currentTreasureIndex = currentStartIndex;
            save.Flush();

            // Refresh all items based on new start index
            RefreshAllItemsData();

            // Move items to positions
            for (int i = 0; i < treasureItems.Count; i++)
            {
                bool isCenter = (i == 0);
                treasureItems[i].MoveTo(treasureItemsPath[i], isCenter);
            }

            yield return new WaitForSeconds(0.35f);

            firstItem.PlayAppearAnim();
            yield return new WaitForSeconds(0.25f);

            isAnimating = false;
            BindFirstItem();
            CheckAllGet();
            RedDotChecker.Instance.CheckTreasureRedDot();
        }

        private void RefreshAllItems()
        {
            currentStartIndex = save.currentTreasureIndex;
            RefreshAllItemsData();
            RefreshItemsPositionInstant();
        }

        private void RefreshAllItemsData()
        {
            int dataCount = treasureDataList.TreasureDataListData.Count;

            for (int i = 0; i < treasureItems.Count; i++)
            {
                int dataIndex = (currentStartIndex + i) % dataCount;

                if (dataCount > 0)
                {
                    treasureItems[i].SetData(treasureDataList.TreasureDataListData[dataIndex], isSubscribe);
                }
                else
                {
                    treasureItems[i].SetEmpty();
                }
            }
        }

        private void BindFirstItem()
        {
            for (int i = 0; i < treasureItems.Count; i++)
            {
                treasureItems[i].SetClickable(i == 0, OnClickFirstItem);
            }
        }

        private void RefreshItemsPositionInstant()
        {
            for (int i = 0; i < treasureItems.Count; i++)
            {
                treasureItems[i].SetPositionInstant(treasureItemsPath[i], i == 0);
            }
        }

        private void CheckAllGet()
        {
            // Check if we've gone through all items at least once?
            // For circular list, "all get" logic might need adjustment based on your requirements
            // This version checks if the current cycle has been completed
            bool allSeen = false;
            if (treasureDataList.TreasureDataListData.Count > 0)
            {
                // You might want to track a separate "completed cycles" counter
                // For now, we'll just hide the tip
                getAllTip.gameObject.SetActive(false);
            }
            else
            {
                getAllTip.gameObject.SetActive(true);
            }
        }

        #region Save
        [System.Serializable]
        public class TreasureSave : ISaveObject
        {
            public int currentTreasureIndex = 0;
            // REMOVED: public int resetLevel = 0;

            public void Flush()
            {
            }
        }
        #endregion
    }
}
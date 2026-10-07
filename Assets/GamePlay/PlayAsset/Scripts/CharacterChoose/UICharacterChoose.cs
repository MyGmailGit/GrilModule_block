using System.Collections.Generic;
using UnityEngine;
using Data;
using Watermelon;
using UnityEngine.UI;
using Game.RedDot;
using FancyScrollView;
using System.Linq;
using VideoSystem;
using System.Runtime.InteropServices;
using System;
using Watermelon.IAPStore;

namespace GameUI
{
    public class UICharacterChoose : UIPage
    {
        // private TreasureDataList treasureDataList = TreasureDataList.Data;

        [SerializeField] ScrollView scrollView;
        [SerializeField] private Button choseHerBtn;
        [SerializeField] private Image choseHerVipImg;
        [SerializeField] private Button refreshListBtn;
        [SerializeField] CurrencyUIPanelSimple diamondPanel;
        [SerializeField] private Button closeButton;


        private SimpleIntSave saveData;

        private static Action<int> OnFinishChooseCallBack = null;


        private int currentStartIndex = 0;

        private int NeedDiamond = 10;

        private bool isSubscribe;

        public static void ShowPage(Action<int> OnFinishChoose)
        {
            OnFinishChooseCallBack = OnFinishChoose;
            UIController.ShowPage<UICharacterChoose>();
        }

        public override void Init()
        {
            isSubscribe = IAPManager.IsSubscribed();

            // getAllTip.gameObject.SetActive(false);
            // save = SaveController.GetSaveObject<TreasureSave>("TreasureSave");
            saveData = SaveController.GetSaveObject<SimpleIntSave>("UICharacterChoose");

            // // REMOVED: CheckLevel() call

            closeButton.onClick.AddListener(() =>
            {
                OnFinishChooseCallBack?.Invoke(-1);
                UIController.HidePage(this);

                AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            });

            choseHerBtn.onClick.AddListener(OnChooseHerTouch);
            refreshListBtn.onClick.AddListener(OnRefreshListTouch);

            scrollView.onSelectionIndex += OnSelectionIndex;
            diamondPanel.AddButton.onClick.AddListener(OnAddDiamond);

            // CheckAllGet();
        }

        public override void PlayHideAnimation()
        {
            OnFinishChooseCallBack = null;
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            LoadChooseImg();

            isSubscribe = IAPManager.IsSubscribed();
            UIController.OnPageOpened(this);
            // RefreshAllItems();
            InitView();
        }

        private void LoadChooseImg(bool reset = false)
        {
            var data = VideoSerilNumberManager.Instance.GetCandidates(reset);

            List<ItemData> itemDatas = new();
            foreach (var it in data)
            {
                itemDatas.Add(new ItemData(it.mainId, VideoSerilNumberManager.FormatMainIdFileId(it.mainId, it.fileRange[0]), saveData.Value != 1 ? true : false));
            }

            scrollView.UpdateData(itemDatas);
        }

        private void InitView()
        {
            if (saveData.Value != 1)
            {
                refreshListBtn.gameObject.SetActive(false);
                closeButton.gameObject.SetActive(false);
                diamondPanel.gameObject.SetActive(false);
                choseHerVipImg.gameObject.SetActive(false);
            }
            else
            {
                refreshListBtn.gameObject.SetActive(true);
                closeButton.gameObject.SetActive(true);
                diamondPanel.gameObject.SetActive(true);
            }
        }
        private void OnAddDiamond()
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            UIController.ShowPage<UIStoreCoinDiamond>();
        }
        private void OnChooseHerTouch()
        {
            if (saveData.Value != 1)
            {
                saveData.Value = 1;
                saveData.Flush();
            }
            else
            {
                if (currentStartIndex == 2 && isSubscribe != true)
                {
                    UIController.ShowPage<UIStoreSubscribe>();
                    return;
                }
                else if (currentStartIndex == 3)
                {
                    AdsManager.ShowRewardBasedVideo((bool reward) =>
                    {
                        if (reward)
                        {
                            OnChooseConfirm();
                        }
                    }, AnalyticsStr.reward_girls_character_choose);
                    return;
                }
            }
            OnChooseConfirm();

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnChooseConfirm()
        {
            VideoSerilNumberManager.Instance.SelectCandidate(currentStartIndex);
            OnFinishChooseCallBack?.Invoke(currentStartIndex);
            UIController.HidePage(this);
        }


        private void OnRefreshListTouch()
        {
            // var currency = CurrencyController.GetCurrency(CurrencyType.Diamond);

            if (CurrencyController.HasAmount(CurrencyType.Diamond, NeedDiamond))
            {
                CurrencyController.Substract(CurrencyType.Diamond, NeedDiamond, "refresh img");

                LoadChooseImg(true);
            }
            else
            {
                UIController.ShowPage<UIStoreCoinDiamond>();
            }

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        }

        private void OnSelectionIndex(int idx)
        {
            currentStartIndex = idx;
            if (saveData.Value == 1)
                if (currentStartIndex == 2)
                {
                    choseHerVipImg.gameObject.SetActive(true);
                }
                else
                {
                    choseHerVipImg.gameObject.SetActive(false);
                }
        }
    }
}
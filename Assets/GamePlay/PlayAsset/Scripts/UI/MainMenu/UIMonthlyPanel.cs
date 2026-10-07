using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIMonthlyPanel : IMainPage
{
    public int currentListIndex = 0;
    public class MonthlyScrollData
    {
        public MonthlyData monthlyData;
        public int lastLevelNum;
        public int maxLevelNum;
    }

    [SerializeField] private ScrollView m_gridScrollView;
    [SerializeField] TextMeshProUGUI timeTxt;
    [SerializeField] private Button premBuyBtn;

    [SerializeField] private Button getAllBtn;


    [SerializeField] Image[] todayWinFrame;
    [SerializeField] TodayTaskProgress todayTaskProgress;
    // [SerializeField] Image todayTaskProgress;
    // [SerializeField] TextMeshProUGUI todayTaskWinGetNum;
    // [SerializeField] TextMeshProUGUI todayTaskWinProgreTxt;
    // [SerializeField] Image todayTaskIcon;


    Vector2 m_cellSize = new Vector2(900, 280);
    bool isInit = false;

    bool shouldUpdate = false;

    public override void Init()
    {
        if (isInit) return;

        m_gridScrollView.Init();
        m_gridScrollView.Clear();

        premBuyBtn.onClick.AddListener(OnPremBuy);
        getAllBtn.onClick.AddListener(OnGetAllButton);
    }


    public override void Show()
    {
        base.Show();
        __InitView();
        __InitList();

        shouldUpdate = true;
    }
    public override void Hide()
    {
        base.Hide();
        shouldUpdate = false;
    }

    private void Update()
    {
        if (shouldUpdate)
        {
            timeTxt.text = MonthlyCtrl.Instance.GetFormattedRemainingTime();
        }
    }


    private void __InitView()
    {
        __InitFiveHeart();

        if (!MonthlyCtrl.Instance.GetIAP_BuyPremium())
        {
            premBuyBtn.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            premBuyBtn.transform.DOKill();
        }
    }
    private void __InitFiveHeart()
    {
        int fiveHeart = MonthlyCtrl.Instance.GetCurrentHeartLevel();
        foreach (var it in todayWinFrame)
        {
            it.gameObject.SetActive(false);
        }
        if (fiveHeart >= (int)MonthlyCtrl.FiveWinHeart.Five)
        {
            todayWinFrame[4].gameObject.SetActive(true);
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.Four)
        {
            todayWinFrame[3].gameObject.SetActive(true);
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.Three)
        {
            todayWinFrame[2].gameObject.SetActive(true);
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.Two)
        {
            todayWinFrame[1].gameObject.SetActive(true);
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.One)
        {
            todayWinFrame[0].gameObject.SetActive(true);
        }

        // heart progress

        // todayTaskIcon
        todayTaskProgress.InitTodayTaskProgress();

        // int currentPlayingIdx = MonthlyCtrl.Instance.GetCurrentHeartPlayingIndex();
        // int currentTodayHeart = MonthlyCtrl.Instance.GetTodayHeartCount();

        // icon
        // var playingTaskRewartd = MonthlyDataList.Data.FiveHeartDailyTask[currentPlayingIdx];
        // var rewardPreview = playingTaskRewartd.rewardsSet.GetPreviews()[0];
        // todayTaskIcon.sprite = rewardPreview.Icon;
        // todayTaskWinGetNum.text = rewardPreview.Text;

        // progress

        // int needNum = playingTaskRewartd.needHeartNum;
        // todayTaskWinProgreTxt.text = $"{Math.Min(currentTodayHeart, needNum)}/{needNum}";

        // todayTaskProgress.fillAmount = ((float)currentTodayHeart) / needNum;
    }

    private void __InitList()
    {
        var data = MonthlyDataList.Data.MonthlyDataListData;

        m_gridScrollView.Clear();

        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        int itemCount = (m_gridScrollView as GridView).straightCount;
        var itemW = m_cellSize.x * itemCount + m_gridScrollView.spaceX * (itemCount - 1);
        var deltaWidth = (width - itemW) / 2;

        m_gridScrollView.paddingLeft = (int)deltaWidth;

        int levelLast = 0;

        var levelNumFinish = MonthlyCtrl.Instance.GetTaskCompleteCount();
        currentListIndex = -1;

        int finishMaxNum = int.MaxValue;

        for (int i = 0; i < data.Count; ++i)
        {
            var dataOneTemp = data[i];

            m_gridScrollView.AddData(new ScrollViewItemData("MonthlyTaskItem", new MonthlyScrollData()
            {
                monthlyData = dataOneTemp,
                lastLevelNum = levelLast,
                maxLevelNum = data.Count,
            }, m_cellSize));

            levelLast = dataOneTemp.need;
            // 设置滚动位置
            if (currentListIndex == -1 && levelNumFinish < dataOneTemp.need)
            {
                currentListIndex = i;
            }
            // 记录一下完成的最大值，用来判断是否需要显示 获取按钮
            if (levelNumFinish >= dataOneTemp.need)
            {
                finishMaxNum = dataOneTemp.need;
            }
        }

        m_gridScrollView.Refresh();

        if (currentListIndex == -1)
        {
            currentListIndex = data.Count - 1;
        }

        DOVirtual.DelayedCall(0.3f, () => { m_gridScrollView.ScrollToItem(currentListIndex); });

        bool iap_buy = MonthlyCtrl.Instance.GetIAP_BuyPremium();

        if (MonthlyCtrl.Instance.CanClaimTaskReward_free(finishMaxNum) || (iap_buy && MonthlyCtrl.Instance.CanClaimTaskReward_Prem(finishMaxNum)))
        {
            getAllBtn.gameObject.SetActive(true);
        }
        else
        {
            getAllBtn.gameObject.SetActive(false);
        }
    }

    private void OnGetAllButton()
    {
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);

        List<IRewardPreview> rewardPreviews = new();

        var data = MonthlyDataList.Data.MonthlyDataListData;

        bool iap_buy = MonthlyCtrl.Instance.GetIAP_BuyPremium();

        VideoSerilNumberManager.Instance.specialData.ClearLastTimeGet();

        foreach (var it in data)
        {
            // 能领取
            if (MonthlyCtrl.Instance.CanClaimTaskReward_free(it.need))
            {
                MonthlyCtrl.Instance.ClaimTaskReward(it.need);

                rewardPreviews.AddRange(it.rewardsFreeSet.GetPreviews());
                it.rewardsFreeSet.ApplyReward();
            }

            if (iap_buy && MonthlyCtrl.Instance.CanClaimTaskReward_Prem(it.need))
            {
                MonthlyCtrl.Instance.ClaimTaskReward_Prem(it.need);

                rewardPreviews.AddRange(it.rewardsPremSet.GetPreviews());
                it.rewardsPremSet.ApplyReward();
            }
        }

        UIRewardsConfirmation.Display(rewardPreviews, null, true);

        if (VideoSerilNumberManager.Instance.specialData.lastTimeGetIds.Count > 0)
        {
            List<string> unlockIdsStr = new List<string>();
            foreach (var it in VideoSerilNumberManager.Instance.specialData.lastTimeGetIds)
            {
                unlockIdsStr.Add(VideoSerilNumberManager.FormatSurpriseMainIdFileId(it.mainId_last, it.fileId_last));
            }

            UIPopDownload.Show(unlockIdsStr, true);

            VideoSerilNumberManager.Instance.specialData.ClearLastTimeGet();
        }


        __InitView();
        __InitList();
    }

    private void OnPremBuy()
    {
        if (!MonthlyCtrl.Instance.GetIAP_BuyPremium())
        {
            UIMonthlyBuy.Show(() =>
            {
                __InitView();
                __InitList();
            });
        }
        // UIController.ShowPage<UIMonthlyBuy>();

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

}

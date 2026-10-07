using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class UIWinCountProgress : UIPage
{
    [SerializeField] private RectTransform fiveWinFrame;
    [SerializeField] private TextMeshProUGUI[] fiveWinStreakTxts;
    [SerializeField] private Image heartProgress;
    [SerializeField] private Image dailytaskIcon;
    [SerializeField] private RectTransform heartRect;
    [SerializeField] private TextMeshProUGUI winGetNum;
    [SerializeField] private TextMeshProUGUI winProgressTxt;

    [SerializeField] private RectTransform flyHeartParent;
    [SerializeField] private RectTransform[] flyHearts;

    private static Action callback = null;

    public static void Show(Action action)
    {
        // 判断：1.是否已经领取今天的。2.是否已经开始月度任务。3.当前的5心连是否大于0，小于1就不能跑获取的动画，所以不显示
        if (MonthlyCtrl.Instance.IsTodayRewardClaimed() ||
            !MonthlyCtrl.Instance.IsStartMonthlyTask() ||
            MonthlyCtrl.Instance.GetCurrentHeartLevel() < 1)
        {
            action?.Invoke();
            return;
        }

        callback = action;
        UIController.ShowPage<UIWinCountProgress>();
    }


    public override void Init()
    {
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);
        callback?.Invoke();
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);

        initView();
    }

    private void initView()
    {
        int currentPlayingIdx = MonthlyCtrl.Instance.GetCurrentHeartPlayingIndex();
        int currentTodayHeart = MonthlyCtrl.Instance.GetTodayHeartCount();

        // icon
        var playingTaskRewartd = MonthlyDataList.Data.FiveHeartDailyTask[currentPlayingIdx];
        var rewardPreview = playingTaskRewartd.rewardsSet.GetPreviews()[0];
        dailytaskIcon.sprite = rewardPreview.Icon;
        winGetNum.text = rewardPreview.Text;

        // progress
        int needNum = playingTaskRewartd.needHeartNum;
        // winProgressTxt.text = $"{currentTodayHeart}/{needNum}";

        // heartProgress.fillAmount = ((float)currentTodayHeart) / needNum;


        RectTransform frameMoveTo = null;
        int fiveHeart = MonthlyCtrl.Instance.GetCurrentHeartLevel();
        int flyHeartNum = 0;
        if (fiveHeart >= (int)MonthlyCtrl.FiveWinHeart.Five)
        {
            frameMoveTo = fiveWinStreakTxts[4].rectTransform;
            flyHeartNum = (int)MonthlyCtrl.FiveWinHeart.Five;
            flyHeartParent.localPosition = fiveWinStreakTxts[4].rectTransform.localPosition;
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.Four)
        {
            frameMoveTo = fiveWinStreakTxts[3].rectTransform;
            flyHeartNum = (int)MonthlyCtrl.FiveWinHeart.Four;
            flyHeartParent.localPosition = fiveWinStreakTxts[3].rectTransform.localPosition;
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.Three)
        {
            frameMoveTo = fiveWinStreakTxts[2].rectTransform;
            flyHeartNum = (int)MonthlyCtrl.FiveWinHeart.Three;
            flyHeartParent.localPosition = fiveWinStreakTxts[2].rectTransform.localPosition;
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.Two)
        {
            frameMoveTo = fiveWinStreakTxts[1].rectTransform;
            flyHeartNum = (int)MonthlyCtrl.FiveWinHeart.Two;

            flyHeartParent.localPosition = fiveWinStreakTxts[1].rectTransform.localPosition;
        }
        else if (fiveHeart == (int)MonthlyCtrl.FiveWinHeart.One)
        {
            frameMoveTo = fiveWinStreakTxts[0].rectTransform;
            flyHeartNum = (int)MonthlyCtrl.FiveWinHeart.One;

            flyHeartParent.localPosition = fiveWinStreakTxts[0].rectTransform.localPosition;
        }

        if (frameMoveTo == null)
        {
            fiveWinFrame.gameObject.SetActive(false);
        }
        else
        {
            //跑动画
            var seq = DOTween.Sequence();
            seq.Append(fiveWinFrame.transform.DOLocalMoveX(frameMoveTo.localPosition.x, 0.6f));
            seq.AppendInterval(0.25f);

            for (int i = 0; i < flyHeartNum; i++)
            {
                var flyHeart = flyHearts[i];
                flyHeart.gameObject.SetActive(true);
                var localPos = flyHeart.localPosition;

                seq.Join(flyHeart.DOScale(1, 0.1f));
                seq.Join(flyHeart.DOLocalMove(localPos + new Vector3(UnityEngine.Random.Range(-100.0f, 100.0f), UnityEngine.Random.Range(-100.0f, 100.0f), 0), 0.1f));
            }
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() =>
            {
                AudioController.PlaySound(AudioController.AudioClips.Fly);
            });

            for (int i = 0; i < flyHeartNum; i++)
            {
                var flyHeart = flyHearts[i];
                seq.Join(flyHeart.DOMove(heartRect.position, 0.65f));
            }

            seq.AppendCallback(() => { UHeartProgress(currentTodayHeart, needNum); });

            // 如果heart够了 显示一下获取
            if (currentTodayHeart >= needNum && !MonthlyCtrl.Instance.IsTodayRewardClaimed())
            {
                //设置数据已经获取
                playingTaskRewartd.rewardsSet.ApplyReward();
                MonthlyCtrl.Instance.ClaimDailyHeartReward();

                seq.AppendInterval(0.6f);

                seq.AppendCallback(() =>
                {
                    UIRewardsConfirmation.Display(playingTaskRewartd.rewardsSet.GetPreviews());
                });
            }
            seq.AppendInterval(0.5f);
            seq.AppendCallback(() => { UIController.HidePage(this); });
            seq.Play();
        }
    }
    private void UHeartProgress(int getNum, int needNum)
    {
        // int needNum = playingTaskRewartd.needHeartNum;
        winProgressTxt.text = $"{getNum}/{needNum}";
        heartProgress.fillAmount = 0; heartProgress.DOFillAmount(((float)getNum) / needNum, 0.5f);
    }
}

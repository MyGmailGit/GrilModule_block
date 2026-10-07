using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class TodayTaskProgress : MonoBehaviour
{
    [SerializeField] Image todayTaskProgress;
    [SerializeField] TextMeshProUGUI todayTaskWinGetNum;
    [SerializeField] TextMeshProUGUI todayTaskWinProgreTxt;
    [SerializeField] Image todayTaskIcon;
    // [SerializeField] bool autoInit;

    public void InitTodayTaskProgress()
    {
        int currentPlayingIdx = MonthlyCtrl.Instance.GetCurrentHeartPlayingIndex();
        int currentTodayHeart = MonthlyCtrl.Instance.GetTodayHeartCount();

        // icon
        if (currentPlayingIdx == -1) currentPlayingIdx = 0;
        var playingTaskRewartd = MonthlyDataList.Data.FiveHeartDailyTask[currentPlayingIdx];
        var rewardPreview = playingTaskRewartd.rewardsSet.GetPreviews()[0];
        todayTaskIcon.sprite = rewardPreview.Icon;
        todayTaskWinGetNum.text = rewardPreview.Text;

        int needNum = playingTaskRewartd.needHeartNum;
        todayTaskWinProgreTxt.text = $"{Math.Min(currentTodayHeart, needNum)}/{needNum}";

        todayTaskProgress.fillAmount = ((float)currentTodayHeart) / needNum;
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class MonthlyTaskItem : ScrollViewItem
{
    [SerializeField] Image icon_free;
    [SerializeField] TextMeshProUGUI remain_free;
    [SerializeField] Image lockcover_free;
    [SerializeField] Image limitTag_free;
    [SerializeField] Image getCover_free;

    [SerializeField] Image icon_prem;
    [SerializeField] TextMeshProUGUI remain_prem;
    [SerializeField] Image lockcover_prem;
    [SerializeField] Image limitTag_prem;
    [SerializeField] Image getCover_prem;

    // [SerializeField] TextMeshProUGUI finishLevelNum;
    [SerializeField] Image finishLevelProgress;
    [SerializeField] Image finishLevelProgressBg;
    [SerializeField] Image finishLight;

    [SerializeField] Image levelNumLast;
    [SerializeField] TextMeshProUGUI levelNumLastTxt;

    [SerializeField] Image levelNumNow;
    [SerializeField] TextMeshProUGUI levelNumNowTxt;

    [SerializeField] Sprite[] monthlyProgresSpr;



    private int _itemIdx;
    private UIMonthlyPanel.MonthlyScrollData _data;
    protected override void OnInit()
    {
        base.OnInit();
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnRefresh()
    {
        base.OnRefresh();
        __InitView();
    }

    public override bool IsMatch(object para)
    {
        return para == _data;
    }

    protected override void OnRefreshData(int index, object data = null)
    {
        base.OnRefreshData(index, data);
        _itemIdx = index;
        _data = (UIMonthlyPanel.MonthlyScrollData)data;
        __InitView();
    }

    protected override void OnRecycle()
    {
        base.OnRecycle();
    }

    protected override void OnRelease()
    {
    }

    private void __InitView()
    {
        if (_itemIdx == 0)
        {
            finishLevelProgressBg.gameObject.SetActive(false);
        }
        else
        {
            finishLevelProgressBg.gameObject.SetActive(true);
        }

        limitTag_free.gameObject.SetActive(_data.monthlyData.limitedTag_free);
        var freePreviList = _data.monthlyData.rewardsFreeSet.GetPreviews();
        if (freePreviList.Count > 0)
        {
            icon_free.sprite = freePreviList[0].Icon;
            remain_free.text = freePreviList[0].Text;
        }

        limitTag_prem.gameObject.SetActive(_data.monthlyData.limitedTag_prem);
        var premPreviList = _data.monthlyData.rewardsPremSet.GetPreviews();
        if (premPreviList.Count > 0)
        {
            icon_prem.sprite = premPreviList[0].Icon;
            remain_prem.text = premPreviList[0].Text;
        }

        levelNumLast.gameObject.SetActive(false);
        levelNumNow.gameObject.SetActive(false);

        if (_data.maxLevelNum - 1 == _itemIdx)
        {
            levelNumLast.gameObject.SetActive(true);
            levelNumLastTxt.text = _data.lastLevelNum.ToString();
            levelNumNow.gameObject.SetActive(true);
            levelNumNowTxt.text = _data.monthlyData.need.ToString();
        }
        else if (_data.lastLevelNum != 0)
        {
            levelNumLast.gameObject.SetActive(true);
            levelNumLastTxt.text = _data.lastLevelNum.ToString();
        }

        levelNumLast.sprite = monthlyProgresSpr[1];
        // levelNumNow.sprite = monthlyProgresSpr[1];

        // finishLight.gameObject.SetActive(false);
        getCover_free.gameObject.SetActive(false);
        getCover_prem.gameObject.SetActive(false);

        var levelNumf = MonthlyCtrl.Instance.GetTaskCompleteCount();
        if (levelNumf >= _data.lastLevelNum)
        {
            levelNumLast.sprite = monthlyProgresSpr[0];
        }

        if (levelNumf < _data.lastLevelNum)
        {
            lockcover_free.gameObject.SetActive(true);
            lockcover_prem.gameObject.SetActive(true);
            finishLevelProgress.fillAmount = 0;
        }
        else if (levelNumf >= _data.monthlyData.need)
        {
            finishLevelProgress.fillAmount = 1;

            // levelNumLast.sprite = monthlyProgresSpr[0];
            // levelNumNow.sprite = monthlyProgresSpr[0];

            lockcover_free.gameObject.SetActive(false);
            if (MonthlyCtrl.Instance.GetIAP_BuyPremium())
            {
                lockcover_prem.gameObject.SetActive(false);
            }

            // if (MonthlyCtrl.Instance.CanClaimTaskReward_free(_data.monthlyData.need))
            // {
            //     finishLight.gameObject.SetActive(true);
            // }
            // else
            // {
            //     finishLight.gameObject.SetActive(false);
            // }

            if (MonthlyCtrl.Instance.IsTaskRewardClaimed_Free(_data.monthlyData.need))
            {
                getCover_free.gameObject.SetActive(true);
            }
            if (MonthlyCtrl.Instance.IsTaskRewardClaimed_Prem(_data.monthlyData.need))
            {
                getCover_prem.gameObject.SetActive(true);
            }
        }
        else
        {
            // lockcover_free.gameObject.SetActive(false);
            // lockcover_prem.gameObject.SetActive(false);
            float fullNum = _data.monthlyData.need - _data.lastLevelNum;
            int curNum = levelNumf - _data.lastLevelNum;

            finishLevelProgress.fillAmount = curNum / fullNum;
        }

    }
}

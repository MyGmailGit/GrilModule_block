using System;
using System.Collections.Generic;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UICharacterItem : ScrollViewItem
{
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] RawImage[] rawImg;

    private UIPopCharacterView.CharacterViewData _data;

    private List<RawImgLoading> rawImgLoading = new();
    // private Button[] buttons;

    protected override void OnInit()
    {
        base.OnInit();

        // 绑定按钮事件
        // selfButton.onClick.AddListener(OnClick);
        // playButton.onClick.AddListener(OnPlayTouch);
    }

    public void OnClick(int i)
    {
        if (_data == null) return;
        if (i < _data.stageData.completedFileIds.Count)
        {

            var str = VideoSerilNumberManager.FormatMainIdFileId(_data.mainId, _data.stageData.completedFileIds[i]);
            UIGameGirlsShowView.ShowForItem(str, str);
        }

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnRefresh()
    {
        base.OnRefresh();
    }

    public override bool IsMatch(object para)
    {
        return para == _data;
    }

    protected override void OnRefreshData(int index, object data = null)
    {
        base.OnRefreshData(index, data);
        _data = (UIPopCharacterView.CharacterViewData)data;

        // progressTxt.text = $"{_data.maindata.completedCount}/{_data.maindata.totalCount}";
        // progress.fillAmount = _data.maindata.completedCount * 1.0f / _data.maindata.totalCount;

        // if (_data.maindata.isPlayed)
        // {
        //     playCover.gameObject.SetActive(false);
        // }
        // else
        // {
        //     playCover.gameObject.SetActive(true);
        // }

        if (rawImgLoading.Count < 1)
        {
            foreach (var it in rawImg)
            {
                rawImgLoading.Add(it.GetComponent<RawImgLoading>());
            }
        }

        foreach (var it in rawImgLoading)
        {
            it.gameObject.SetActive(false);
        }

        for (int i = 0; i < _data.stageData.completedFileIds.Count; ++i)
        {
            if (_data.stageData.completedFileIds[i] >= 1)
            {
                string imgId = VideoSerilNumberManager.FormatMainIdFileId(_data.mainId, _data.stageData.completedFileIds[i]);
                if (i < rawImgLoading.Count)
                {
                    rawImgLoading[i].LoadImage(imgId);
                    rawImgLoading[i].gameObject.SetActive(true);
                }
            }
        }
    }

    protected override void OnRecycle()
    {
        base.OnRecycle();
    }

    protected override void OnRelease()
    {
        foreach (var it in rawImgLoading)
        {
            it.Release();
        }
    }

}


using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIAlbumsCharaterItem : ScrollViewItem
{
    [SerializeField] Button selfButton;
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] RawImage rawImg;
    [SerializeField] Image progress;
    [SerializeField] TextMeshProUGUI progressTxt;

    private UIAlbumsPanel.CharacterScrollData _data;

    private RawImgLoading rawImgLoading;

    protected override void OnInit()
    {
        base.OnInit();
        // 绑定按钮事件
        selfButton.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        UIPopCharacterView.Show(_data);
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
        _data = (UIAlbumsPanel.CharacterScrollData)data;

        progressTxt.text = $"{_data.maindata.completedCount}/{_data.maindata.totalCount}";
        progress.fillAmount = _data.maindata.completedCount * 1.0f / _data.maindata.totalCount;

        if (rawImgLoading == null) rawImgLoading = rawImg.GetComponent<RawImgLoading>();
        string imgId = VideoSerilNumberManager.FormatMainIdFileId(_data.maindata.mainId, _data.maindata.stages[0].completedFileIds[0]);
        rawImgLoading.LoadImage(imgId);

        nameTxt.text = AB_Ctrl.Instance.GetNameWithMainId(_data.maindata.mainId);
    }

    protected override void OnRecycle()
    {
        base.OnRecycle();
    }

    protected override void OnRelease()
    {
        rawImgLoading.Release();
    }

}


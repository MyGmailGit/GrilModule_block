using System;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIAlbumsSurpriseItem : ScrollViewItem
{
    [SerializeField] Button selfButton;
    [SerializeField] TextMeshProUGUI nameTxt;
    [SerializeField] RawImage rawImg;
    [SerializeField] Image playCover;
    [SerializeField] Button playButton;

    private UIAlbumsPanel.SurpriseScrollData _data;

    private RawImgLoading rawImgLoading;

    protected override void OnInit()
    {
        base.OnInit();
        // 绑定按钮事件
        selfButton.onClick.AddListener(OnCloseClick);
        playButton.onClick.AddListener(OnPlayTouch);
    }

    private void OnPlayTouch()
    {
        playButton.interactable = false;

        if (VideoSerilNumberManager.Instance.surpriseData.GetOneFileLevel(_data.maindata.mainId, _data.maindata.fileId) == -1)
        {
            int levelIndex = ActiveSession.Current.GetNextSpecialLevelIndex();

            VideoSerilNumberManager.Instance.surpriseData.SetOneFileLevel(_data.maindata.mainId, _data.maindata.fileId, levelIndex);
        }
        int levelid = VideoSerilNumberManager.Instance.surpriseData.GetOneFileLevel(_data.maindata.mainId, _data.maindata.fileId);

        VideoSerilNumberManager.Instance.surpriseData.SetCurrentPlaying(_data.maindata.mainId, _data.maindata.fileId);

        MenuController.LoadSpecialGame(levelid);

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    private void OnCloseClick()
    {
        if (_data == null) return;

        var str = VideoSerilNumberManager.FormatSurpriseMainIdFileId(_data.maindata.mainId, _data.maindata.fileId);
        UIGameGirlsShowView.ShowForItem(str, str);

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
        _data = (UIAlbumsPanel.SurpriseScrollData)data;

        // progressTxt.text = $"{_data.maindata.completedCount}/{_data.maindata.totalCount}";
        // progress.fillAmount = _data.maindata.completedCount * 1.0f / _data.maindata.totalCount;

        if (_data.maindata.isPlayed)
        {
            playCover.gameObject.SetActive(false);
        }
        else
        {
            playCover.gameObject.SetActive(true);
        }

        if (rawImgLoading == null) rawImgLoading = rawImg.GetComponent<RawImgLoading>();
        string imgId = VideoSerilNumberManager.FormatSurpriseMainIdFileId(_data.maindata.mainId, _data.maindata.fileId);
        rawImgLoading.LoadImage(imgId);
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


using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.IAPStore;

public class UIFinishGetImgShow : UIPage
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private RawImage icon;
    [SerializeField] private RawImgLoading iconLoading;
    [SerializeField] private Toggle likeBtn;
    [SerializeField] private Image loadingImg;
    [SerializeField] private Button rvBtn;
    [SerializeField] private Button diamondBtn;

    [SerializeField] private TextMeshProUGUI priceTxt;

    [SerializeField] private TextMeshProUGUI nameTxt;

    [SerializeField] private Button floorButton;

    private const int COIN_DOWNLOAD_COST = 5;

    private static string fullFileId;
    private static string mainId;
    private static Action onCloseCallback;

    public static void Show(string mainIdArg, string fullFileIdArg, Action onClose = null)
    {
        mainId = mainIdArg;
        fullFileId = fullFileIdArg;
        onCloseCallback = onClose;
        UIController.ShowPage<UIFinishGetImgShow>();
    }

    public override void Init()
    {
        closeBtn.onClick.AddListener(OnCloseClick);
        likeBtn.onValueChanged.AddListener(OnLikeClick);
        rvBtn.onClick.AddListener(OnRvClick);
        diamondBtn.onClick.AddListener(OnDiamondClick);
        floorButton.onClick.AddListener(OnFloorClick);
    }

    private void OnCloseClick()
    {
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        UIController.HidePage(this);
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);
        iconLoading.Release();
        onCloseCallback?.Invoke();
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);


        priceTxt.text = COIN_DOWNLOAD_COST.ToString();
        iconLoading.LoadImage(fullFileId);

        nameTxt.text = AB_Ctrl.Instance.GetNameWithMainId(mainId);

        rvBtn.interactable = true;
        diamondBtn.interactable = true;

        likeBtn.SetIsOnWithoutNotify(GameGirlsLikeController.IsLiked(fullFileId));

        AudioController.PlaySound(AudioController.AudioClips.appear);
    }

    private void OnDiamondClick()
    {

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        if (CurrencyController.HasAmount(CurrencyType.Diamond, COIN_DOWNLOAD_COST))
        {
            CurrencyController.Substract(CurrencyType.Diamond, COIN_DOWNLOAD_COST, "downloadImage");

            DownloadSave();

            DOVirtual.DelayedCall(0.35f, () =>
            {
                OnCloseClick();
            });

        }
        else
        {
            UIController.ShowPage<UIStoreCoinDiamond>();
        }
    }

    private void OnRvClick()
    {
        rvBtn.interactable = false;

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        AdsManager.ShowRewardBasedVideo((success) =>
        {
            if (!success)
            {
                rvBtn.interactable = true;
                return;
            }
            DownloadSave();

            DOVirtual.DelayedCall(0.35f, () =>
            {
                OnCloseClick();
            });

        }, AnalyticsStr.reward_game_girls_download_at_finish);
    }

    private void DownloadSave()
    {
        string videoPath = UIGameGirlsShowView.PendingSelection.GetVideoPath(fullFileId);
        bool saved = GameGirlsVideoSaveUtility.TrySaveVideoToDevice(videoPath, fullFileId, fullFileId, out string message);
        if (saved)
        {
            GameGirlsDownloadController.MarkAsDownloaded(fullFileId);
        }

        SystemMessage.ShowMessage(message);
    }

    private void OnLikeClick(bool isOn)
    {
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        GameGirlsLikeController.SetLiked(fullFileId, isOn);
    }

    private void OnFloorClick()
    {
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        // var pendingSelection = new UIGameGirlsShowView.PendingSelection(levelId, fileId);
        // UIController.ShowPage<UIGameGirlsShowView>();
        UIGameGirlsShowView.ShowForItem(fullFileId, fullFileId, () =>
        {
            likeBtn.SetIsOnWithoutNotify(GameGirlsLikeController.IsLiked(fullFileId));
        });

        UIController.HidePage<UIFinishParticle>();



    }
}

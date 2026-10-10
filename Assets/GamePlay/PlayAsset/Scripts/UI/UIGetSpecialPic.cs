using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIGetSpecialPic : UIPage, IPopupWindow
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private RawImage icon;
    [SerializeField] private RawImgLoading iconLoading;
    [SerializeField] private Image loadingImg;
    [SerializeField] private Button rvBtn;
    [SerializeField] private Button coinBtn;
    [SerializeField] private TextMeshProUGUI priceTxt;

    public bool IsOpened => canvas.enabled;
    private const int COIN_DOWNLOAD_COST = 200;

    private static string fullFileId;
    private static string mainId;
    private static int fileId;
    // private static Action onCloseCallback;

    public static bool shouldShowSpecialGet { get; set; } = false;


    public static void Show(Action onClose = null)
    {
        //直接取消了，不要了 special直接送图，但是不玩关卡
        onClose?.Invoke(); return;

        var specialOne = VideoSerilNumberManager.Instance.specialData.GetOneFileToUse();
        if (!shouldShowSpecialGet || specialOne == null) { onClose?.Invoke(); return; }

        mainId = specialOne.Value.mainId;
        fileId = specialOne.Value.fileId;

        fullFileId = VideoSerilNumberManager.FormatSurpriseMainIdFileId(specialOne.Value.mainId, specialOne.Value.fileId);

        UIPopupQueueManager.Instance.EnqueuePopup<UIGetSpecialPic>();
        shouldShowSpecialGet = false;
    }

    public override void Init()
    {
        closeBtn.onClick.AddListener(OnCloseClick);
        rvBtn.onClick.AddListener(OnRvClick);
        coinBtn.onClick.AddListener(OnDiamondClick);
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
        // onCloseCallback?.Invoke();
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);

        priceTxt.text = COIN_DOWNLOAD_COST.ToString();

        icon.SetAlpha(0);


        loadingImg.gameObject.SetActive(true);
        iconLoading.LoadImage(fullFileId, () =>
        {
            loadingImg.gameObject.SetActive(false);
            icon.DOFade(1, 0.3F);
        });

        rvBtn.interactable = true;
        coinBtn.interactable = true;

    }

    private void OnDiamondClick()
    {
        coinBtn.interactable = false;

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        if (CurrencyController.HasAmount(CurrencyType.Coins, COIN_DOWNLOAD_COST))
        {
            CurrencyController.Substract(CurrencyType.Coins, COIN_DOWNLOAD_COST, "");

            PlaySpecialLevel();

            // DOVirtual.DelayedCall(0.35f, () =>
            // {
            //     OnCloseClick();
            // });
        }
        else
        {
            coinBtn.interactable = true;
            SystemMessage.ShowMessage("Not enough Coin");
            // UIController.ShowPage<DiamondCoinBuy>();
        }
    }

    private void OnRvClick()
    {
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
        rvBtn.interactable = false;
        AdsManager.ShowRewardBasedVideo((success) =>
        {
            if (!success)
            {
                rvBtn.interactable = true;
                return;
            }
            PlaySpecialLevel();

            // DOVirtual.DelayedCall(0.35f, () =>
            // {
            //     OnCloseClick();
            // });

        }, AnalyticsStr.reward_special_get);
    }

    private void PlaySpecialLevel()
    {
        VideoSerilNumberManager.Instance.specialData.SetOneFileUse(mainId, fileId);

        if (VideoSerilNumberManager.Instance.specialData.GetOneFileLevel(mainId, fileId) == -1)
        {
            int levelIndex = ActiveSession.Current.GetNextSpecialLevelIndex();

            VideoSerilNumberManager.Instance.specialData.SetOneFileLevel(mainId, fileId, levelIndex);
        }
        int levelid = VideoSerilNumberManager.Instance.specialData.GetOneFileLevel(mainId, fileId);

        VideoSerilNumberManager.Instance.specialData.SetCurrentPlaying(mainId, fileId);

        MenuController.LoadSpecialGame(levelid);
    }
}

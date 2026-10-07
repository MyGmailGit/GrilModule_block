using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIPlaySameGirl : UIPage
{
    [SerializeField] private Button findNewBtn;
    [SerializeField] private RawImage icon;
    [SerializeField] private RawImgLoading iconLoading;
    [SerializeField] private Image loadingImg;
    [SerializeField] private GameObject[] unlockProgress;
    [SerializeField] private TextMeshProUGUI unlockProgressTxt;
    [SerializeField] private Button rvBtn;
    [SerializeField] private TextMeshProUGUI nameGirl;

    private int chooseStage = -1;


    private static string mainId;
    private static Action onCloseCallback;

    public static void Show(Action onClose = null)
    {
        var previousImage = VideoSerilNumberManager.Instance.GetPreviousImage();

        if (previousImage == null || VideoSerilNumberManager.Instance.IsMainIdCompleted(previousImage.Value.mainId))
        {
            onClose?.Invoke();
            return;
        }

        mainId = previousImage.Value.mainId;
        onCloseCallback = onClose;
        UIController.ShowPage<UIPlaySameGirl>();
    }

    public override void Init()
    {
        findNewBtn.onClick.AddListener(OnCloseClick);
        rvBtn.onClick.AddListener(OnRvClick);
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

        InitView();
    }

    private void InitView()
    {
        rvBtn.interactable = true;

        var completionInfo = VideoSerilNumberManager.Instance.GetMainIdCompletionInfo(mainId);
        var fullStageCount = completionInfo.totalCount / 4;
        var finishStageCount = completionInfo.completedCount / 4;

        unlockProgressTxt.text = $"Unlocked:{finishStageCount}/{fullStageCount}";

        nameGirl.text = AB_Ctrl.Instance.GetNameWithMainId(mainId);

        foreach (var it in unlockProgress)
        {
            it.gameObject.SetActive(false);
        }

        if (finishStageCount >= 3)
        {
            unlockProgress[1].SetActive(true);
        }
        if (finishStageCount >= 2)
        {
            unlockProgress[0].SetActive(true);
        }
        // 给一个没有使用的stage ，然后返回第一张图的fileid
        var NotUseStage = VideoSerilNumberManager.Instance.GetAllUnusedStagesAndImages(mainId);
        // LoadImg
        for (int i = 0; i < 4; i++)
        {
            if (NotUseStage.ContainsKey(i))
            {
                int fileId = NotUseStage[i][0];
                loadingImg.gameObject.SetActive(true);
                iconLoading.LoadImage(VideoSerilNumberManager.FormatMainIdFileId(mainId, fileId), () => { loadingImg.gameObject.SetActive(false); });
                chooseStage = i;
                break;
            }
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

            VideoSerilNumberManager.Instance.StartStageDirectly(mainId, chooseStage);

            DOVirtual.DelayedCall(0.35f, () =>
            {
                OnCloseClick();
            });

        }, AnalyticsStr.reward_play_same_girl);
    }

}

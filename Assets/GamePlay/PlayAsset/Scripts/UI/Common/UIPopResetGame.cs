using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class UIPopResetGame : UIPage
{
    public enum CloseType
    {
        Cancel,
        RvTouch,
        LostTouch,
    }

    [SerializeField] private Button closeBtn;
    [SerializeField] private Button rvBtn;
    [SerializeField] private Button lostBtn;
    [SerializeField] private TextMeshProUGUI winTxt;

    static Action<CloseType> closeCallback;
    public static void Show(Action<CloseType> action)
    {
        closeCallback = action;
        UIController.ShowPage<UIPopResetGame>();
    }

    public override void Init()
    {
        closeBtn.onClick.AddListener(() =>
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            UIController.HidePage(this);
            closeCallback?.Invoke(CloseType.Cancel);
        });
        rvBtn.onClick.AddListener(OnRvButton);
        lostBtn.onClick.AddListener(OnLostButtton);
    }

    private void OnLostButtton()
    {
        lostBtn.interactable = false;
        closeCallback?.Invoke(CloseType.LostTouch);
        UIController.HidePage(this);

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    private void OnRvButton()
    {
        rvBtn.interactable = false;
        AdsManager.ShowRewardBasedVideo(success =>
        {
            if (success)
            {
                UIController.HidePage(this);
                closeCallback?.Invoke(CloseType.RvTouch);
                return;
            }

            rvBtn.interactable = true;
        }, AnalyticsStr.reward_keepwinStreak);

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);

        winTxt.text = $"{MonthlyCtrl.Instance.GetCurrentHeartLevel()} Win Streak";

        lostBtn.interactable = true;
        rvBtn.interactable = true;
        closeBtn.interactable = true;
    }
}

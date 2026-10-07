using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

/// <summary>
/// 完成一个阶段展示这个
/// </summary>
public class UIPlayCompletePicStage : UIPage
{
    [SerializeField] private Button nextBtn;
    [SerializeField] private RawImgLoading[] iconLoading;
    [SerializeField] private RectTransform[] frameRotateTrans;

    private List<Quaternion> frameRotateTransQuas = new List<Quaternion>();
    private Quaternion zeroQuaternion = Quaternion.Euler(0, 0, 0);

    private static string mainId;
    private static int startIdx;
    private static int endIdx;

    private static Action onCloseCallback;


    public static void Show(string mainIdArg, int startIdxArg, int endIdxArg, Action onClose = null)
    {
        mainId = mainIdArg;
        startIdx = startIdxArg;
        endIdx = endIdxArg;

        onCloseCallback = onClose;
        UIController.ShowPage<UIPlayCompletePicStage>();
    }

    public override void Init()
    {
        foreach (var it in frameRotateTrans)
        {
            frameRotateTransQuas.Add(it.localRotation);
            it.localRotation = zeroQuaternion;
        }


        nextBtn.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick()
    {
        AudioController.PlaySound(AudioController.AudioClips.appear);
        UIController.HidePage(this);
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);
        onCloseCallback?.Invoke();
        onCloseCallback = null;
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);

        foreach (var it in frameRotateTrans)
        {
            it.localRotation = zeroQuaternion;
        }
        AudioController.PlaySound(AudioController.AudioClips.Fly);

        InitView();
        for (int i = 0; i < frameRotateTrans.Length; ++i)
        {
            frameRotateTrans[i].DOLocalRotateQuaternion(frameRotateTransQuas[i], 0.4f).SetDelay(0.2f);
        }
    }

    private void InitView()
    {
        int imgIdx = 0;

        for (int i = startIdx; i <= endIdx; ++i)
        {
            iconLoading[imgIdx].LoadImage(VideoSerilNumberManager.FormatMainIdFileId(mainId, i));
            imgIdx++;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class UIGameLevelLabel : UIPage
{
    [SerializeField] private CanvasGroup labCanvas;
    [SerializeField] private Image labelPanel;
    [SerializeField] private TextMeshProUGUI leveltxt;

    private static Action callback = null;
    private static string levelstr = "";

    public static void Show(string showStr, Action action = null)
    {
        levelstr = showStr;
        callback = action;
        DOVirtual.DelayedCall(0.5f, () => { UIController.ShowPage<UIGameLevelLabel>(); });
    }


    public override void Init()
    {
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);
        callback?.Invoke();
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);

        labCanvas.alpha = 0;

        StartCoroutine(initView());
    }

    private IEnumerator initView()
    {
        leveltxt.text = levelstr;

        labelPanel.fillAmount = 0;
        leveltxt.SetAlpha(0);

        AudioController.PlaySound(AudioController.AudioClips.startLevelLabel);

        labCanvas.DOFade(1f, 0.5f);
        yield return labelPanel.DOFillAmount(1, 0.7f).WaitForCompletion();
        leveltxt.DOFade(1, 0.5f);
        yield return new WaitForSeconds(1f);
        yield return labCanvas.DOFade(0, 0.3f).WaitForCompletion();
        UIController.HidePage(this);
    }
}

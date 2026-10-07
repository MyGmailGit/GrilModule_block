using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using DG.Tweening;
using Coffee.UIExtensions;


/// <summary>
/// 主要用于在surprise获取和mothly获取图片
/// </summary>
public class UISurpriseSpecialGetPop : UIPage
{
    public static List<string> downloadFiles;
    public static void Show(List<string> downlaodFileName)
    {
        downloadFiles = downlaodFileName;
        UIController.ShowPage<UISurpriseSpecialGetPop>();
    }
    [SerializeField] private Image bg;
    [SerializeField] private Button closeBtn;
    [SerializeField] private RectTransform imgPanel;
    [SerializeField] private Animator BgRotate;
    [SerializeField] private RawImage icon;
    [SerializeField] private RawImgLoading iconLoading;
    [SerializeField] private UIParticle startParticle;
    [SerializeField] private UIParticle explosionParticle;
    [SerializeField] private UIParticle rectParticle;



    public override void Init()
    {
        closeBtn.onClick.AddListener(OnCloseClick);
    }
    private void OnCloseClick()
    {
        closeBtn.interactable = false;

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);

        var item = UIController.GetPage<UIMainMenu>().GetMenuItem(3);
        if (item != null)
        {
            rectParticle.Stop();
            rectParticle.Clear();

            bg.DOFade(0, 0.3f);
            imgPanel.transform.DOMove(item.transform.position, 0.6f);
            imgPanel.transform.DOScale(0, 0.6f).OnComplete(() =>
            {
                UIController.HidePage(this);
            });
        }
        else
        {
            UIController.HidePage(this);
        }
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);

        iconLoading.Release();

    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);
        imgPanel.localScale = Vector3.zero;
        iconLoading.LoadImage(downloadFiles[0]);
        closeBtn.interactable = true;
        imgPanel.transform.localPosition = Vector3.zero;

        bg.SetAlpha(0.9f);
        startParticle.Stop();
        startParticle.Clear();
        explosionParticle.Stop();
        explosionParticle.Clear();
        rectParticle.Stop();
        rectParticle.Clear();

        StartCoroutine(ShowRotate());
    }

    IEnumerator ShowRotate()
    {
        BgRotate.Play("startAnim");
        BgRotate.transform.localScale = Vector3.zero;
        BgRotate.transform.DOScale(1, 0.5f);

        startParticle.Play();

        AudioController.PlaySound(AudioController.AudioClips.appear);

        imgPanel.DOScale(1, 0.2f).SetEase(Ease.OutBack).SetDelay(2).OnComplete(() => { BgRotate.Play("Hide"); rectParticle.Play(); });
        yield return new WaitForSeconds(1.9f);
        explosionParticle.Play();

        AudioController.PlaySound(AudioController.AudioClips.alert);

        yield return new WaitForSeconds(0.5f);

        startParticle.Stop();
        startParticle.Clear();

    }



}

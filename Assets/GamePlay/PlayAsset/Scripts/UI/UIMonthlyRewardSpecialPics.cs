using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using DG.Tweening;


/// <summary>
/// 主要用于在surprise获取和mothly获取图片
/// </summary>
public class UIMonthlyRewardSpecialPics : UIPage
{
    public static List<string> downloadFiles;
    public static void Show(List<string> downlaodFileName)
    {
        downloadFiles = downlaodFileName;
        UIController.ShowPage<UIMonthlyRewardSpecialPics>();
    }
    [SerializeField] private Image bg;
    [SerializeField] private Button closeBtn;
    [SerializeField] private RectTransform imgPanel;

    public override void Init()
    {
        closeBtn.onClick.AddListener(OnCloseClick);
    }
    private void OnCloseClick()
    {
        closeBtn.interactable = false;

        var item = UIController.GetPage<UIMainMenu>().GetMenuItem(3);
        if (item != null)
        {
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
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class UIPopCharacterView : UIPage, IPopupWindow//, IPausePopup
{
    [SerializeField] private GridView scrollView;
    [SerializeField] private Button closeBtn;
    [SerializeField] private TextMeshProUGUI title;

    public bool IsOpened => canvas.enabled;

    private static UIAlbumsPanel.CharacterScrollData _data;

    Vector2 m_cellSize = new Vector2(813, 540);

    public static void Show(UIAlbumsPanel.CharacterScrollData data)
    {
        if (data == null) return;
        _data = data;
        UIController.ShowPage<UIPopCharacterView>();
    }

    public override void Init()
    {
        closeBtn.onClick.AddListener(() =>
        {
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);
            UIController.HidePage(this);
        });

        scrollView.Init();
        scrollView.Clear();
    }

    public override void PlayHideAnimation()
    {
        UIController.OnPageClosed(this);
        _data = null;
    }

    public override void PlayShowAnimation()
    {
        UIController.OnPageOpened(this);
        __InitList();

        title.text = AB_Ctrl.Instance.GetNameWithMainId(_data.maindata.mainId);
    }


    #region  character
    public class CharacterViewData
    {
        public string mainId;
        public VideoSystem.VideoSerilNumberManager.StageCompletionInfo stageData;
    }
    private void __InitList()
    {
        m_cellSize.x = (transform as RectTransform).rect.width;

        var mainIdTemp = _data.maindata.mainId;

        scrollView.Clear();


        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        int itemCount = (scrollView as GridView).straightCount;
        var itemW = m_cellSize.x * itemCount + scrollView.spaceX * (itemCount - 1);
        var deltaWidth = (width - itemW) / 2;

        scrollView.paddingLeft = (int)deltaWidth;

        foreach (var it in _data.maindata.stages)
        {
            scrollView.AddData(new ScrollViewItemData("CharacterViewItem", new CharacterViewData()
            {
                mainId = mainIdTemp,
                stageData = it

            }, m_cellSize));
        }
        scrollView.Refresh();
    }
    #endregion

}

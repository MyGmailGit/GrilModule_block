using System.Collections;
using System.Collections.Generic;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UISurprisePanel : IMainPage
{
    public class SurpriseScrollData
    {
        public string titletxt;
        public Watermelon.CurrencyType currencyType;
        public int price;
        public string mainId;
        public int iconIdx;
        public bool useB_Icon;
        public int currentUseIdx;
    }

    [SerializeField] private ScrollView m_gridScrollView;

    [SerializeField] CurrencyUIPanelSimple coinsPanel;
    [SerializeField] CurrencyUIPanelSimple diamondPanel;

    private static UISurprisePanel uISurprisePanelInstance = null;

    Vector2 m_cellSize = new Vector2(985, 533);

    List<AB_Ctrl.SupriseData> supriseDatas = null;

    bool isInit = false;

    public override void Init()
    {
        if (isInit) return;

        uISurprisePanelInstance = this;

        m_gridScrollView.Init();
        m_gridScrollView.Clear();

        coinsPanel.Init();
        diamondPanel.Init();

        coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);
        diamondPanel.AddButton.onClick.AddListener(AddDiamondButton);
    }

    public override void Show()
    {
        base.Show();
        // canvasPage.enabled = true;
        __InitList();
    }
    public override void Hide()
    {
        base.Hide();
        // canvasPage.enabled = false;
    }

    private void __InitList(bool isForceRefresh = false)
    {
        // var list = StageGalleryListCtrl.Instance.GetStageGalleryDatas();

        var supriseDatasTemp = VideoSerilNumberManager.Instance.surpriseData.GetSurpriseDatas();
        if (!isForceRefresh)
            if (supriseDatas == supriseDatasTemp.Item1) return;

        supriseDatas = supriseDatasTemp.Item1;

        m_gridScrollView.Clear();

        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        var deltaWidth = (width - m_cellSize.x) / 2;

        m_gridScrollView.paddingLeft = (int)deltaWidth;

        int Idx = -1;
        foreach (var it in supriseDatas)
        {
            Idx += 1;
            int currentUseIdxTemp = VideoSerilNumberManager.Instance.surpriseData.GetMainIdIt(it.mainId);
            if (currentUseIdxTemp > 1) // 这个id 是已经用过的，而且获取的是使用倒序
            {
                m_gridScrollView.AddData(new ScrollViewItemData("SurpriseItem", new SurpriseScrollData()
                {
                    titletxt = it.titletxt,
                    currencyType = it.currencyType,
                    price = it.price,
                    mainId = it.mainId,
                    iconIdx = Idx,
                    useB_Icon = supriseDatasTemp.Item2,
                    currentUseIdx = currentUseIdxTemp,
                }, m_cellSize));
            }
        }
        m_gridScrollView.Refresh();
    }

    public void AddCoinsButton()
    {
        // UIController.ShowPage<UIStore>();
        // OnMenuItemTouch(0);
        UIController.GetPage<UIMainMenu>().ShowMenuStore(1);
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }
    public void AddDiamondButton()
    {
        // UIController.ShowPage<UIStore>();
        // OnMenuItemTouch(0);
        UIController.GetPage<UIMainMenu>().ShowMenuStore(2);
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }
    public void RefreshScroll()
    {
        __InitList(true);
    }

    public static void RefreshSurpriseItem()
    {
        uISurprisePanelInstance?.RefreshScroll();
    }
}

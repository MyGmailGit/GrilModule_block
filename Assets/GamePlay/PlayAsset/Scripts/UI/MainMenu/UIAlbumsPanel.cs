using System;
using System.Collections;
using System.Collections.Generic;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UIAlbumsPanel : IMainPage
{
    [SerializeField] private ScrollView m_gridCharacterScrollView;
    [SerializeField] private ScrollView m_gridSurpriseScrollView;
    [SerializeField] private ScrollView m_gridSpecialScrollView;

    [SerializeField] private SwitchPanelItem[] switchPanelItems;

    [SerializeField] private AlbumsLikeTimeSwitch albumsLikeSwitches;
    [SerializeField] private AlbumsLikeTimeSwitch albumsTimeSwitches;

    [SerializeField] CurrencyUIPanelSimple coinsPanel;
    [SerializeField] CurrencyUIPanelSimple diamondPanel;


    private List<ScrollView> m_gridScrollViews = new();
    Vector2 m_cellSize = new Vector2(244, 420);
    // List<AB_Ctrl.SupriseData> supriseDatas = null;
    bool isInit = false;

    bool isShowLike = false;
    bool isTimeUp = false;

    public override void Init()
    {
        if (isInit) return;

        // m_gridScrollView.Init();
        // m_gridScrollView.Clear();

        coinsPanel.Init();
        diamondPanel.Init();

        coinsPanel.AddButton.onClick.AddListener(AddCoinsButton);
        diamondPanel.AddButton.onClick.AddListener(AddDiamondButton);

        m_gridScrollViews.Add(m_gridCharacterScrollView);
        m_gridScrollViews.Add(m_gridSurpriseScrollView);
        m_gridScrollViews.Add(m_gridSpecialScrollView);

        foreach (var it in m_gridScrollViews)
        {
            it.Init();
            it.Clear();
        }

        foreach (var it in switchPanelItems)
        {
            it.touchAction = OnSwitchPanelTouch;
        }

        albumsLikeSwitches.switchTouchCall = OnAlbumsLikeSwitch;
        albumsTimeSwitches.switchTouchCall = OnAlbumsTimeSwitch;

        // VideoSerilNumberManager.Instance.TestCompleteFiles();
        // VideoSerilNumberManager.Instance.surpriseData.SetupTestData();
        // VideoSerilNumberManager.Instance.specialData.GenerateTestData();


        SetTransShowHide(m_gridCharacterScrollView.transform as RectTransform, true);
        SetTransShowHide(m_gridSurpriseScrollView.transform as RectTransform, false);
        SetTransShowHide(m_gridSpecialScrollView.transform as RectTransform, false);
    }



    public override void Hide()
    {
        base.Hide();
    }
    public override void Show()
    {
        base.Show();
        // __InitSurpriseList();
        __InitCharacterList();
        __InitSurpriseList();
        __InitSpecialList();
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

    private void SetTransShowHide(RectTransform trans, bool isShow)
    {
        if (isShow)
        {
            var pos = trans.anchoredPosition;
            pos.x = 0;
            trans.anchoredPosition = pos;
        }
        else
        {
            var pos = trans.anchoredPosition;
            pos.x = 2500;
            trans.anchoredPosition = pos;
        }

    }

    private void OnSwitchPanelTouch(SwitchPanelItem item)
    {
        for (int i = 0; i < switchPanelItems.Length; ++i)
        {
            switchPanelItems[i].SetState(false);
            if (item == switchPanelItems[i])
            {
                SetTransShowHide(m_gridScrollViews[i].transform as RectTransform, true);
            }
            else
            {
                SetTransShowHide(m_gridScrollViews[i].transform as RectTransform, false);
            }
        }

        item.SetState(true);

        // if (item == buyPanelButton)
        // {
        //     puPanelButton.SetState(false);

        //     currencyScroll.gameObject.SetActive(true);
        //     puScroll.gameObject.SetActive(false);
        // }
        // else
        // {
        //     buyPanelButton.SetState(false);

        //     currencyScroll.gameObject.SetActive(false);
        //     puScroll.gameObject.SetActive(true);
        // }
    }
    #region  character
    public class CharacterScrollData
    {
        public VideoSerilNumberManager.MainIdCompletionInfo maindata;
    }
    private void __InitCharacterList()
    {
        var charDatasTemp = VideoSerilNumberManager.Instance.GetAllMainIdCompletionInfo();

        m_gridCharacterScrollView.Clear();

        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        int itemCount = (m_gridCharacterScrollView as GridView).straightCount;
        var itemW = m_cellSize.x * itemCount + m_gridCharacterScrollView.spaceX * (itemCount - 1);
        var deltaWidth = (width - itemW) / 2;

        m_gridCharacterScrollView.paddingLeft = (int)deltaWidth;


        foreach (var it in charDatasTemp)
        {
            m_gridCharacterScrollView.AddData(new ScrollViewItemData("CharacterItem", new CharacterScrollData()
            {
                maindata = it
            }, m_cellSize));
        }
        m_gridCharacterScrollView.Refresh();
    }
    #endregion


    #region  surprise
    private void OnAlbumsLikeSwitch(int arg1, AlbumsLikeTimeSwitch sswitch)
    {
        sswitch.RevertShow();
        isShowLike = !sswitch.IsShowFirst;

        __InitSurpriseList(isTimeUp, isShowLike);
    }
    private void OnAlbumsTimeSwitch(int arg1, AlbumsLikeTimeSwitch sswitch)
    {
        sswitch.RevertShow();
        isTimeUp = sswitch.IsShowFirst;
        __InitSurpriseList(isTimeUp, isShowLike);
    }
    public class SurpriseScrollData
    {
        public SurpriseSerilMgr.SurpriseItemOut maindata;
    }
    private void __InitSurpriseList(bool ascending = false, bool onlyLiked = false)
    {
        var supriseDatasTemp = VideoSerilNumberManager.Instance.surpriseData.GetSortedListByPlayedAndTime(ascending, onlyLiked);

        m_gridSurpriseScrollView.Clear();

        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        int itemCount = (m_gridSurpriseScrollView as GridView).straightCount;
        var itemW = m_cellSize.x * itemCount + m_gridSurpriseScrollView.spaceX * (itemCount - 1);
        var deltaWidth = (width - itemW) / 2;

        m_gridSurpriseScrollView.paddingLeft = (int)deltaWidth;

        if (supriseDatasTemp == null) return;

        foreach (var it in supriseDatasTemp)
        {
            m_gridSurpriseScrollView.AddData(new ScrollViewItemData("SurpriseItem", new SurpriseScrollData()
            {
                maindata = it
            }, m_cellSize));
        }
        m_gridSurpriseScrollView.Refresh();
    }
    #endregion


    #region  special
    public class SpecialScrollData
    {
        public SpecialSerilMgr.SpecialIdxSaveOut maindata;
    }
    private void __InitSpecialList()
    {
        var specialDatasTemp = VideoSerilNumberManager.Instance.specialData.GetTotalUseFiles();

        m_gridSpecialScrollView.Clear();
        if (specialDatasTemp == null) return;

        var parentRect = transform.parent as RectTransform;
        var width = parentRect.rect.width;
        int itemCount = (m_gridSpecialScrollView as GridView).straightCount;
        var itemW = m_cellSize.x * itemCount + m_gridSpecialScrollView.spaceX * (itemCount - 1);
        var deltaWidth = (width - itemW) / 2;

        m_gridSpecialScrollView.paddingLeft = (int)deltaWidth;

        foreach (var it in specialDatasTemp)
        {
            m_gridSpecialScrollView.AddData(new ScrollViewItemData("SpecialItem", new SpecialScrollData()
            {
                maindata = it
            }, m_cellSize));
        }
        m_gridSpecialScrollView.Refresh();
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UIUtil.UI;
using UnityEngine;
using UnityEngine.UI;
using VideoSystem;
using Watermelon;

public class UISurpriseItem : ScrollViewItem
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] Image head;
    [SerializeField] Image cards;
    [SerializeField] TextMeshProUGUI remain;
    [SerializeField] TextMeshProUGUI buyNumDec;
    [SerializeField] Button buyButton;
    [SerializeField] Image buyNeedCoin;
    [SerializeField] TextMeshProUGUI priceTxt;
    [SerializeField] Button plusButton;
    [SerializeField] Button subButton;
    [SerializeField] TextMeshProUGUI addNum;

    [SerializeField] Sprite[] B_heads;
    [SerializeField] Sprite[] B_cards;
    [SerializeField] Sprite[] A_heads;
    [SerializeField] Sprite[] A_cards;

    private UISurprisePanel.SurpriseScrollData _data;
    private int _currentAddNum = 1; // 当前购买数量
    private const int MIN_ADD_NUM = 1;
    private const int MAX_ADD_NUM = 10;

    protected override void OnInit()
    {
        base.OnInit();
        // 绑定按钮事件
        plusButton.onClick.AddListener(OnPlusButtonClick);
        subButton.onClick.AddListener(OnSubButtonClick);
        buyButton.onClick.AddListener(OnBuyButtonClick);
    }

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnRefresh()
    {
        base.OnRefresh();
    }

    public override bool IsMatch(object para)
    {
        return para == _data;
    }

    protected override void OnRefreshData(int index, object data = null)
    {
        base.OnRefreshData(index, data);
        _data = (UISurprisePanel.SurpriseScrollData)data;
        _currentAddNum = 1; // 重置购买数量
        __RefreshItem();
    }

    protected override void OnRecycle()
    {
        base.OnRecycle();
    }

    protected override void OnRelease()
    {
        // 移除事件监听
        plusButton.onClick.RemoveListener(OnPlusButtonClick);
        subButton.onClick.RemoveListener(OnSubButtonClick);
        buyButton.onClick.RemoveListener(OnBuyButtonClick);
    }

    #region Private Methods

    private void __RefreshItem()
    {
        // 1. 设置标题
        title.text = _data.titletxt;

        // 2. 设置剩余数量 (currentUseIdx - 1)
        remain.text = $"Remaining:{_data.currentUseIdx - 1}";

        // 3. 根据 useB_Icon 和 iconIdx 设置 head 和 cards 的 sprite
        if (_data.useB_Icon)
        {
            // 使用 B 系列图标
            if (_data.iconIdx >= 0 && _data.iconIdx < B_heads.Length)
            {
                head.sprite = B_heads[_data.iconIdx];
            }
            if (_data.iconIdx >= 0 && _data.iconIdx < B_cards.Length)
            {
                cards.sprite = B_cards[_data.iconIdx];
            }
        }
        else
        {
            // 使用 A 系列图标
            if (_data.iconIdx >= 0 && _data.iconIdx < A_heads.Length)
            {
                head.sprite = A_heads[_data.iconIdx];
            }
            if (_data.iconIdx >= 0 && _data.iconIdx < A_cards.Length)
            {
                cards.sprite = A_cards[_data.iconIdx];
            }
        }
        head.SetNativeSize();
        // 4. 设置购买描述文本
        buyNumDec.text = string.Format("Buy {0} pictures at once.", _currentAddNum);

        // 5. 更新价格显示
        UpdatePrice();

        // 6. 更新加减按钮状态
        UpdateButtonStates();

        UpdateAddNumDisplay();

        Currency currency = CurrencyController.GetCurrency(_data.currencyType);

        buyNeedCoin.sprite = currency.Icon;
    }

    private void UpdatePrice()
    {
        int totalPrice = _currentAddNum * _data.price;
        priceTxt.text = totalPrice.ToString();
    }

    private void UpdateButtonStates()
    {
        // 控制减按钮状态（不能小于最小值）
        subButton.interactable = _currentAddNum > MIN_ADD_NUM;

        // 控制加按钮状态（不能大于最大值）
        plusButton.interactable = _currentAddNum < MAX_ADD_NUM;
    }

    #endregion

    #region UI Event Handlers

    private void OnPlusButtonClick()
    {
        if (_currentAddNum < MAX_ADD_NUM && _currentAddNum < _data.currentUseIdx - 1)
        {
            _currentAddNum++;
            UpdateAddNumDisplay();
        }
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    private void OnSubButtonClick()
    {
        if (_currentAddNum > MIN_ADD_NUM)
        {
            _currentAddNum--;
            UpdateAddNumDisplay();
        }
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    private void UpdateAddNumDisplay()
    {
        // 更新购买数量显示
        addNum.text = _currentAddNum.ToString();

        // 更新描述文本
        buyNumDec.text = string.Format("Buy {0} pictures at once.", _currentAddNum);

        // 更新价格
        UpdatePrice();

        // 更新按钮状态
        UpdateButtonStates();
    }

    private void OnBuyButtonClick()
    {
        // // TODO: 实现购买逻辑
        // // 购买数量为 _currentAddNum
        // // 总价为 _currentAddNum * _data.price
        // Debug.Log($"Buy {_currentAddNum} items, Total Price: {_currentAddNum * _data.price}");

        // // 可以在这里触发购买事件或调用外部方法
        var prices = _currentAddNum * _data.price;
        if (CurrencyController.HasAmount(_data.currencyType, prices))
        {
            CurrencyController.Substract(_data.currencyType, prices, "Buy Girl Img");

            // LivesSystem.RefillLifes();

            // UIController.HidePage<UIAddLivesPanel>();

            var unlockedIds = VideoSerilNumberManager.Instance.surpriseData.SetGetFileId(_data.mainId, _currentAddNum);
            Debug.Log($"Unlocked surprise files: {string.Join(",", unlockedIds)}");

            _data.currentUseIdx -= _currentAddNum;

            List<string> unlockIdsStr = new List<string>();
            foreach (var it in unlockedIds)
            {
                unlockIdsStr.Add(VideoSerilNumberManager.FormatSurpriseMainIdFileId(_data.mainId, it));
            }

            UIPopDownload.Show(unlockIdsStr);

            UISurprisePanel.RefreshSurpriseItem();
        }
        else
        {
            // UIController.ShowPage<UIStore>();
            SystemMessage.ShowMessage("Not enough coins or diamonds.");
        }

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 重置购买数量
    /// </summary>
    public void ResetAddNum()
    {
        _currentAddNum = MIN_ADD_NUM;
        UpdateAddNumDisplay();
    }

    #endregion
}


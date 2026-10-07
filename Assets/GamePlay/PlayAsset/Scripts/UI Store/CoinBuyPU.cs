using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

public class CoinBuyPU : MonoBehaviour
{
    [SerializeField] private PUType pUType;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countTxt;
    [SerializeField] private Button addbtn;
    [SerializeField] private Button subbtn;
    [SerializeField] private Button buyBtn;
    [SerializeField] private TextMeshProUGUI priceTxt;
    public PUType PUType { get { return pUType; } }

    private PUSettings settings;
    private int count = 1;
    private const int MIN_COUNT = 1;
    private const int MAX_COUNT = 99;

    void Start()
    {
        PUBehavior[] activePowerUps = PUController.ActivePowerUps;
        foreach (var it in activePowerUps)
        {
            if (it.Settings.Type == pUType)
            {
                settings = it.Settings;
                break;
            }
        }

        icon.sprite = settings.Icon;
        icon.SetNativeSize();

        UpdateCountDisplay();
        UpdatePriceDisplay();

        // Add button listeners
        addbtn.onClick.AddListener(OnAddButtonClicked);
        subbtn.onClick.AddListener(OnSubButtonClicked);
        buyBtn.onClick.AddListener(OnBuyButtonClicked);

        // Update button states
        UpdateButtonStates();
    }

    private void OnAddButtonClicked()
    {
        if (count < MAX_COUNT)
        {
            count++;
            UpdateCountDisplay();
            UpdatePriceDisplay();
            UpdateButtonStates();
        }
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    private void OnSubButtonClicked()
    {
        if (count > MIN_COUNT)
        {
            count--;
            UpdateCountDisplay();
            UpdatePriceDisplay();
            UpdateButtonStates();
        }
        AudioController.PlaySound(AudioController.AudioClips.buttonSound);
    }

    private void OnBuyButtonClicked()
    {
        int totalPrice = settings.Price * count;

        AudioController.PlaySound(AudioController.AudioClips.buttonSound);

        bool purchaseSuccessful = PUController.PurchasePowerUp(settings.Type, count);

        if (purchaseSuccessful)
        {
            List<IRewardPreview> rewardPreviews = new List<IRewardPreview> { new RewardPreview(settings.Icon, $"x{count}", 0, null) };
            UIRewardsConfirmation.Display(rewardPreviews, () => { });

            count = MIN_COUNT;
            UpdateCountDisplay();
            UpdatePriceDisplay();
            UpdateButtonStates();
        }
        else
        {
            SystemMessage.ShowMessage("Not enough Coin");
        }


        // // Check if user has enough currency
        // if (CurrencyController.GetCurrencyAmount(settings.CurrencyType) >= totalPrice)
        // {
        //     // Deduct currency
        //     CurrencyController.SpendCurrency(settings.CurrencyType, totalPrice);

        //     // Add the power-up with the purchased quantity
        //     PUController.AddPowerUp(pUType, count);

        //     // Optional: Show success message or play sound
        //     Debug.Log($"Purchased {count} x {pUType} for {totalPrice} {settings.CurrencyType}");

        //     // Reset count after purchase (optional)
        //     count = MIN_COUNT;
        //     UpdateCountDisplay();
        //     UpdatePriceDisplay();
        //     UpdateButtonStates();
        // }
        // else
        // {
        //     // Show insufficient currency message
        //     Debug.LogWarning($"Insufficient {settings.CurrencyType}! Need {totalPrice}, but have {CurrencyController.GetCurrencyAmount(settings.CurrencyType)}");
        //     // You might want to show a UI message here
        // }
    }

    private void UpdateCountDisplay()
    {
        countTxt.text = count.ToString();
    }

    private void UpdatePriceDisplay()
    {
        int totalPrice = settings.Price * count;
        priceTxt.text = totalPrice.ToString();
    }

    private void UpdateButtonStates()
    {
        // Disable subtract button if at minimum count
        subbtn.interactable = count > MIN_COUNT;

        // Disable add button if at maximum count
        addbtn.interactable = count < MAX_COUNT;
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        addbtn.onClick.RemoveListener(OnAddButtonClicked);
        subbtn.onClick.RemoveListener(OnSubButtonClicked);
        buyBtn.onClick.RemoveListener(OnBuyButtonClicked);
    }
}
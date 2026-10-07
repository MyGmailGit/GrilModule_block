using UnityEngine;
using UnityEngine.UI;
using System;
using Data;
using DG.Tweening;
using TMPro;
using Watermelon;

namespace GameUI
{
    public class TreasureItem : Watermelon.RewardsHolder
    {
        [SerializeField] private Button button;
        [SerializeField] private Watermelon.IAPButton buttonIAP;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;

        [SerializeField] private Image[] getIcons;

        [SerializeField] private GameObject vipTag;

        // [SerializeField] private Sprite coinSprite;
        // [SerializeField] private Sprite hammerSprite;
        // [SerializeField] private Sprite frezeSprite;
        // [SerializeField] private Sprite magnetSprite;

        TreasureData data;

        private Tween moveTween;
        private Tween fadeTween;
        private Tween scaleTween;

        private Action clickCallback;

        private bool isSubscribe;

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            // 

            clickCallback?.Invoke();

        }

        private void InitItem()
        {
            if (data.TreasureType == TreasureType.Iap)
            {
                buttonIAP.gameObject.SetActive(true);
                button.gameObject.SetActive(false);

                buttonIAP.Init(data.ProductKeyType);
            }
            else
            {
                buttonIAP.gameObject.SetActive(false);
                button.gameObject.SetActive(true);
            }

            SetGetView();
        }


        public void SetClickable(bool clickable, Action callback)
        {
            button.interactable = clickable;
            buttonIAP.Button.interactable = clickable;
            clickCallback = clickable ? callback : null;
        }

        public void SetPositionInstant(RectTransform target, bool isCenter)
        {
            rectTransform.position = target.position;
            rectTransform.localScale = isCenter ? Vector3.one * 0.95f : Vector3.one * 0.9f;
        }
        public void MoveTo(RectTransform target, bool isCenter)
        {
            moveTween?.Kill();
            moveTween = null;
            scaleTween?.Kill();
            scaleTween = null;

            // rectTransform.DOLocalMove(target.localPosition, 0.35f).SetEase(Ease.OutCubic);

            // rectTransform.DOLocalMove(target.localPosition, 0.35f).SetEase(DG.Tweening.Ease.OutCubic);

            moveTween = rectTransform
                .DOMove(target.position, 0.35f)
                .SetEase(Ease.OutCubic);

            scaleTween = rectTransform
                .DOScale(isCenter ? 0.95f : 0.9f, 0.35f)
                .SetEase(Ease.OutCubic);
        }

        public Tween PlayDisappearAnim()
        {
            fadeTween?.Kill();
            fadeTween = null;

            return fadeTween = canvasGroup.DOFade(0, 0.2f)
                .SetEase(Ease.OutQuad).OnComplete(() => { data.RewardsSet.ApplyReward(this.isSubscribe ? 2 : 1); });
        }

        public Tween PlayAppearAnim()
        {
            fadeTween?.Kill();
            fadeTween = null;

            canvasGroup.alpha = 0;

            return fadeTween = canvasGroup.DOFade(1, 0.25f).SetEase(Ease.OutQuad);
        }

        public void SetData(TreasureData data, bool isSubscribe)
        {
            this.isSubscribe = isSubscribe;

            gameObject.SetActive(true);

            SetRewardSet(data.RewardsSet);
            this.data = data;
            InitItem();
            InitializeComponents();
            Watermelon.IAPManager.SubscribeOnPurchaseModuleInitted(OnIAPManagerLoaded);

            if (vipTag != null) vipTag.SetActive(isSubscribe);
        }

        public void SetEmpty()
        {
            gameObject.SetActive(false);
            this.data = null;
        }
        public bool IsEmpty()
        {
            if (data == null && !gameObject.activeSelf)
            {
                return true;
            }
            return false;
        }


        private void SetGetView()
        {
            foreach (Image img in getIcons)
                img.gameObject.SetActive(false);

            int getIconIndex = 0;
            for (int i = 0; i < data.RewardsSet.Rewards.Count; i++)
            {
                var reward = data.RewardsSet.Rewards[i];
                if (reward == null) continue;
                reward.GetRewardPreviews(this.isSubscribe ? 2 : 1).ForEach(r =>
                {
                    if (r != null)
                    {
                        getIcons[getIconIndex].sprite = r.Icon;
                        getIcons[getIconIndex].GetComponentInChildren<TextMeshProUGUI>().text = r.Text;
                        getIcons[getIconIndex].gameObject.SetActive(true);
                        getIconIndex++;
                    }
                });
            }
            if (getIconIndex > 1)
            {
                foreach (Image img in getIcons)
                {
                    img.rectTransform.sizeDelta = new Vector2(60, 60);
                }
            }
        }

        private void OnEnable()
        {
            // Subscribe to purchase callback
            Watermelon.IAPManager.PurchaseCompleted += OnPurchaseComplete;
        }

        private void OnDisable()
        {
            // Unsubscribe from purchase callback
            Watermelon.IAPManager.PurchaseCompleted -= OnPurchaseComplete;
        }
        private void OnIAPManagerLoaded()
        {
            // Get the product save file to check if it was previously purchased
            // This data is stored only locally so after the game reinstall it will be reset
            var save = Watermelon.SaveController.GetSaveObject<Watermelon.IAPItem.Save>($"iap_{data.ProductKeyType}");

            // Get product data wrapper
            // To acess Unity IAP product use product.Product property
            var product = Watermelon.IAPManager.GetProductData(data.ProductKeyType);

            // Update button state
            // If there is problem with the internet connection or server didn't return product data loading animation appeared
            buttonIAP.UpdateState(product);

            // if (Watermelon.IAPManager.IsPurchased(data.ProductKeyType) || product.ProductType == Watermelon.ProductType.NonConsumable && save.IsPurchased)
            // {
            //     // // Disable holder if it's an one time purchase (non-consumable) product 
            //     // if (product.ProductType == Watermelon.ProductType.NonConsumable)
            //     // {
            //     //     // Disable holder game object
            //     //     gameObject.SetActive(false);

            //     //     return;
            //     // }
            // }

            // Check if holder needs to be disabled
            if (CheckDisableState())
            {
                // Disable holder game object
                gameObject.SetActive(false);
            }
        }

        private void OnPurchaseComplete(Watermelon.ProductKeyType key)
        {
            if (!isPageActive) return;

            clickCallback?.Invoke();

            // Check if holder needs to be disabled
            if (CheckDisableState())
            {
                // Disable holder game object
                gameObject.SetActive(false);
            }
        }
    }
}
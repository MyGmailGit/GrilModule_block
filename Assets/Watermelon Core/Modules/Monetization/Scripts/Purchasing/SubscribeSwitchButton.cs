using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
namespace Watermelon
{
    public class SubscribeSwitchButton : MonoBehaviour
    {
        [SerializeField] Image chooseImg;
        [SerializeField] Button button;
        [SerializeField] TMP_Text priceText;
        private ProductKeyType key;

        public Action<SubscribeSwitchButton, ProductKeyType> OnClicked;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public void Init(ProductKeyType key)
        {
            this.key = key;
            chooseImg.gameObject.SetActive(false);

            UpdateState();
        }

        public void UpdateState()
        {
            UpdateState(IAPManager.GetProductData(key));
        }

        public void UpdateState(ProductData product)
        {
            if (product != null)
            {
                priceText.gameObject.SetActive(true);

                if (product.Price != 0.01m)
                {
                    priceText.text = product.GetLocalPrice();
                }
                else
                {
                    IAPItem iapItem = IAPManager.GetIAPItem(key);
                    if (iapItem != null)
                    {
                        priceText.text = $"USD {iapItem.DefaultUSDPrice}";
                    }
                    else
                    {
                        priceText.text = product.GetLocalPrice();
                    }
                }
            }
        }
        private void OnButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif
            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            chooseImg.gameObject.SetActive(true);
            OnClicked?.Invoke(this, key);
        }

        public void SetChosen(bool value)
        {
            chooseImg.gameObject.SetActive(value);
        }
    }
}
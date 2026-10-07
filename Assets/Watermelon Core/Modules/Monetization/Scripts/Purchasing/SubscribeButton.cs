using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.IAPStore;

namespace Watermelon
{
    public class SubscribeButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text priceText;
        [SerializeField] GameObject loadingObject;

        private ProductKeyType key;

        private void Awake()
        {
            button.onClick.AddListener(OnButtonClicked);
        }

        public void SetProducktKey(ProductKeyType key)
        {
            this.key = key;
            UpdateState(IAPManager.GetProductData(key));
        }

        public void UpdateState(ProductData product)
        {
            if (loadingObject == null || priceText == null)
            {
                Debug.LogWarning($"[IAPButton] UI references are not assigned. Skipping UpdateState. Key: {key}");

                return;
            }

            if (product != null)
            {
                loadingObject.SetActive(false);
                priceText.gameObject.SetActive(true);

                if (product.Price != 0.01m)
                {
                    priceText.text = GetSubBtnPrice(product.GetLocalPrice());
                }
                else
                {
                    IAPItem iapItem = IAPManager.GetIAPItem(key);
                    if (iapItem != null)
                    {
                        priceText.text = GetSubBtnPrice($"USD {iapItem.DefaultUSDPrice}");
                    }
                    else
                    {
                        priceText.text = GetSubBtnPrice(product.GetLocalPrice());
                    }
                }
            }
        }
        private string GetSubBtnPrice(string price)
        {

            return $"{price}/{UIStoreSubscribe.GetDescription(this.key)[1]}";

        }

        private void OnButtonClicked()
        {
#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_HARD);
#endif

            AudioController.PlaySound(AudioController.AudioClips.buttonSound);

            IAPManager.BuyProduct(key);
        }
    }
}
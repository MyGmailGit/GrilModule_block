using UnityEngine;
using Watermelon;

namespace GameLogic
{
    public class BannerBottom : MonoBehaviour
    {
        private Vector2 originOffsetMin;
        private Vector2 originOffsetMax;

        void Start()
        {
            var rectTransform = transform as RectTransform;
            originOffsetMin = rectTransform.offsetMin;
            originOffsetMax = rectTransform.offsetMax;
            UpdateBannerHeightRatio(AdsManager.GetADBannerHeightRatio());
            AdsManager.onBannerHeightChanged += UpdateBannerHeightRatio;
        }

        void OnDestroy()
        {
            AdsManager.onBannerHeightChanged -= UpdateBannerHeightRatio;
        }

        private void UpdateBannerHeightRatio(float heightRatio)
        {
            if (UIController.Instance != null && UIController.MainCanvas != null)
            {
                float canvasHeight = (UIController.MainCanvas.transform as RectTransform).sizeDelta.y;
                float y = canvasHeight * heightRatio;

                var rectTransform = transform as RectTransform;
                var offsetMin = originOffsetMin;
                var offsetMax = originOffsetMax;
                offsetMin.y += y * (1 - rectTransform.anchorMin.y);
                offsetMax.y += y * (1 - rectTransform.anchorMax.y);
                rectTransform.offsetMin = offsetMin;
                rectTransform.offsetMax = offsetMax;
            }
        }
    }
}
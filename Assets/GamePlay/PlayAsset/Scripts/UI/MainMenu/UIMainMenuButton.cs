#pragma warning disable 0618

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Watermelon
{
    [System.Serializable]
    public class UIMainMenuButton
    {
        [SerializeField] RectTransform rect;
        [SerializeField] Button button;
        public Button Button => button;

        [Space]
        [SerializeField] AnimationCurve showStoreAdButtonsCurve;
        [SerializeField] AnimationCurve hideStoreAdButtonsCurve;
        [SerializeField] float showHideDuration;

        private float savedRectPosX;
        private float rectXPosBehindOfTheScreen;

        private Tween showHideCase;

        public void Init(float rectXPosBehindOfTheScreen)
        {
            this.rectXPosBehindOfTheScreen = rectXPosBehindOfTheScreen;
            savedRectPosX = rect.anchoredPosition.x;
        }

        public void Show(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.IsActive()) return;

            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);
                return;
            }

            //RESET
            rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);

            showHideCase = rect.DOAnchorPos(rect.anchoredPosition.SetX(savedRectPosX), showHideDuration).SetEase(showStoreAdButtonsCurve);
        }

        public void Hide(bool immediately = false)
        {
            if (showHideCase != null && showHideCase.IsActive()) return;

            if (immediately)
            {
                rect.anchoredPosition = rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen);
                return;
            }

            //RESET
            rect.anchoredPosition = rect.anchoredPosition.SetX(savedRectPosX);

            showHideCase = rect.DOAnchorPos(rect.anchoredPosition.SetX(rectXPosBehindOfTheScreen), showHideDuration).SetEase(hideStoreAdButtonsCurve);
        }
    }
}

using UnityEngine;
using Watermelon;

namespace GameLogic
{
    public class SafeTop : MonoBehaviour
    {
        void Start()
        {
            var topRect = gameObject.transform as RectTransform;
            if (topRect != null)
            {
                //var anchoredPosition = topRect.anchoredPosition;
                //anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y - _notchHeight);
                //topRect.anchoredPosition = anchoredPosition;

                // var canvas = GameModule.UI.UICanvas;
                // if (canvas != null)
                if (UIController.Instance != null && UIController.MainCanvas != null)
                {
                    float canvasHeight = (UIController.MainCanvas.transform as RectTransform).sizeDelta.y;
                    float rate = GetNotchHeightRate();
                    float y = canvasHeight * rate;

                    var offset = topRect.offsetMax;
                    offset.y -= y;
                    topRect.offsetMax = offset;
                }
            }
        }

        internal float GetNotchHeightRate()
        {
            var notchHeight = Screen.height - (Screen.safeArea.y + Screen.safeArea.height);
            return notchHeight / Screen.height;
        }
    }
}
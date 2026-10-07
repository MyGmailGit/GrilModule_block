using UnityEngine;
using DG.Tweening;
namespace Watermelon
{
    [RequireComponent(typeof(Canvas))]
    public class FadeOverlayPanel : MonoBehaviour, IOverlayPanel
    {
        [SerializeField] Ease showEasingType;
        [SerializeField] Ease hideEasingType;

        [Space]
        [SerializeField] GameObject loadingObject;

        private CanvasGroup canvasGroup;
        private Tween tweenCase;
        private Canvas canvas;

        public void Init()
        {
            canvas = gameObject.GetComponent<Canvas>();

            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.0f;
        }

        public void Show(float duration, SimpleCallback onCompleted)
        {
            tweenCase.Kill();
            tweenCase = canvasGroup.DOFade(1.0f, duration).SetEase(showEasingType).OnComplete(() => { onCompleted?.Invoke(); });
        }

        public void Hide(float duration, SimpleCallback onCompleted)
        {
            tweenCase.Kill();
            tweenCase = canvasGroup.DOFade(0.0f, duration).SetEase(hideEasingType).OnComplete(() => { onCompleted?.Invoke(); });
        }

        public void Clear()
        {
            tweenCase.Kill();
        }

        public void SetState(bool state)
        {
            canvas.enabled = state;
        }

        public void SetLoadingState(bool state)
        {
            if (loadingObject != null)
            {
                loadingObject.gameObject.SetActive(state);
            }
        }
    }
}

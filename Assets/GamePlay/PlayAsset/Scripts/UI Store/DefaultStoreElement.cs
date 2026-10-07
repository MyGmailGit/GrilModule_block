using UnityEngine;
using DG.Tweening;


namespace Watermelon.IAPStore
{
    public class DefaultStoreElement : IStoreElement
    {
        private RectTransform rectTransform;
        private Tween tweenCase;

        private GameObject gameObject;

        public bool IsActive => gameObject != null && gameObject.activeSelf;
        public float Height => rectTransform.sizeDelta.y;

        private SimpleCallback completeCallback;

        public DefaultStoreElement(RectTransform rectTransform)
        {
            this.rectTransform = rectTransform;

            gameObject = rectTransform.gameObject;
        }

        public void Init()
        {

        }

        public void KillTweenCases()
        {
            tweenCase.Kill();
        }

        public void PlayAnimation(int elementIndex)
        {
            rectTransform.localScale = Vector3.zero;

            tweenCase = rectTransform.DOScale(1.0f, 0.3f).SetEase(Ease.OutCirc).OnComplete(() => { completeCallback.Invoke(); });
        }

        public void OnComplete(SimpleCallback completeCallback)
        {
            this.completeCallback = completeCallback;
        }
    }
}
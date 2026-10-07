#pragma warning disable 0618

using UnityEngine;
using DG.Tweening;

namespace Watermelon
{
    public class FloatingTextBehavior : FloatingTextBaseBehavior
    {
        [Space]
        [SerializeField] Vector3 offset;
        [SerializeField] float time;
        [SerializeField] Ease easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] AnimationCurve scaleAnimationCurve;

        private Vector3 defaultScale;

        private Tween scaleTween;
        private Tween moveTween = null;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            textRef.text = text;
            textRef.color = color;

            transform.localScale = Vector3.zero;
            scaleTween = transform.DOScale(defaultScale * scaleMultiplier, scaleTime).SetEase(scaleAnimationCurve);
            moveTween = transform.DOMove(transform.position + offset, time).SetEase(easing).OnComplete(delegate
             {
                 gameObject.SetActive(false);

                 InvokeCompleteEvent();
                 moveTween = null;
             });
        }

        public void AddOnTimeReached(float time, SimpleCallback callback)
        {
            if (moveTween != null && moveTween.IsActive())
            {
                // moveTween.OnTimeReached(time, callback);
            }
        }

        public void SetText(string text)
        {
            textRef.text = text;
        }

        public void Reset()
        {
            scaleTween.Kill();
            moveTween.Kill();
        }
    }
}
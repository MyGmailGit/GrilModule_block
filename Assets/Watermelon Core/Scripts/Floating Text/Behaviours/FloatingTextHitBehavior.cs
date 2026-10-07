using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace Watermelon
{
    public class FloatingTextHitBehavior : FloatingTextBaseBehavior
    {
        [SerializeField] TextMeshProUGUI floatingText;

        [Space]
        [SerializeField] float delay;
        [SerializeField] float disableDelay;
        [SerializeField] float startScale;
        [SerializeField] float time;
        [SerializeField] Ease easing;

        [Space]
        [SerializeField] float scaleTime;
        [SerializeField] Ease scaleEasing;

        private Vector3 defaultScale;

        private void Awake()
        {
            defaultScale = transform.localScale;
        }

        public override void Activate(string text, float scaleMultiplier, Color color)
        {
            floatingText.text = text;
            floatingText.color = color;

            int sign = Random.value >= 0.5f ? 1 : -1;

            transform.localScale = defaultScale * startScale * scaleMultiplier;
            transform.localRotation = Quaternion.Euler(70, 0, 18 * sign);

            DOVirtual.DelayedCall(delay, delegate
            {
                transform.DOLocalRotateQuaternion(Quaternion.Euler(70, 0, 0), time).SetEase(easing).OnComplete(delegate
                {
                    DOVirtual.DelayedCall(disableDelay, delegate
                    {
                        gameObject.SetActive(false);

                        InvokeCompleteEvent();
                    });
                });

                transform.DOScale(defaultScale, scaleTime).SetEase(scaleEasing);
            });
        }
    }
}
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace Watermelon
{
    public class ChainVisualsBehavior : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI amountText;
        public TextMeshProUGUI AmountText => amountText;

        [SerializeField] GameObject lockObject;
        public GameObject LockObject => lockObject;

        [SerializeField] Transform shackleTransform;

        [SerializeField] GameObject chainsObject;
        public GameObject ChainsObject => chainsObject;

        [SerializeField] ParticleSystem keyParticle;
        public ParticleSystem KeyParticle => keyParticle;

        [SerializeField] ParticleSystem destroyParticle;
        public ParticleSystem DestroyParticle => destroyParticle;

        private Tween rotateTweenCase;

        public void PlayOpenAnimation(SimpleCallback completeCallback)
        {
            rotateTweenCase.Kill();

            shackleTransform.localRotation = Quaternion.identity;
            rotateTweenCase = shackleTransform.DOLocalRotateQuaternion(Quaternion.Euler(0, -25, 0), 0.15f).OnComplete(() =>
            {
                rotateTweenCase = shackleTransform.DOLocalRotateQuaternion(Quaternion.Euler(-5, 25, -170), 0.25f).OnComplete(() =>
                {
                    lockObject.SetActive(false);
                    chainsObject.SetActive(false);

                    completeCallback?.Invoke();
                });
            });
        }

        private void OnDestroy()
        {
            rotateTweenCase.Kill();
        }
    }
}

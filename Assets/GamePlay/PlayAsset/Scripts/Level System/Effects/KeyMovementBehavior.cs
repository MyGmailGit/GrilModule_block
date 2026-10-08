using UnityEngine;
using DG.Tweening;
namespace Watermelon
{
    [System.Serializable]
    public class KeyMovementBehavior : MonoBehaviour
    {
        [SerializeField] float yOffset = 0.5f;
        [SerializeField] float yDuration = 0.3f;

        [Space]
        [SerializeField] float moveDuration = 0.3f;
        [SerializeField] Vector3 moveOffset;
        [SerializeField] Ease moveEasing;

        [Space]
        [SerializeField] Vector3 baseRotation;
        [SerializeField] float baseRotationDuration = 0.2f;
        [SerializeField] Vector3 targetRotation;
        [SerializeField] float targetRotationDuration = 0.2f;

        private TweenCaseCollection tweenCaseCollection;

        private IChainElement linkedChainElement;
        private bool isLinked;

        private void Update()
        {
            if (!isLinked) return;

            if (linkedChainElement == null || linkedChainElement.ChainVisualsBehavior == null)
            {
                tweenCaseCollection.Kill();

                Destroy(transform.gameObject);
            }
        }

        public void StartMovement(IChainElement chainElement)
        {
            isLinked = true;
            linkedChainElement = chainElement;

            tweenCaseCollection.Kill();
            tweenCaseCollection = new TweenCaseCollection();

            chainElement?.OnKeyLinked();

            transform.SetParent(null);

            Vector3 localPosition = transform.localPosition;
            tweenCaseCollection += transform.DOLocalMoveY(localPosition.y + yOffset, yDuration).OnComplete(() =>
            {
                if (linkedChainElement == null || linkedChainElement.ChainVisualsBehavior == null)
                {
                    tweenCaseCollection.Kill();

                    Destroy(transform.gameObject);

                    return;
                }

                Vector3 lockPosition = linkedChainElement.ChainVisualsBehavior.LockObject.transform.position + moveOffset;
                tweenCaseCollection += transform.DORotateQuaternion(Quaternion.Euler(baseRotation), baseRotationDuration);
                // tweenCaseCollection += transform.DOBezierMove(lockPosition, 1.5f, 0, 0, moveDuration).SetEasing(moveEasing).OnComplete(() =>
                tweenCaseCollection += transform.DOMove(lockPosition, 1.5f).SetDelay(moveDuration).SetEase(moveEasing).OnComplete(() =>
                {
                    tweenCaseCollection += transform.DORotateQuaternion(Quaternion.Euler(targetRotation), targetRotationDuration).SetDelay(0.1f).OnComplete(() =>
                    {
                        tweenCaseCollection += DOVirtual.DelayedCall(0.2f, () =>
                        {
                            linkedChainElement?.OnKeyReached();

                            Destroy(transform.gameObject);
                        });
                    });
                });
            });
        }

        private void OnDestroy()
        {
            tweenCaseCollection.Kill();
        }
    }
}

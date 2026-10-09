using UnityEngine;
using DG.Tweening;
namespace Watermelon
{
    [System.Serializable]
    public class ScissorsMovementBehavior : MonoBehaviour
    {
        [SerializeField] MeshRenderer leftHalfMeshRenderer;
        [SerializeField] MeshRenderer rightHalfMeshRenderer;

        [Space]
        [SerializeField] float yOffset = 0.5f;
        [SerializeField] float yDuration = 0.3f;

        [Space]
        [SerializeField] float moveDuration = 0.3f;
        [SerializeField] Vector3 moveOffset;
        [SerializeField] float rotationDuration = 0.3f;
        [SerializeField] Vector3 moveRotation = new Vector3(-54, 0, 0);

        [Space]
        [SerializeField] float baseRotation;
        [SerializeField] float baseRotationDuration = 0.2f;

        [Space]
        [SerializeField] float targetRotation;
        [SerializeField] float targetRotationDuration = 0.2f;

        private TweenCaseCollection tweenCaseCollection;
        private RopeBehavior linkedRopeBehavior;
        private bool isLinked;

        private MaterialPropertyBlock propertyBlock;

        public void Init(BlockColorData colorData)
        {
            propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", colorData.Color);

            leftHalfMeshRenderer.SetPropertyBlock(propertyBlock);
            rightHalfMeshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void Update()
        {
            if (!isLinked) return;

            if (linkedRopeBehavior == null)
            {
                tweenCaseCollection?.Kill();

                Destroy(transform.gameObject);
            }
        }

        public void StartMovement(RopeBehavior ropeBehavior)
        {
            isLinked = true;
            linkedRopeBehavior = ropeBehavior;

            tweenCaseCollection?.Kill();
            tweenCaseCollection = new TweenCaseCollection();

            ropeBehavior.OnRopeLinked();

            transform.SetParent(null);

            Vector3 localPosition = transform.localPosition;
            tweenCaseCollection += transform.DOLocalMoveY(localPosition.y + yOffset, yDuration).OnComplete(() =>
            {
                Vector3 ropePosition = ropeBehavior.transform.position + moveOffset;
                tweenCaseCollection += transform.DORotateQuaternion(Quaternion.Euler(moveRotation), rotationDuration);
                tweenCaseCollection += transform.DOMove(ropePosition, moveDuration).OnComplete(() =>
                {
                    tweenCaseCollection += leftHalfMeshRenderer.transform.DORotateQuaternion(Quaternion.Euler(0, baseRotation, 0), baseRotationDuration);
                    tweenCaseCollection += rightHalfMeshRenderer.transform.DORotateQuaternion(Quaternion.Euler(0, -baseRotation, 0), baseRotationDuration).OnComplete(() =>
                    {
                        tweenCaseCollection += leftHalfMeshRenderer.transform.DORotateQuaternion(Quaternion.Euler(0, targetRotation, 0), targetRotationDuration);
                        tweenCaseCollection += rightHalfMeshRenderer.transform.DORotateQuaternion(Quaternion.Euler(0, -targetRotation, 0), targetRotationDuration).OnComplete(() =>
                        {
                            ropeBehavior.OnRopeCut();

                            Destroy(transform.gameObject);
                        });
                    });
                });
            });
        }

        private void OnDestroy()
        {
            tweenCaseCollection?.Kill();
        }
    }
}

using UnityEngine;

namespace Watermelon
{
    public sealed class KeyEffectBehavior : BlockEffectBehavior
    {
        [SerializeField] KeyMovementBehavior keyBehavior;

        [Space]
        [SerializeField] float offsetY = 0.1f;

        private bool isKeyCollected;

        public override int EffectSortingOrder => 2;

        public override void OnCreated(LevelBlockBehavior blockBehavior)
        {
            Bounds bounds = blockBehavior.Figure.GetVerticalCenterBounds();

            transform.position = blockBehavior.transform.position + bounds.center + new Vector3(0, offsetY * orderID, 0);
        }

        private void CollectKey()
        {
            if (isKeyCollected) return;

            isKeyCollected = true;

            IChainElement chainElement = ChainManager.GetChainElement();
            if (chainElement != null)
            {
                keyBehavior.StartMovement(chainElement);
            }

            DisableEffect();
        }

        public override void OnBlockCollected()
        {
            CollectKey();
        }

        public override bool CanBeReapplied()
        {
            return false;
        }
    }
}

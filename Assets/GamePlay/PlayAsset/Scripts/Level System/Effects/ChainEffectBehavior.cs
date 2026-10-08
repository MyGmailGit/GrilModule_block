using UnityEngine;

namespace Watermelon
{
    public sealed class ChainEffectBehavior : BlockEffectBehavior, IChainElement
    {
        [SerializeField] ChainVisualsData[] chainVisualsData;

        [Space]
        [SerializeField] float offsetY = 0.1f;

        private int keysAmount;
        public int KeysLeft => keysAmount;

        private int visualKeysAmount;

        public Transform Transform => transform;

        private ChainVisualsBehavior chainVisualsBehavior;
        public ChainVisualsBehavior ChainVisualsBehavior => chainVisualsBehavior;

        public override int EffectSortingOrder => 2;

        public override void OnCreated(LevelBlockBehavior blockBehavior)
        {
            keysAmount = effectData.keysAmount;
            visualKeysAmount = keysAmount;

            ChainVisualsData visualsData = GetVisualsData(blockBehavior.BlockData.Type);
            if (visualsData != null)
            {
                GameObject visualsObject = Instantiate(visualsData.VisualsPrefab, transform);
                visualsObject.transform.localPosition = Vector3.zero;

                chainVisualsBehavior = visualsObject.GetComponent<ChainVisualsBehavior>();
            }
            else
            {
                DisableEffect();

                return;
            }

            Bounds bounds = blockBehavior.Figure.GetHorizontalCenterBounds();

            transform.position = blockBehavior.transform.position + bounds.center + visualsData.VisualsOffset + new Vector3(0, offsetY * orderID, 0);

            chainVisualsBehavior.AmountText.text = keysAmount.ToString();

            ChainManager.RegisterElement(this);
        }

        public override void OnDisabled(LevelBlockBehavior blockBehavior)
        {
            ChainManager.UnregisterElement(this);
        }

        public override bool IsClickable()
        {
            return false;
        }

        public void OnKeyLinked()
        {
            keysAmount--;
        }

        public void OnKeyReached()
        {
            visualKeysAmount--;

            chainVisualsBehavior.AmountText.text = visualKeysAmount.ToString();

            chainVisualsBehavior.KeyParticle.Play();

            if (visualKeysAmount <= 0)
            {
                chainVisualsBehavior.transform.SetParent(null);
                chainVisualsBehavior.PlayOpenAnimation(() =>
                {
                    ParticleCase destroyParticleCase = chainVisualsBehavior.DestroyParticle.PlayCase();
                    destroyParticleCase.Disabled += () =>
                    {
                        Destroy(chainVisualsBehavior.gameObject);
                    };

                    DisableEffect();
                });
            }

            AudioController.PlaySound(AudioController.AudioClips.actionDone);
        }

        private ChainVisualsData GetVisualsData(BlockType blockType)
        {
            for (int i = 0; i < chainVisualsData.Length; i++)
            {
                if (chainVisualsData[i].BlockType == blockType)
                {
                    return chainVisualsData[i];
                }
            }

            Debug.LogError($"Chain visuals data not found for element type: {blockType} in {gameObject.name}.", gameObject);

            return null;
        }

        public override bool CanBeReapplied()
        {
            return false;
        }

        public override BlockEffectData GetCurrentEffectData()
        {
            return new BlockEffectData(effectData) { keysAmount = keysAmount };
        }

        [System.Serializable]
        public class ChainVisualsData
        {
            [SerializeField] BlockType blockType;
            public BlockType BlockType => blockType;

            [SerializeField] GameObject visualsPrefab;
            public GameObject VisualsPrefab => visualsPrefab;

            [SerializeField] Vector3 visualsOffset;
            public Vector3 VisualsOffset => visualsOffset;
        }
    }
}

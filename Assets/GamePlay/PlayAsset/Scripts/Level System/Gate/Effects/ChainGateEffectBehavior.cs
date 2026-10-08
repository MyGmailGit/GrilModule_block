using UnityEngine;

namespace Watermelon
{
    public sealed class ChainGateEffectBehavior : GateEffectBehavior, IChainElement
    {
        [SerializeField] ChainVisualsData[] chainVisualsData;

        private int keysAmount;
        public int KeysLeft => keysAmount;

        private int visualKeysAmount;

        public Transform Transform => transform;

        private ChainVisualsBehavior chainVisualsBehavior;
        public ChainVisualsBehavior ChainVisualsBehavior => chainVisualsBehavior;

        public override void OnCreated(GateBehavior gateBehavior)
        {
            keysAmount = data.KeysAmount;
            visualKeysAmount = keysAmount;

            ChainVisualsData visualsData = GetVisualsData(gateBehavior.Data.UnifiedElements.Count);
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

            transform.localRotation = Quaternion.identity;
            chainVisualsBehavior.LockObject.transform.rotation = Quaternion.Euler(0, 180, 0);

            chainVisualsBehavior.AmountText.text = keysAmount.ToString();

            ChainManager.RegisterElement(this);
        }

        public override void OnDisabled(GateBehavior gateBehavior)
        {
            ChainManager.UnregisterElement(this);
        }

        public override bool CanGoThroughGate(LevelBlockBehavior levelBlockBehavior)
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
        }

        private ChainVisualsData GetVisualsData(int gateSize)
        {
            ChainVisualsData tempVisualData = null;
            int closestDifference = int.MaxValue;

            for (int i = 0; i < chainVisualsData.Length; i++)
            {
                int difference = Mathf.Abs(chainVisualsData[i].GateSize - gateSize);
                if(difference < closestDifference)
                {
                    closestDifference = difference;
                    tempVisualData = chainVisualsData[i];
                }
            }

            return tempVisualData;
        }

        [System.Serializable]
        public class ChainVisualsData
        {
            [SerializeField] int gateSize;
            public int GateSize => gateSize;

            [SerializeField] GameObject visualsPrefab;
            public GameObject VisualsPrefab => visualsPrefab;
        }
    }
}
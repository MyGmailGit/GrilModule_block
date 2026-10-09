using TMPro;
using UnityEngine;

namespace Watermelon
{
    public sealed class IceEffectBehavior : BlockEffectBehavior
    {
        [SerializeField] TextMeshProUGUI turnsText;
        [SerializeField] Material iceMaterial;

        [Space]
        [SerializeField] AudioClip turnSound;
        [SerializeField] AudioClip disableSound;

        [Space]
        [SerializeField] ParticleSystem turnParticle;
        [SerializeField] ParticleSystem disableParticle;

        private Material storedMaterial;
        private Material meshMaterial;
        private int turnsLeft;
        private int lastCollectedFrame = -1;

        public override int EffectSortingOrder => 10;

        private static AudioClipHandler soundHandler;

        public override void OnCreated(LevelBlockBehavior blockBehavior)
        {
            if (!Application.isPlaying)
            {
                meshMaterial = new Material(blockBehavior.MeshRenderer.sharedMaterial);
            }
            else
            {
                meshMaterial = blockBehavior.MeshRenderer.material;
            }

            storedMaterial = meshMaterial;
            blockBehavior.MeshRenderer.material = iceMaterial;

            Bounds bounds = blockBehavior.Figure.GetHorizontalCenterBounds();
            transform.position = blockBehavior.transform.position + bounds.center;

            turnsLeft = effectData.iceTurnsAmount;
            turnsText.text = turnsLeft.ToString();

            foreach (BlockEffectBehavior effect in linkedBlock.Effects)
            {
                if (effect == this) continue;

                if (effect.IsActive)
                    effect.gameObject.SetActive(false);
            }
        }

        public override void OnDisabled(LevelBlockBehavior blockBehavior)
        {
            blockBehavior.MeshRenderer.material = storedMaterial;
        }

        public override void OnBlockCollectedGlobal(LevelBlockBehavior levelBlockBehavior)
        {
            if (lastCollectedFrame == Time.frameCount) return;

            lastCollectedFrame = Time.frameCount;
            turnsLeft--;

            if (turnsLeft <= 0)
            {
                if (disableSound != null)
                    PlaySound(disableSound);

                if (disableParticle != null)
                {
                    disableParticle.transform.SetParent(null);
                    disableParticle.PlayCase().Disabled += () =>
                    {
                        Destroy(disableParticle.gameObject);
                    };
                }

                DisableEffect();

                foreach (BlockEffectBehavior effect in linkedBlock.Effects)
                {
                    if (effect.IsActive)
                    {
                        effect.gameObject.SetActive(true);
                    }
                }
            }
            else
            {
                if (turnSound != null)
                    PlaySound(turnSound);

                if (turnParticle != null)
                    turnParticle.Play();

                turnsText.text = turnsLeft.ToString();
            }
        }

        public override void OnNewEffectAddedToBlock(BlockEffectBehavior effect)
        {
            if (turnsLeft == 0) return;

            // Turn off visuals of the effect that was added to the block
            effect.gameObject.SetActive(false);
        }

        public override bool IsClickable()
        {
            return false;
        }

        public override BlockEffectData GetCurrentEffectData()
        {
            return new BlockEffectData(effectData) { iceTurnsAmount = turnsLeft };
        }

        private static void PlaySound(AudioClip sound)
        {
            if (soundHandler == null)
            {
                soundHandler = new AudioClipHandler(AudioType.Sound, 1.0f, 0.1f);
            }

            soundHandler.Play(sound);
        }
    }
}

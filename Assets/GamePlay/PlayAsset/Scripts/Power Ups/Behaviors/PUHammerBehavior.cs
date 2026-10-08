using UnityEngine;

namespace Watermelon
{
    public class PUHammerBehavior : PUBehavior
    {
        private const string TIMER_UNIQUE_NAME = "hammer";

        [SerializeField] ParticleSystem hitParticle;
        [SerializeField] HammerAnimationBehavior hammerAnimation;
        [SerializeField] AudioClip hammerHitSound;

        public override void Init()
        {
            hammerAnimation.Init(this);
        }

        public override bool Activate()
        {
            return true;
        }

        public override bool ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition)
        {
            if (clickableObject is LevelBlockBehavior levelBlockBehavior)
            {
                if (levelBlockBehavior != null && !levelBlockBehavior.IsCollected)
                {
                    IsBusy = true;

                    LevelController.GameplayTimer.Pause(TIMER_UNIQUE_NAME);

                    Vector3 centerPosition = levelBlockBehavior.transform.position + levelBlockBehavior.Figure.GetHorizontalCenterBounds().center;

                    ParticleSystem.MainModule main = hitParticle.main;
                    main.startColor = levelBlockBehavior.ColorData.Color;

                    levelBlockBehavior.OnBlockCollected();

                    LevelRepresentation levelRepresentation = LevelController.LevelRepresentation;

                    LevelController.OnBlockDestructed(levelBlockBehavior);

                    hammerAnimation.PlayHitAnimation(centerPosition + new Vector3(0, 1.1f, 0), levelBlockBehavior, () =>
                    {
                        hitParticle.gameObject.SetActive(true);
                        hitParticle.transform.position = centerPosition + new Vector3(0, 1.1f, 0);

                        hitParticle.Play();

                        levelBlockBehavior.gameObject.SetActive(false);

                        levelBlockBehavior.DisableEffects();

                        LevelController.GameplayTimer.Resume(TIMER_UNIQUE_NAME);

                        if (levelRepresentation.AllBlocksCleared)
                        {
                            GameController.GameComplete();
                        }

                        AudioController.PlaySound(AudioController.AudioClips.blockDestroy);

                        if (hammerHitSound != null)
                        {
                            AudioController.PlaySound(hammerHitSound);
                        }

                        IsBusy = false;
                    });

                    return true;
                }
            }

            return false;
        }

        public override bool IsSelectable() => true;
    }
}

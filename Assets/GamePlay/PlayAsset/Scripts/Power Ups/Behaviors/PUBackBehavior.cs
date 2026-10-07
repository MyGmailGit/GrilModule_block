using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class PUBackBehavior : PUBehavior
    {
        private const string TIMER_UNIQUE_NAME = "back";

        private bool isPlaying;

        public override void Init()
        {
            isPlaying = false;
        }
        public override bool Activate()
        {
            return LevelController.PullBackOneStep();
        }

        /*
                public override bool ApplyToElement(IClickableObject clickableObject, Vector3 clickPosition)
                {

                    return LevelController.PullBackOneStep();

                    // if (clickableObject is LevelBlockBehavior levelBlockBehavior)
                    // {
                    //     if (!levelBlockBehavior.IsCollected)
                    //     {
                    //         isPlaying = true;
                    //         IsBusy = true;

                    //         // LevelController.GameplayTimer.Pause(TIMER_UNIQUE_NAME);

                    //         LevelRepresentation levelRepresentation = LevelController.LevelRepresentation;

                    //         BlockColor selectedColor = levelBlockBehavior.ColorData.Type;

                    //         List<LevelBlockBehavior> selectedBlocks = new List<LevelBlockBehavior>();
                    //         selectedBlocks.Add(levelBlockBehavior);

                    //         List<LevelBlockBehavior> activeBlocks = levelRepresentation.ActiveBlocks;
                    //         foreach (LevelBlockBehavior block in activeBlocks)
                    //         {
                    //             if (block.ColorData.Type == selectedColor && !block.IsCollected && block != levelBlockBehavior)
                    //             {
                    //                 selectedBlocks.Add(block);
                    //             }
                    //         }

                    //         Bounds levelBounds = levelRepresentation.LevelBounds;
                    //         Vector3 magnetPosition = new Vector3(levelBounds.center.x, 1.5f, levelBounds.min.z - 1.0f);

                    //         magnetVisuals.PlayAnimation(magnetPosition);

                    //         Vector3 finalPosition = magnetPosition + new Vector3(0, 1f, 1.2f);
                    //         Vector3 magnetPointPosition = finalPosition + new Vector3(0, 0, 1);

                    //         int completedElements = 0;
                    //         float delay = startDelay;
                    //         foreach (LevelBlockBehavior block in selectedBlocks)
                    //         {
                    //             block.OnBlockCollected();
                    //             block.DisableEffects();

                    //             // LevelController.OnBlockDestructed(block);

                    //             float distance = Vector3.Distance(block.transform.position, finalPosition);
                    //             float duration = distance / moveSpeed;

                    //             block.transform.DOScale(0, duration, delay).SetEase(scaleEasingType);
                    //             block.transform.DOBezierMove(finalPosition, magnetPointPosition, duration, delay).SetEase(moveEasingType).OnComplete(() =>
                    //             {
                    //                 block.gameObject.SetActive(false);

                    //                 completedElements++;
                    //                 if (completedElements >= selectedBlocks.Count)
                    //                 {
                    //                     magnetVisuals.StopAnimation();

                    //                     isPlaying = false;
                    //                     IsBusy = false;

                    //                     // LevelController.GameplayTimer.Resume(TIMER_UNIQUE_NAME);

                    //                     if (levelRepresentation.AllBlocksCleared)
                    //                     {
                    //                         GameController.GameComplete();
                    //                     }
                    //                 }
                    //             });

                    //             if (magnetEffectSound != null)
                    //             {
                    //                 AudioController.PlaySound(magnetEffectSound);
                    //             }

                    //             delay += 0.05f;
                    //         }

                    //         return true;
                    //     }
                    // }

                    return true;
                }
                */

        public override bool IsSelectable() => false;
    }
}

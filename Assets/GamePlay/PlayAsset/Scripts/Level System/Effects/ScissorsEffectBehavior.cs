// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class ScissorsEffectBehavior : BlockEffectBehavior
//     {
//         [SerializeField] ScissorsMovementBehavior scissorsMovementBehavior;

//         [Space]
//         [SerializeField] float offsetY = 0.1f;

//         private BlockColor scissorsColor;

//         private bool isCollected;

//         public override int EffectSortingOrder => 2;

//         public override void OnCreated(LevelBlockBehavior blockBehavior)
//         {
//             Bounds bounds = blockBehavior.Figure.GetHorizontalCenterBounds();
//             transform.position = blockBehavior.transform.position + bounds.center + new Vector3(0, offsetY * orderID, 0);

//             scissorsColor = effectData.scissorsColor;

//             // BlockColorData colorData = LevelController.GetBlockColorData(scissorsColor);
//             // scissorsMovementBehavior.Init(colorData);
//         }

//         private void CollectScissors()
//         {
//             if (isCollected) return;

//             RopeBehavior ropeBehavior = RopesManager.GetRopeBehavior(scissorsColor);
//             if (ropeBehavior != null)
//             {
//                 scissorsMovementBehavior.StartMovement(ropeBehavior);
//             }

//             DisableEffect();
//         }

//         public override void OnBlockCollected()
//         {
//             CollectScissors();
//         }

//         public override bool CanBeReapplied()
//         {
//             return false;
//         }
//     }
// }

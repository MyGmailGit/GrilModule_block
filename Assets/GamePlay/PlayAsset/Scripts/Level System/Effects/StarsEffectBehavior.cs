// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class StarsEffectBehavior : BlockEffectBehavior
//     {
//         [SerializeField] GameObject visualsPrefab;

//         public override void OnCreated(LevelBlockBehavior blockBehavior)
//         {
//             LevelFigure figure = blockBehavior.Figure;
//             Vector2Int size = figure.Size;

//             for (int x = 0; x < size.x; x++)
//             {
//                 for (int y = 0; y < size.y; y++)
//                 {
//                     int index = x + y * size.x;

//                     if (figure.Points[index].IsFilled)
//                     {
//                         GameObject visualObject = Instantiate(visualsPrefab, transform);
//                         visualObject.transform.position = blockBehavior.transform.position + new Vector3(x, GameConsts.BLOCK_HEIGHT, y);
//                     }
//                 }
//             }
//         }

//         public override bool CanGoThroughGate(GateBehavior gateBehavior)
//         {
//             if (gateBehavior.Data.LevelElementData.BlockColor == linkedBlock.GetActiveBlockColor())
//             {
//                 if (gateBehavior.HasEffect(GateEffectType.Stars))
//                 {
//                     return true;
//                 }
//             }

//             return false;
//         }
//     }
// }

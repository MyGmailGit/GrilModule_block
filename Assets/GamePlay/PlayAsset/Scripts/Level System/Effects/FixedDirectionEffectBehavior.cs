// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class FixedDirectionEffectBehavior : BlockEffectBehavior
//     {
//         [SerializeField] Transform arrowTransform;
//         [SerializeField] ModelSlicer arrowSlicer;

//         [Space]
//         [SerializeField] float offsetY = 0.1f;

//         public override int EffectSortingOrder => 1;

//         public override void OnCreated(LevelBlockBehavior blockBehavior)
//         {
//             Bounds bounds = effectData.horizontalDirection ? blockBehavior.Figure.GetHorizontalCenterBounds() : blockBehavior.Figure.GetVerticalCenterBounds();

//             arrowSlicer.Init();
//             arrowSlicer.ApplyScaling(new Vector3(1, 1, effectData.horizontalDirection ? bounds.size.x : bounds.size.z));

//             arrowTransform.transform.localRotation = Quaternion.Euler(0, effectData.horizontalDirection ? 90 : 0, 0);
//             arrowTransform.transform.position = arrowTransform.transform.position + bounds.center + new Vector3(0, offsetY * orderID, 0);
//         }

//         public override void OnBlockCollected()
//         {
//             DisableEffect();
//         }

//         public override void OverrideMovementMatrix(ref int[,] movementMatrix)
//         {
//             LevelFigure figure = linkedBlock.Figure;
//             Vector2Int figureSize = figure.Size;
//             Vector2Int position = linkedBlock.MatrixPosition;

//             int rows = movementMatrix.GetLength(0);
//             int cols = movementMatrix.GetLength(1);

//             if (effectData.horizontalDirection)
//             {
//                 int minY = Mathf.Max(0, position.y);
//                 int maxY = Mathf.Min(cols - 1, position.y + figureSize.y - 1);

//                 for (int y = 0; y < cols; y++)
//                 {
//                     for (int x = 0; x < rows; x++)
//                     {
//                         if (movementMatrix[x, y] == MovementMatrix.STATUS_FREE)
//                         {
//                             if (y < minY || y > maxY)
//                             {
//                                 movementMatrix[x, y] = MovementMatrix.STATUS_BLOCKED;
//                             }
//                         }
//                     }
//                 }
//             }
//             else
//             {
//                 int minX = Mathf.Max(0, position.x);
//                 int maxX = Mathf.Min(rows - 1, position.x + figureSize.x - 1);

//                 for (int y = 0; y < cols; y++)
//                 {
//                     for (int x = 0; x < rows; x++)
//                     {
//                         if (movementMatrix[x, y] == MovementMatrix.STATUS_FREE)
//                         {
//                             if (x < minX || x > maxX)
//                             {
//                                 movementMatrix[x, y] = MovementMatrix.STATUS_BLOCKED;
//                             }
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }

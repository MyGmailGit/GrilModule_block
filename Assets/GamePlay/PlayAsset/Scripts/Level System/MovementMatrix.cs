// using UnityEngine;
// using System.Collections.Generic;

// namespace Watermelon
// {
//     public class MovementMatrix
//     {
//         public const int STATUS_BLOCKED = -1;
//         public const int STATUS_FREE = 0;
//         public const int STATUS_OCCUPIED = 1;

//         private const float BLOCK_HALF_SIZE = 0.5f;
//         private const float BLOCK_SIZE_PERCENT = 0.97f;

//         private readonly Vector3[] OFFSETS = new Vector3[]
//         {
//             new Vector3(BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT, 0, BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT),
//             new Vector3(-BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT, 0, BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT),
//             new Vector3(-BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT, 0, -BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT),
//             new Vector3(BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT, 0, -BLOCK_HALF_SIZE * BLOCK_SIZE_PERCENT)
//         };

//         public int[,] Matrix { get; private set; }

//         private int width;
//         public int Width => width;

//         private int height;
//         public int Height => height;

//         public MovementMatrix(int[,] matrix)
//         {
//             Matrix = matrix;

//             width = matrix.GetLength(0);
//             height = matrix.GetLength(1);
//         }

//         public MovementMatrix(LevelRepresentation levelRepresentation, LevelBlockBehavior levelBlockBehavior)
//         {
//             Vector2Int size = levelRepresentation.Size;
//             LevelElementData[,] levelMatrix = levelRepresentation.LevelMatrix;

//             int[,] movementMatrix = new int[size.x, size.y];
//             for (int x = 0; x < size.x; x++)
//             {
//                 for (int y = 0; y < size.y; y++)
//                 {
//                     if (levelMatrix[x, y].Type == ElementType.Empty || levelMatrix[x, y].Type == ElementType.Gate || levelMatrix[x, y].Type == ElementType.Obstacle || levelMatrix[x, y].Type == ElementType.Border)
//                     {
//                         movementMatrix[x, y] = MovementMatrix.STATUS_BLOCKED; // Block movement on these elements
//                     }
//                     else
//                     {
//                         movementMatrix[x, y] = MovementMatrix.STATUS_FREE; // Allow movement on inner tiles
//                     }
//                 }
//             }

//             // Calculate interactables
//             List<InteractableObjectBehavior> interactableObjects = levelRepresentation.InteractableObjects;
//             for(int i = 0; i < interactableObjects.Count; i++)
//             {
//                 InteractableObjectBehavior interactableObject = interactableObjects[i];
//                 Vector2Int position = interactableObject.Position;

//                 if(interactableObject.IsMovable(levelBlockBehavior))
//                 {
//                     movementMatrix[position.x, position.y] = MovementMatrix.STATUS_FREE;
//                 }
//                 else
//                 {
//                     movementMatrix[position.x, position.y] = MovementMatrix.STATUS_OCCUPIED;
//                 }
//             }

//             // Calculate blocks
//             List<LevelBlockBehavior> blocks = levelRepresentation.ActiveBlocks;
//             for (int i = 0; i < blocks.Count; i++)
//             {
//                 LevelFigure figure = blocks[i].Figure;
//                 Vector2Int position = blocks[i].MatrixPosition;

//                 Vector2Int figureSize = figure.Size;
//                 Vector2Int pivotPoint = figure.PivotPoint;

//                 for (int fX = 0; fX < figureSize.x; fX++)
//                 {
//                     for (int fY = 0; fY < figureSize.y; fY++)
//                     {
//                         int index = fX + fY * figureSize.x;
//                         if (figure.Points[index].IsFilled)
//                         {
//                             int x = position.x + fX;
//                             int y = position.y + fY;

//                             if (x >= 0 && x < size.x && y >= 0 && y < size.y)
//                             {
//                                 movementMatrix[x, y] = MovementMatrix.STATUS_OCCUPIED; // Mark the block position as occupied
//                             }
//                         }
//                     }
//                 }
//             }

//             if(levelBlockBehavior.HasActiveEffect())
//             {
//                 List<BlockEffectBehavior> effects = levelBlockBehavior.Effects;
//                 foreach(BlockEffectBehavior effect in effects)
//                 {
//                     effect.OverrideMovementMatrix(ref movementMatrix);
//                 }
//             }

//             Matrix = movementMatrix;

//             width = size.x;
//             height = size.y;
//         }

//         public bool IsFree(Vector2Int position)
//         {
//             if (position.x < 0 || position.x >= width || position.y < 0 || position.y >= height)
//                 return false;

//             return Matrix[position.x, position.y] == STATUS_FREE;
//         }

//         public bool IsMovementAllowed(LevelFigure figure, Vector3 position, List<BlockMovementManager.LinkedObjectData> linkedObjects)
//         {
//             foreach(BlockMovementManager.LinkedObjectData linkedObject in linkedObjects)
//             {
//                 if(!IsMovementAllowed(linkedObject.Block.Figure, position + linkedObject.Offset))
//                 {
//                     return false;
//                 }
//             }

//             if(!IsMovementAllowed(figure, position))
//             {
//                 return false;
//             }

//             return true;
//         }

//         public bool IsMovementAllowed(LevelFigure figure, Vector3 position)
//         {
//             Vector2Int size = figure.Size;

//             for (int fX = 0; fX < size.x; fX++)
//             {
//                 for (int fY = 0; fY < size.y; fY++)
//                 {
//                     int index = fX + fY * size.x;
//                     if (figure.Points[index].IsFilled)
//                     {
//                         for (int o = 0; o < 4; o++)
//                         {
//                             Vector3 blockPosition = position + OFFSETS[o];

//                             Vector2Int matrixPosition = WorldToGridPosition(blockPosition);

//                             int x = matrixPosition.x + fX;
//                             int y = matrixPosition.y + fY;

//                             if (x < 0 || x >= width || y < 0 || y >= height || Matrix[x, y] != STATUS_FREE)
//                             {
//                                 return false;
//                             }
//                         }

//                     }
//                 }
//             }

//             return true;
//         }

//         public void OverrideFigureStatus(LevelFigure figure, Vector2Int position, int status)
//         {
//             Vector2Int size = figure.Size;

//             for (int fX = 0; fX < size.x; fX++)
//             {
//                 for (int fY = 0; fY < size.y; fY++)
//                 {
//                     int index = fX + fY * size.x;
//                     if (figure.Points[index].IsFilled)
//                     {
//                         int x = position.x + fX;
//                         int y = position.y + fY;
//                         if (x >= 0 && x < width && y >= 0 && y < height)
//                         {
//                             Matrix[x, y] = status;
//                         }
//                     }
//                 }
//             }
//         }

//         public void WorldToGridPosition(Vector3 worldPosition, ref Vector2Int gridPosition)
//         {
//             // Calculate the grid position
//             int x = Mathf.RoundToInt(worldPosition.x);
//             int z = Mathf.RoundToInt(worldPosition.z);

//             // Ensure the position is within the bounds of the grid
//             if (x < 0 || x >= width || z < 0 || z >= height)
//             {
//                 gridPosition.x = -1;
//                 gridPosition.y = -1;

//                 return;
//             }

//             gridPosition.x = x;
//             gridPosition.y = z;
//         }

//         public Vector2Int WorldToGridPosition(Vector3 worldPosition)
//         {
//             // Calculate the grid position
//             int x = Mathf.RoundToInt(worldPosition.x);
//             int z = Mathf.RoundToInt(worldPosition.z);

//             // Ensure the position is within the bounds of the grid
//             if (x < 0 || x >= width || z < 0 || z >= height)
//             {
//                 return new Vector2Int(-1, -1);
//             }

//             return new Vector2Int(x, z);
//         }

//         public void Print()
//         {
//             // Debug log to show preview of the matrix
//             string matrixPreview = "Movement Matrix:\n";
//             for (int y = Matrix.GetLength(1) - 1; y >= 0; y--)
//             {
//                 for (int x = 0; x < Matrix.GetLength(0); x++)
//                 {
//                     matrixPreview += Matrix[x, y].ToString().PadLeft(2) + " ";
//                 }
//                 matrixPreview += "\n";
//             }

//             Debug.Log(matrixPreview);
//         }

//         public void DebugMatrix()
//         {
//             for (int y = Matrix.GetLength(1) - 1; y >= 0; y--)
//             {
//                 for (int x = 0; x < Matrix.GetLength(0); x++)
//                 {
//                     Debug.DrawLine(new Vector3(x, 0f, y), new Vector3(x, 2f, y), Matrix[x, y] == 0 ? Color.green : Color.red);
//                 }
//             }
//         }
//     }
// }

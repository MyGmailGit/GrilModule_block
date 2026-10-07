// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// namespace Watermelon
// {
//     public class EnvironmentSpawner
//     {
//         private readonly Vector3 RIGHT_BOTTOM_OFFSET = new Vector3(0.25f, 0f, -0.25f);
//         private readonly Vector3 RIGHT_TOP_OFFSET = new Vector3(0.25f, 0f, 0.25f);
//         private readonly Vector3 LEFT_BOTTOM_OFFSET = new Vector3(-0.25f, 0f, -0.25f);
//         private readonly Vector3 LEFT_TOP_OFFSET = new Vector3(-0.25f, 0f, 0.25f);

//         private readonly Vector3 RIGHT_OFFSET = new Vector3(0.25f, 0f, 0f);
//         private readonly Vector3 LEFT_OFFSET = new Vector3(-0.25f, 0f, 0f);
//         private readonly Vector3 TOP_OFFSET = new Vector3(0f, 0f, 0.25f);
//         private readonly Vector3 BOTTOM_OFFSET = new Vector3(0f, 0f, -0.25f);

//         private LevelRepresentation levelRepresentation;

//         private EnvironmentData data;
//         private Transform parentTransform;
//         private LevelData levelData;

//         private Vector2Int size;
//         private BorderData[,] bordersMatrix;

//         private float minYPosition = int.MaxValue;
//         private float maxYPosition = int.MinValue;

//         private float minXPosition = int.MaxValue;
//         private float maxXPosition = int.MinValue;

//         private List<GateBehavior> gates;
//         public List<GateBehavior> Gates => gates;

//         public EnvironmentSpawner(LevelRepresentation levelRepresentation)
//         {
//             this.levelRepresentation = levelRepresentation;

//             data = levelRepresentation.EnvironmentData;
//             parentTransform = levelRepresentation.LevelTransform;
//             levelData = levelRepresentation.LevelData;
//             size = levelData.Size;

//             gates = new List<GateBehavior>();

//             LevelElementData[] levelElements = levelData.LevelElements;

//             bordersMatrix = new BorderData[size.x, size.y];
//             for (int i = 0; i < levelElements.Length; i++)
//             {
//                 BorderData borderData = new BorderData(levelElements[i]);

//                 bordersMatrix[borderData.Position.x, borderData.Position.y] = borderData;
//             }
//         }

//         public void SpawnBorders(LevelElementData[] levelElements)
//         {
//             for (int x = 0; x < bordersMatrix.GetLength(0); x++)
//             {
//                 for (int y = 0; y < bordersMatrix.GetLength(1); y++)
//                 {
//                     BorderData element = bordersMatrix[x, y];

//                     if (element.Type == ElementType.Border)
//                     {
//                         if (IsCorner(element))
//                         {
//                             element.SetCornerState(true);
//                             element.InitOffset(CalculateCornerOffset(element));
//                             element.InitRotation(CalculateCornerRotaion(element));
//                         }
//                         else
//                         {
//                             element.SetCornerState(false);
//                             element.InitOffset(CalculateBorderOffset(element));
//                             element.InitRotation(CalculateBorderRotation(element));

//                         }
//                     }
//                     else if (element.Type == ElementType.Gate)
//                     {
//                         element.SetCornerState(false);
//                         element.InitOffset(CalculateBorderOffset(element));
//                         element.InitRotation(CalculateBorderRotation(element));
//                     }
//                 }
//             }

//             for (int x = 0; x < bordersMatrix.GetLength(0); x++)
//             {
//                 for (int y = 0; y < bordersMatrix.GetLength(1); y++)
//                 {
//                     BorderData element = bordersMatrix[x, y];

//                     if (element.Type == ElementType.Border)
//                     {
//                         if (!element.IsCorner)
//                         {
//                             UnifyNeigbouringElements(element);
//                         }
//                     }
//                     else if (element.Type == ElementType.Gate)
//                     {
//                         UnifyNeigbouringElements(element);
//                     }
//                 }
//             }

//             for (int x = 0; x < bordersMatrix.GetLength(0); x++)
//             {
//                 for (int y = 0; y < bordersMatrix.GetLength(1); y++)
//                 {
//                     BorderData element = bordersMatrix[x, y];
//                     Vector2 position = element.Position;

//                     if (element.Type == ElementType.Border)
//                     {
//                         if (element.IsCorner)
//                         {
//                             SpawnCorner(new Vector3(position.x, 0f, position.y) + element.Offset, element.Rotation, element.IsConvex);

//                             if (!element.IsConvex)
//                             {
//                                 SpawnExtraBordersIfNeeded(element);
//                             }
//                         }
//                         else
//                         {
//                             if (element.ShoulBeSpawned)
//                             {
//                                 SpawnBorderElement(element.SpawnPosition + element.Offset, element.Rotation, element.SpawnSize);
//                             }
//                         }
//                     }
//                     else if (element.Type == ElementType.InnerTile || element.Type == ElementType.Block || element.Type == ElementType.InteractableObject)
//                     {
//                         SpawnInnerTile(new Vector3(position.x, 0f, position.y));
//                     }
//                     else if (element.Type == ElementType.Gate)
//                     {
//                         if (element.ShoulBeSpawned)
//                         {
//                             SpawnGate(element, element.SpawnPosition + element.Offset, element.Rotation, element.SpawnSize);
//                         }
//                     }
//                     else if (element.Type == ElementType.Obstacle)
//                     {
//                         SpawnInnerObstacle(new Vector3(position.x, 0f, position.y));
//                         SpawnInnerTile(new Vector3(position.x, 0f, position.y));
//                     }
//                 }
//             }

//             SortAndLinkGatesClockwise();
//         }

//         private void SortAndLinkGatesClockwise()
//         {
//             if (gates == null || gates.Count == 0)
//                 return;

//             float centerX = (bordersMatrix.GetLength(0) - 1) * 0.5f;
//             float centerY = (bordersMatrix.GetLength(1) - 1) * 0.5f;
//             Vector2 center = new Vector2(centerX, centerY);

//             // 1) Sort: top (12 o’clock) → clockwise. Tie‑break by radius (inner first).
//             gates.Sort((a, b) =>
//             {
//                 var posA = a.Data.Position;
//                 var posB = b.Data.Position;

//                 float angleA = AngleTopClockwise(posA, center);
//                 float angleB = AngleTopClockwise(posB, center);

//                 int cmp = angleA.CompareTo(angleB);
//                 if (cmp != 0) return cmp;

//                 float rA = (posA - center).sqrMagnitude;
//                 float rB = (posB - center).sqrMagnitude;

//                 cmp = rA.CompareTo(rB);
//                 if (cmp != 0) return cmp;

//                 // Final deterministic tie-breaker (rare, identical pos):
//                 return a.GetInstanceID().CompareTo(b.GetInstanceID());
//             });

//             // 2) Link circularly: prev = i-1, next = i+1 (mod N).
//             int n = gates.Count;

//             if (n == 1)
//                 return;

//             for (int i = 0; i < n; i++)
//             {
//                 GateBehavior prev = gates[(i - 1 + n) % n];
//                 GateBehavior next = gates[(i + 1) % n];

//                 gates[i].LinkGatesInClockwiseOrder(next, prev);
//             }
//         }

//         private float AngleTopClockwise(Vector2 p, Vector2 center)
//         {
//             // Mathf.Atan2 returns angle with 0 at +X, CCW positive.
//             float raw = Mathf.Atan2(p.y - center.y, p.x - center.x); // [-π, π]
//                                                                      // Shift so 0 is at top (+Y) and increase clockwise:
//             float shifted = (Mathf.PI * 0.5f - raw);
//             // Normalize to [0, 2π):
//             float twoPi = Mathf.PI * 2f;
//             shifted %= twoPi;

//             if (shifted < 0f) shifted += twoPi;

//             return shifted;
//         }

//         private void UpdateBounds(Vector3 position)
//         {
//             if (position.x < minXPosition)
//             {
//                 minXPosition = position.x;
//             }

//             if (position.x > maxXPosition)
//             {
//                 maxXPosition = position.x;
//             }

//             if (position.z < minYPosition)
//             {
//                 minYPosition = position.z;
//             }

//             if (position.z > maxYPosition)
//             {
//                 maxYPosition = position.z;
//             }
//         }

//         private void SpawnCorner(Vector3 position, Vector3 rotation, bool isConvex)
//         {
//             GameObject corner = GameObject.Instantiate(data.CornerPrefab, position, Quaternion.Euler(rotation), parentTransform);

//             UpdateBounds(position);

//             // spawning inner ground
//             if (isConvex)
//             {
//                 GameObject innerGround = GameObject.Instantiate(data.CornerInnerGroundConvex, position, Quaternion.Euler(rotation), parentTransform);
//             }
//             else
//             {
//                 GameObject innerGround = GameObject.Instantiate(data.CornerInnerGroundConcave, position, Quaternion.Euler(rotation), parentTransform);
//             }
//         }

//         private void SpawnBorderElement(Vector3 position, Vector3 rotation, float size)
//         {
//             GameObject border = GameObject.Instantiate(data.BorderPrefab, position, Quaternion.Euler(rotation), parentTransform);

//             ModelSlicer modelSlicer = border.GetComponent<ModelSlicer>();
//             if (modelSlicer != null)
//             {
//                 modelSlicer.Init();
//                 modelSlicer.ApplyScaling(Vector3.one.SetZ(size));
//             }
//             else
//             {
//                 border.transform.localScale = border.transform.localScale.SetZ(size);
//             }

//             // spawn Border Inner Ground element
//             GameObject innerGround = GameObject.Instantiate(data.BorderInnerGround, position + GetBorderInnerTileOffset(position, rotation), Quaternion.Euler(rotation), parentTransform);
//             innerGround.transform.localScale = innerGround.transform.localScale.SetZ(size);

//             UpdateBounds(position);
//         }

//         private Vector3 GetBorderInnerTileOffset(Vector3 borderPosition, Vector3 rotation)
//         {
//             // vertical border
//             if (rotation.y == 0)
//             {
//                 float offsetFromCenter = borderPosition.x % 1;

//                 if (offsetFromCenter == 0.25f)
//                 {
//                     return Vector3.right * 0.125f;
//                 }
//                 else if (offsetFromCenter == 0.75f)
//                 {
//                     return Vector3.right * -0.125f;
//                 }
//             }
//             // horizontal border
//             else
//             {
//                 float offsetFromCenter = borderPosition.z % 1;

//                 if (offsetFromCenter == 0.25f)
//                 {
//                     return Vector3.forward * 0.125f;
//                 }
//                 else if (offsetFromCenter == 0.75f)
//                 {
//                     return Vector3.forward * -0.125f;
//                 }

//             }

//             return Vector3.zero;
//         }

//         private void SpawnExtraBordersIfNeeded(BorderData element)
//         {
//             if (!element.IsCorner)
//                 return;

//             if (element.IsConvex)
//                 return;

//             Vector2 matrixPos = element.LevelElementData.Position;

//             BorderData leftNeighbour = GetMatrixElement(matrixPos.AddToX(-1), true);

//             if (leftNeighbour != null && (leftNeighbour.IsCorner || leftNeighbour.Type == ElementType.Gate))
//             {
//                 SpawnBorderElement(new Vector3(element.Position.x, 0f, element.Position.y) + element.Offset + LEFT_OFFSET * 2f, new Vector3(0f, 90f, 0f), 0.5f);
//             }

//             BorderData rightNeighbour = GetMatrixElement(matrixPos.AddToX(1), true);

//             if (rightNeighbour != null && (rightNeighbour.IsCorner || rightNeighbour.Type == ElementType.Gate))
//             {
//                 SpawnBorderElement(new Vector3(element.Position.x, 0f, element.Position.y) + element.Offset + RIGHT_OFFSET * 2f, new Vector3(0f, 90f, 0f), 0.5f);
//             }

//             BorderData bottomNeighbour = GetMatrixElement(matrixPos.AddToY(-1), true);

//             if (bottomNeighbour != null && (bottomNeighbour.IsCorner || bottomNeighbour.Type == ElementType.Gate))
//             {
//                 SpawnBorderElement(new Vector3(element.Position.x, 0f, element.Position.y) + element.Offset + BOTTOM_OFFSET * 2f, Vector3.zero, 0.5f);
//             }

//             BorderData topNeighbour = GetMatrixElement(matrixPos.AddToY(1), true);

//             if (topNeighbour != null && (topNeighbour.IsCorner || topNeighbour.Type == ElementType.Gate))
//             {
//                 SpawnBorderElement(new Vector3(element.Position.x, 0f, element.Position.y) + element.Offset + TOP_OFFSET * 2f, Vector3.zero, 0.5f);
//             }
//         }

//         private void SpawnInnerTile(Vector3 position)
//         {
//             GameObject innerTile = GameObject.Instantiate(data.InnerTilePrefab, position, Quaternion.identity, parentTransform);
//         }

//         private void SpawnGate(BorderData borderData, Vector3 position, Vector3 rotation, float size)
//         {
//             GameObject gate = GameObject.Instantiate(data.GatePrefab, position, Quaternion.Euler(rotation), parentTransform);

//             ModelSlicer modelSlicer = gate.GetComponent<ModelSlicer>();
//             if (modelSlicer != null)
//             {
//                 modelSlicer.Init();
//                 modelSlicer.ApplyScaling(Vector3.one.SetZ(size));
//             }
//             else
//             {
//                 gate.transform.localScale = gate.transform.localScale.SetZ(size);
//             }

//             GateBehavior gateBehavior = gate.GetComponent<GateBehavior>();
//             gateBehavior.Init(borderData);

//             LevelElementData elementData = borderData.LevelElementData;

//             GateEffectData[] gateEffects = elementData.GateEffects;
//             if(!gateEffects.IsNullOrEmpty())
//             {
//                 foreach(GateEffectData effect in gateEffects)
//                 {
//                     GateEffectType effectType = effect.Type;
//                     if (effectType != GateEffectType.None)
//                     {
//                         LevelGateEffectData effectData = LevelController.GetEffectData(effectType);

//                         effectData.Behavior.ApplyEffect(gateBehavior, effectType, effect);
//                     }
//                 }
//             }

//             List<BorderData> unifiedElements = borderData.UnifiedElements;
//             if (!unifiedElements.IsNullOrEmpty())
//             {
//                 foreach (BorderData unifiedElement in unifiedElements)
//                 {
//                     Vector2Int elementPosition = unifiedElement.Position;

//                     bordersMatrix[elementPosition.x, elementPosition.y].SetGateBehavior(gateBehavior);
//                 }
//             }

//             gates.Add(gateBehavior);

//             // spawn Border Inner Ground element
//             GameObject innerGround = GameObject.Instantiate(data.BorderInnerGround, position + GetBorderInnerTileOffset(position, rotation), Quaternion.Euler(rotation), parentTransform);
//             innerGround.transform.localScale = innerGround.transform.localScale.SetZ(size);
//         }

//         private void SpawnInnerObstacle(Vector3 position)
//         {
//             GameObject innerObstacle = GameObject.Instantiate(data.InnerObstaclePrefab, position, Quaternion.identity, parentTransform);
//         }

//         private void UnifyNeigbouringElements(BorderData element)
//         {
//             if (!element.IsUnified && element.IsHorizonal)
//             {
//                 element.InitUnifiedState(true);
//                 element.InitShouldBeSpawned(true);

//                 if (element.Type == ElementType.Gate)
//                     element.SetGateDirection(GetGateDirection(element));

//                 List<BorderData> elementsToUnify = new List<BorderData>();
//                 elementsToUnify.Add(element);

//                 //adding right neighbours
//                 elementsToUnify.AddRange(CollectNeighboursThatSuitForUnification(element, Vector2.right));

//                 //adding left neighbours
//                 elementsToUnify.AddRange(CollectNeighboursThatSuitForUnification(element, Vector2.left));

//                 elementsToUnify.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));

//                 float extraOffsetCausedByInnerCorners = 0;
//                 float extraSizeCausedByInnerCorners = 0;

//                 // checking right edge neighbour (only for borders)
//                 BorderData edgeElement = GetMatrixElement(elementsToUnify.Last().Position.AddToX(1), true);

//                 if (edgeElement != null && element.Type != ElementType.Gate && edgeElement.Type == ElementType.Border && edgeElement.IsCorner && !edgeElement.IsConvex)
//                 {
//                     extraOffsetCausedByInnerCorners += 0.25f;
//                     extraSizeCausedByInnerCorners += 0.5f;
//                 }

//                 // checking left edge neighbour (only for borders)
//                 edgeElement = GetMatrixElement(elementsToUnify.First().Position.AddToX(-1), true);

//                 if (edgeElement != null && element.Type != ElementType.Gate && edgeElement.Type == ElementType.Border && edgeElement.IsCorner && !edgeElement.IsConvex)
//                 {
//                     extraOffsetCausedByInnerCorners -= 0.25f;
//                     extraSizeCausedByInnerCorners += 0.5f;
//                 }

//                 if (elementsToUnify.Count <= 1)
//                 {
//                     element.InitSpawnPosition(new Vector3(element.Position.x + extraOffsetCausedByInnerCorners, 0f, element.Position.y));
//                     element.InitSpawnSize(1f + extraSizeCausedByInnerCorners);
//                 }
//                 else
//                 {
//                     Vector3 spawnPosition = new Vector3(elementsToUnify.First().Position.x + (elementsToUnify.Last().Position.x - elementsToUnify.First().Position.x) * 0.5f, 0f, element.Position.y);
//                     element.InitSpawnPosition(spawnPosition.AddToX(extraOffsetCausedByInnerCorners));

//                     element.InitSpawnSize(elementsToUnify.Count + extraSizeCausedByInnerCorners);
//                 }

//                 element.SetUnifiedElements(elementsToUnify);
//             }
//             else if (!element.IsUnified && !element.IsHorizonal)
//             {
//                 element.InitUnifiedState(true);
//                 element.InitShouldBeSpawned(true);

//                 if (element.Type == ElementType.Gate)
//                     element.SetGateDirection(GetGateDirection(element));

//                 List<BorderData> elementsToUnify = new List<BorderData>();
//                 elementsToUnify.Add(element);

//                 //adding top neighbours
//                 elementsToUnify.AddRange(CollectNeighboursThatSuitForUnification(element, Vector2.up));

//                 //adding bottom neighbours
//                 elementsToUnify.AddRange(CollectNeighboursThatSuitForUnification(element, Vector2.down));

//                 elementsToUnify.Sort((a, b) => a.Position.y.CompareTo(b.Position.y));

//                 float extraOffsetCausedByInnerCorners = 0;
//                 float extraSizeCausedByInnerCorners = 0;

//                 // checking top edge neighbour (only for borders)
//                 BorderData edgeElement = GetMatrixElement(elementsToUnify.Last().Position.AddToY(1), true);

//                 if (edgeElement != null && element.Type != ElementType.Gate && edgeElement.Type == ElementType.Border && edgeElement.IsCorner && !edgeElement.IsConvex)
//                 {
//                     extraOffsetCausedByInnerCorners += 0.25f;
//                     extraSizeCausedByInnerCorners += 0.5f;
//                 }

//                 // checking bottom edge neighbour (only for borders)
//                 edgeElement = GetMatrixElement(elementsToUnify.First().Position.AddToY(-1), true);

//                 if (edgeElement != null && element.Type != ElementType.Gate && edgeElement.Type == ElementType.Border && edgeElement.IsCorner && !edgeElement.IsConvex)
//                 {
//                     extraOffsetCausedByInnerCorners -= 0.25f;
//                     extraSizeCausedByInnerCorners += 0.5f;
//                 }

//                 if (elementsToUnify.Count <= 1)
//                 {
//                     element.InitSpawnPosition(new Vector3(element.Position.x, 0f, element.Position.y + extraOffsetCausedByInnerCorners));
//                     element.InitSpawnSize(1f + extraSizeCausedByInnerCorners);
//                 }
//                 else
//                 {
//                     Vector3 spawnPosition = new Vector3(element.Position.x, 0f, elementsToUnify.First().Position.y + (elementsToUnify.Last().Position.y - elementsToUnify.First().Position.y) * 0.5f);
//                     element.InitSpawnPosition(spawnPosition.AddToZ(extraOffsetCausedByInnerCorners));

//                     element.InitSpawnSize(elementsToUnify.Count + extraSizeCausedByInnerCorners);
//                 }

//                 element.SetUnifiedElements(elementsToUnify);
//             }
//         }

//         private GateDirection.Type GetGateDirection(BorderData borderData)
//         {
//             BorderData neighbour;

//             // checking right neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToX(1), true);

//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 return GateDirection.Type.Right;
//             }

//             // checking left neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToX(-1), true);
//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 return GateDirection.Type.Left;
//             }

//             // checking top neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToY(1), true);
//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 return GateDirection.Type.Top;
//             }

//             // checking bottom neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToY(-1), true);
//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 return GateDirection.Type.Bottom;
//             }

//             return GateDirection.Type.None;
//         }

//         private List<BorderData> CollectNeighboursThatSuitForUnification(BorderData originalElement, Vector2 neighboursDirection)
//         {
//             bool shouldContinue = false;
//             int counter = 0;

//             List<BorderData> result = new List<BorderData>();

//             do
//             {
//                 counter++;

//                 BorderData neighbour = GetMatrixElement(originalElement.Position + neighboursDirection * counter, true);

//                 if (DoesElementSuitsForUnification(originalElement, neighbour))
//                 {
//                     result.Add(neighbour);

//                     neighbour.InitUnifiedState(true);
//                     neighbour.InitShouldBeSpawned(false);

//                     shouldContinue = true;
//                 }
//                 else
//                 {
//                     shouldContinue = false;
//                 }
//             }
//             while (shouldContinue);

//             return result;
//         }

//         private bool DoesElementSuitsForUnification(BorderData originalElement, BorderData elementToCheck)
//         {
//             bool result = elementToCheck != null && elementToCheck.Type == originalElement.Type && !elementToCheck.IsCorner && !elementToCheck.IsUnified;

//             if (originalElement.Type == ElementType.Gate)
//             {
//                 if (originalElement.LevelElementData.BlockColor != elementToCheck.LevelElementData.BlockColor)
//                     result = false;
//             }

//             return result;
//         }

//         private bool IsBorder(Vector2 position)
//         {
//             BorderData element = GetMatrixElement(position, true);

//             if (element != null)
//             {
//                 return element.Type == ElementType.Border;
//             }
//             else
//             {
//                 return false;
//             }
//         }

//         private bool IsCorner(BorderData borderData)
//         {
//             //checking right neighbour
//             if (IsBorder(borderData.Position.AddToX(1)) || IsGate(borderData.Position.AddToX(1)))
//             {
//                 //checking if element has border neighbour above
//                 if (IsBorder(borderData.Position.AddToY(1)) || IsGate(borderData.Position.AddToY(1)))
//                 {
//                     return true;
//                 }
//                 //checking if element has border neighbour below
//                 else if (IsBorder(borderData.Position.AddToY(-1)) || IsGate(borderData.Position.AddToY(-1)))
//                 {
//                     return true;

//                 }
//             }
//             //checking left neighbour
//             else if (IsBorder(borderData.Position.AddToX(-1)) || IsGate(borderData.Position.AddToX(-1)))
//             {
//                 //checking if element has border neighbour above
//                 if (IsBorder(borderData.Position.AddToY(1)) || IsGate(borderData.Position.AddToY(1)))
//                 {
//                     return true;
//                 }
//                 //checking if element has border neighbour below
//                 else if (IsBorder(borderData.Position.AddToY(-1)) || IsGate(borderData.Position.AddToY(-1)))
//                 {
//                     return true;
//                 }
//             }

//             return false;
//         }

//         private Vector3 CalculateCornerRotaion(BorderData borderData)
//         {
//             if (borderData.Offset == RIGHT_TOP_OFFSET)
//             {
//                 if (borderData.IsConvex)
//                 {
//                     return Vector3.up * 90f;
//                 }
//                 else
//                 {
//                     return Vector3.up * 270f;
//                 }
//             }
//             else if (borderData.Offset == RIGHT_BOTTOM_OFFSET)
//             {
//                 if (borderData.IsConvex)
//                 {
//                     return Vector3.up * 180f;
//                 }
//                 else
//                 {
//                     return Vector3.zero;
//                 }
//             }
//             else if (borderData.Offset == LEFT_TOP_OFFSET)
//             {
//                 if (borderData.IsConvex)
//                 {
//                     return Vector3.zero;
//                 }
//                 else
//                 {
//                     return Vector3.up * 180f;
//                 }
//             }
//             else if (borderData.Offset == LEFT_BOTTOM_OFFSET)
//             {
//                 if (borderData.IsConvex)
//                 {
//                     return Vector3.up * 270f;
//                 }
//                 else
//                 {
//                     return Vector3.up * 90f;
//                 }
//             }

//             return Vector3.zero;
//         }

//         private Vector3 CalculateCornerOffset(BorderData borderData)
//         {
//             //checking right neighbour
//             if (IsBorder(borderData.Position.AddToX(1)) || IsGate(borderData.Position.AddToX(1)))
//             {
//                 //checking if element has border neighbour above
//                 if (IsBorder(borderData.Position.AddToY(1)) || IsGate(borderData.Position.AddToY(1)))
//                 {
//                     //checking if element is convex
//                     BorderData diagonalElement = GetMatrixElement(borderData.Position.AddToX(-1).AddToY(-1), true);

//                     if (diagonalElement == null || diagonalElement.Type == ElementType.Empty)
//                     {
//                         borderData.InitConvexState(true);

//                         return RIGHT_TOP_OFFSET;
//                     }
//                     else
//                     {
//                         borderData.InitConvexState(false);

//                         return LEFT_BOTTOM_OFFSET;
//                     }
//                 }
//                 //checking if element has border neighbour below
//                 else if (IsBorder(borderData.Position.AddToY(-1)) || IsGate(borderData.Position.AddToY(-1)))
//                 {
//                     //checking if element is convex
//                     BorderData diagonalElement = GetMatrixElement(borderData.Position.AddToX(-1).AddToY(1), true);

//                     if (diagonalElement == null || diagonalElement.Type == ElementType.Empty)
//                     {
//                         borderData.InitConvexState(true);
//                         return RIGHT_BOTTOM_OFFSET;
//                     }
//                     else
//                     {
//                         borderData.InitConvexState(false);
//                         return LEFT_TOP_OFFSET;
//                     }
//                 }
//             }
//             //checking left neighbour
//             else if (IsBorder(borderData.Position.AddToX(-1)) || IsGate(borderData.Position.AddToX(-1)))
//             {
//                 //checking if element has border neighbour above
//                 if (IsBorder(borderData.Position.AddToY(1)) || IsGate(borderData.Position.AddToY(1)))
//                 {
//                     //checking if element is convex
//                     BorderData diagonalElement = GetMatrixElement(borderData.Position.AddToX(1).AddToY(-1), true);

//                     if (diagonalElement == null || diagonalElement.Type == ElementType.Empty)
//                     {
//                         borderData.InitConvexState(true);
//                         return LEFT_TOP_OFFSET;
//                     }
//                     else
//                     {
//                         borderData.InitConvexState(false);
//                         return RIGHT_BOTTOM_OFFSET;
//                     }
//                 }
//                 //checking if element has border neighbour below
//                 else if (IsBorder(borderData.Position.AddToY(-1)) || IsGate(borderData.Position.AddToY(-1)))
//                 {
//                     //checking if element is convex
//                     BorderData diagonalElement = GetMatrixElement(borderData.Position.AddToX(1).AddToY(1), true);

//                     if (diagonalElement == null || diagonalElement.Type == ElementType.Empty)
//                     {
//                         borderData.InitConvexState(true);
//                         return LEFT_BOTTOM_OFFSET;
//                     }
//                     else
//                     {
//                         borderData.InitConvexState(false);
//                         return RIGHT_TOP_OFFSET;
//                     }
//                 }
//             }

//             return Vector3.zero;
//         }

//         private Vector3 CalculateBorderOffset(BorderData borderData)
//         {
//             BorderData neighbour;

//             // checking right neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToX(1), true);

//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 borderData.InitHorizonalState(false);
//                 return LEFT_OFFSET;
//             }

//             // checking left neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToX(-1), true);
//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 borderData.InitHorizonalState(false);
//                 return RIGHT_OFFSET;
//             }

//             // checking top neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToY(1), true);
//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 borderData.InitHorizonalState(true);
//                 return BOTTOM_OFFSET;
//             }

//             // checking bottom neighbour
//             neighbour = GetMatrixElement(borderData.Position.AddToY(-1), true);
//             if (neighbour == null || neighbour.Type == ElementType.Empty)
//             {
//                 borderData.InitHorizonalState(true);
//                 return TOP_OFFSET;
//             }

//             return Vector3.zero;
//         }

//         private Vector3 CalculateBorderRotation(BorderData borderData)
//         {
//             if (borderData.IsHorizonal)
//             {
//                 return new Vector3(0f, 90f, 0f);
//             }
//             else
//             {
//                 return Vector3.zero;
//             }
//         }


//         public BorderData GetMatrixElement(Vector2 position, bool skipOutOfBoundsLog = false)
//         {
//             if (!IsWithinLevelBounds(position))
//             {
//                 if (!skipOutOfBoundsLog)
//                     Debug.LogErrorFormat("Requested element with position {0} is outside of the level's bounds", position);

//                 return null;
//             }

//             return bordersMatrix[(int)position.x, (int)position.y];
//         }

//         public bool IsWithinLevelBounds(Vector2 position)
//         {
//             if (position.x < 0 || position.y < 0)
//                 return false;

//             if (position.x > size.x - 1)
//                 return false;

//             if (position.y > size.y - 1)
//                 return false;

//             return true;
//         }

//         public bool IsWithinLevelBounds(Vector2Int position)
//         {
//             if (position.x < 0 || position.y < 0)
//                 return false;

//             if (position.x > size.x - 1)
//                 return false;

//             if (position.y > size.y - 1)
//                 return false;

//             return true;
//         }

//         public GateDirection.Type NearGate(LevelBlockBehavior levelBlockBehavior, MovementMatrix movementMatrix)
//         {
//             Vector2Int position = levelBlockBehavior.MatrixPosition;
//             LevelFigure figure = levelBlockBehavior.Figure;

//             for (int d = 0; d < GateDirection.DIRECTIONS.Length; d++)
//             {
//                 GateDirection gateDirection = GateDirection.DIRECTIONS[d];

//                 bool isNear = true;

//                 int alignedSize = gateDirection.GetAlignedSize(figure);
//                 for (int i = 0; i < alignedSize; i++)
//                 {
//                     Vector2Int checkPosition = gateDirection.CalculateAlignedPosition(position, figure, i);

//                     if (!IsWithinLevelBounds(checkPosition))
//                     {
//                         isNear = false;
//                         break;
//                     }

//                     BorderData element = bordersMatrix[checkPosition.x, checkPosition.y];
//                     if (element == null || element.Type != ElementType.Gate)
//                     {
//                         isNear = false;
//                         break;
//                     }

//                     if (!levelBlockBehavior.CanGoThroughGate(bordersMatrix[checkPosition.x, checkPosition.y].GateBehavior))
//                     {
//                         isNear = false;
//                         break;
//                     }
//                 }

//                 if (isNear)
//                 {
//                     int steps = gateDirection.GetNonAlignedSize(figure) - 1;
//                     if (!IsMovementBlocked(position, figure, gateDirection.PositionOffset, steps, movementMatrix))
//                     {
//                         return (GateDirection.Type)d;
//                     }
//                 }
//             }

//             return GateDirection.Type.None;
//         }

//         public IEnumerable<(GateDirection.Type, GateBehavior)> NearGates(LevelBlockBehavior levelBlockBehavior, MovementMatrix movementMatrix)
//         {
//             Vector2Int position = levelBlockBehavior.MatrixPosition;
//             LevelFigure figure = levelBlockBehavior.Figure;

//             for (int d = 0; d < GateDirection.DIRECTIONS.Length; d++)
//             {
//                 GateDirection gateDirection = GateDirection.DIRECTIONS[d];

//                 int alignedSize = gateDirection.GetAlignedSize(figure);
//                 int steps = gateDirection.GetNonAlignedSize(figure) - 1;

//                 bool isNear = true;

//                 GateBehavior gateBehavior = null;
//                 Vector2Int checkPosition = position;

//                 for (int i = 0; i < alignedSize; i++)
//                 {
//                     checkPosition = gateDirection.CalculateAlignedPosition(position, figure, i);

//                     if (!IsWithinLevelBounds(checkPosition))
//                     {
//                         isNear = false;
//                         break;
//                     }

//                     BorderData element = bordersMatrix[checkPosition.x, checkPosition.y];
//                     if (element == null || element.Type != ElementType.Gate)
//                     {
//                         isNear = false;
//                         break;
//                     }

//                     gateBehavior = bordersMatrix[checkPosition.x, checkPosition.y].GateBehavior;
//                     if (!levelBlockBehavior.CanGoThroughGate(gateBehavior))
//                     {
//                         isNear = false;
//                         break;
//                     }
//                 }

//                 if (isNear)
//                 {
//                     if (!IsMovementBlocked(position, figure, gateDirection.PositionOffset, steps, movementMatrix))
//                     {
//                         yield return ((GateDirection.Type)d, gateBehavior);
//                     }
//                 }
//             }
//         }

//         public GateDirection.Type NearGate(LevelBlockBehavior levelBlockBehavior, MovementMatrix movementMatrix, ref GateBehavior gateBehavior)
//         {
//             Vector2Int position = levelBlockBehavior.MatrixPosition;
//             LevelFigure figure = levelBlockBehavior.Figure;

//             for (int d = 0; d < GateDirection.DIRECTIONS.Length; d++)
//             {
//                 GateDirection gateDirection = GateDirection.DIRECTIONS[d];

//                 int alignedSize = gateDirection.GetAlignedSize(figure);
//                 int steps = gateDirection.GetNonAlignedSize(figure) - 1;

//                 bool isNear = true;

//                 Vector2Int checkPosition = position;

//                 for (int i = 0; i < alignedSize; i++)
//                 {
//                     checkPosition = gateDirection.CalculateAlignedPosition(position, figure, i);

//                     if (!IsWithinLevelBounds(checkPosition))
//                     {
//                         isNear = false;
//                         break;
//                     }

//                     BorderData element = bordersMatrix[checkPosition.x, checkPosition.y];
//                     if(element == null || element.Type != ElementType.Gate)
//                     {
//                         isNear = false;
//                         break;
//                     }

//                     gateBehavior = bordersMatrix[checkPosition.x, checkPosition.y].GateBehavior;
//                     if (!levelBlockBehavior.CanGoThroughGate(gateBehavior))
//                     {
//                         isNear = false;
//                         break;
//                     }
//                 }

//                 if (isNear)
//                 {
//                     if (!IsMovementBlocked(position, figure, gateDirection.PositionOffset, steps, movementMatrix))
//                     {
//                         return (GateDirection.Type)d;
//                     }
//                 }
//             }

//             return GateDirection.Type.None;
//         }

//         private bool IsFigureBlocked(LevelFigure figure, Vector2Int position, MovementMatrix movementMatrix)
//         {
//             Vector2Int size = figure.Size;
//             Vector2Int pivotPoint = figure.PivotPoint;
//             int[,] matrix = movementMatrix.Matrix;
//             int width = movementMatrix.Width;
//             int height = movementMatrix.Height;

//             for (int fX = 0; fX < size.x; fX++)
//             {
//                 for (int fY = 0; fY < size.y; fY++)
//                 {
//                     int index = fX + fY * size.x;
//                     if (figure.Points[index].IsFilled)
//                     {
//                         int x = position.x + fX;
//                         int y = position.y + fY;

//                         if (x < 0 || x >= width || y < 0 || y >= height)
//                         {
//                             continue;
//                         }

//                         if(matrix[x, y] == MovementMatrix.STATUS_OCCUPIED)
//                         {
//                             return true;
//                         }
//                     }
//                 }
//             }

//             return false;
//         }

//         private bool IsMovementBlocked(Vector2Int position, LevelFigure figure, Vector2Int offsetPosition, int steps, MovementMatrix movementMatrix)
//         {
//             PointData[] figurePoints = figure.Points;
//             Vector2Int figureSize = figure.Size;

//             for(int i = 0; i < steps; i++)
//             {
//                 Vector2Int offset = offsetPosition * (i + 1);
//                 if(IsFigureBlocked(figure, position + offset, movementMatrix))
//                 {
//                     return true;
//                 }
//             }

//             return false;
//         }

//         public bool IsGate(Vector2Int position)
//         {
//             BorderData element = GetMatrixElement(position, true);

//             if (element != null)
//             {
//                 return element.Type == ElementType.Gate;
//             }
//             else
//             {
//                 return false;
//             }
//         }

//         public Bounds GetBounds()
//         {
//             Bounds bounds = new Bounds();
//             bounds.center = new Vector3((minXPosition + maxXPosition) * 0.5f, 0f, (minYPosition + maxYPosition) * 0.5f);
//             bounds.size = new Vector3(maxXPosition - minXPosition, 0f, maxYPosition - minYPosition);

//             return bounds;
//         }
//     }
// }

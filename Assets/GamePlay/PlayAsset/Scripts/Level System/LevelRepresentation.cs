// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// namespace Watermelon
// {
//     public class LevelRepresentation
//     {
//         public LevelData LevelData { get; private set; }
//         public Vector2Int Size => LevelData.Size;

//         public Transform LevelTransform { get; private set; }
//         public EnvironmentData EnvironmentData { get; private set; }
//         public EnvironmentSpawner EnvironmentSpawner { get; private set; }

//         public LevelElementData[,] LevelMatrix { get; private set; }
//         public List<LevelBlockBehavior> ActiveBlocks { get; private set; }
//         public List<InteractableObjectBehavior> InteractableObjects { get; private set; }

//         public bool AllBlocksCleared => ActiveBlocks.Count == 0;

//         public Bounds LevelBounds => EnvironmentSpawner.GetBounds();

//         public LevelRepresentation(LevelData level, EnvironmentData environmentData)
//         {
//             EnvironmentData = environmentData;
//             LevelData = level;

//             GameObject levelObject = new GameObject("[LEVEL]");
//             LevelTransform = levelObject.transform;

//             Vector2Int levelSize = Size;
//             LevelElementData[] levelElements = level.LevelElements;

//             LevelMatrix = new LevelElementData[levelSize.x, levelSize.y];

//             LevelElementData element;

//             for (int i = 0; i < levelElements.Length; i++)
//             {
//                 element = levelElements[i];

//                 LevelMatrix[element.Position.x, element.Position.y] = element;
//             }

//             EnvironmentSpawner = new EnvironmentSpawner(this);
//         }

//         public void SpawnEnvironment()
//         {
//             EnvironmentSpawner.SpawnBorders(LevelData.LevelElements);
//         }

//         public void SpawnInteractiveObjects()
//         {
//             InteractableObjects = new List<InteractableObjectBehavior>();

//             LevelElementData[] levelElements = LevelData.LevelElements;
//             for (int i = 0; i < levelElements.Length; i++)
//             {
//                 if (levelElements[i].Type == ElementType.InteractableObject)
//                 {
//                     InteractableObjectType interactableObjectType = levelElements[i].InteractableObjectData.Type;
//                     if (interactableObjectType != InteractableObjectType.None)
//                     {
//                         LevelInteractableObjectData interactableObjectData = LevelController.GetInteractableObject(interactableObjectType);
//                         if (interactableObjectData != null)
//                         {
//                             GameObject interactableObject = Object.Instantiate(interactableObjectData.Prefab, LevelTransform);

//                             interactableObject.transform.position = new Vector3(levelElements[i].Position.x, 0, levelElements[i].Position.y);

//                             InteractableObjectBehavior interactableObjectBehavior = interactableObject.GetComponent<InteractableObjectBehavior>();
//                             interactableObjectBehavior.Init(levelElements[i].InteractableObjectData, levelElements[i].Position);

//                             InteractableObjects.Add(interactableObjectBehavior);
//                         }
//                     }
//                 }
//             }
//         }

//         public LevelBlockBehavior SpawnBlock(Vector2Int position, BlockType blockType, BlockColor blockColor, IEnumerable<BlockEffectData> effects)
//         {
//             BlockData blockData = null;//LevelController.GetBlockData(blockType);
//             if (blockData != null)
//             {
//                 GameObject blockPrefab = blockData.Prefab;

//                 GameObject block = Object.Instantiate(blockPrefab, LevelTransform);

//                 LevelBlockBehavior blockBehavior = block.GetComponent<LevelBlockBehavior>();
//                 Vector2Int pivotPoint = blockBehavior.Figure.PivotPoint;

//                 int x = position.x - pivotPoint.x;
//                 int y = position.y - pivotPoint.y;

//                 block.transform.position = new Vector3(x, 0, y);

//                 blockBehavior.Init(blockData);

//                 // BlockColorData colorData = LevelController.GetBlockColorData(blockColor);

//                 // blockBehavior.SetColor(colorData);

//                 if (effects != null)
//                 {
//                     foreach (BlockEffectData effect in effects)
//                     {
//                         BlockEffectType effectType = effect.Type;
//                         if (effectType != BlockEffectType.None)
//                         {
//                             LevelBlockEffectData effectData = LevelController.GetEffectData(effectType);

//                             effectData.Behavior.ApplyEffect(blockBehavior, effect);
//                         }
//                     }
//                 }

//                 ActiveBlocks.Add(blockBehavior);

//                 return blockBehavior;
//             }

//             return null;
//         }

//         public void SpawnBlocks()
//         {
//             ActiveBlocks = new List<LevelBlockBehavior>();

//             LevelElementData[] levelElements = LevelData.LevelElements;
//             for (int i = 0; i < levelElements.Length; i++)
//             {
//                 LevelElementData levelElement = levelElements[i];
//                 if (levelElement.Type == ElementType.Block)
//                 {
//                     SpawnBlock(levelElement.Position, levelElement.BlockType, levelElement.BlockColor, levelElement.BlockEffects);
//                 }
//             }

//             CombineBlocks();
//         }

//         public void CombineBlocks()
//         {
//             IEnumerable<List<LevelBlockBehavior>> combinedBlocks = ActiveBlocks.Where(block => block.HasEffect(BlockEffectType.Combines)).GroupBy(block => block.GetEffect(BlockEffectType.Combines).EffectData.combineGroupID).Select(group => group.ToList());
//             foreach (List<LevelBlockBehavior> blockGroup in combinedBlocks)
//             {
//                 CombinedEffectBehavior.CombineBlocks(blockGroup);
//             }
//         }

//         public void OnBlockDestructed(LevelBlockBehavior block)
//         {
//             ActiveBlocks.Remove(block);
//         }

//         public void DrawGizmos()
//         {
//             if (EnvironmentSpawner == null) return;

//             var levelBounds = EnvironmentSpawner.GetBounds();

//             Gizmos.color = Color.red;
//             Gizmos.DrawWireCube(levelBounds.center, levelBounds.size);
//         }
//     }
// }

// #pragma warning disable 0649

// using System.Collections.Generic;
// using UnityEngine;

// namespace Watermelon
// {
//     public class LevelBlockBehavior : MonoBehaviour, IClickableObject
//     {
//         protected BlockData blockData;
//         public BlockData BlockData => blockData;

//         protected BlockColorData colorData;
//         public BlockColorData ColorData => colorData;

//         [SerializeField] LevelFigure figure;
//         public LevelFigure Figure => figure;

//         [SerializeField] MeshRenderer meshRenderer;
//         public MeshRenderer MeshRenderer => meshRenderer;

//         public Vector2Int MatrixPosition => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

//         private Vector3 figurePivotOffset;
//         public Vector3 FigurePivotOffset => figurePivotOffset;

//         private List<BlockEffectBehavior> effects;
//         public List<BlockEffectBehavior> Effects => effects;

//         private Tween shakeTweenCase;

//         private bool isCollected;
//         public bool IsCollected => isCollected;

//         public event SimpleCallback BlockCollected;

//         public void Init(BlockData blockData)
//         {
//             this.blockData = blockData;

//             figurePivotOffset = new Vector3(figure.PivotPoint.x, 0, figure.PivotPoint.y);

//             isCollected = false;

//             effects = new List<BlockEffectBehavior>();
//         }

//         public void SetColor(BlockColorData colorData)
//         {
//             this.colorData = colorData;

//             meshRenderer.material = colorData.Material;
//         }

//         private void OnDestroy()
//         {
//             shakeTweenCase.Kill();
//         }

//         public void OnClickBlocked()
//         {
//             shakeTweenCase.Kill();
//             shakeTweenCase = transform.DOShake(0.05f, 0.15f);
//         }

//         public void OnObjectClicked()
//         {
//             // LevelController.OnObjectPicked(this);
//         }

//         public bool CanBeClicked()
//         {
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (!effect.IsClickable())
//                     return false;
//             }

//             return true;
//         }

//         public List<LevelBlockBehavior> GetLinkedBlocks()
//         {
//             List<LevelBlockBehavior> linked = new List<LevelBlockBehavior>();
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 foreach (LevelBlockBehavior block in effect.GetLinkedBlocks())
//                     linked.Add(block);
//             }

//             return linked;
//         }

//         public bool MoveMultiplyObjects()
//         {
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (effect.MoveMultiplyObjects())
//                     return true;
//             }
//             return false;
//         }

//         public bool CanGoThroughGate(GateBehavior gateBehavior)
//         {
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (!effect.CanGoThroughGate(gateBehavior))
//                     return false;
//             }

//             return gateBehavior.Data.LevelElementData.BlockColor == GetActiveBlockColor();
//         }

//         public void OnBlockEnteredGate(LevelBlockBehavior levelBlockBehavior, GateBehavior gateBehavior)
//         {
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 effect.OnBlockEnteredGate(levelBlockBehavior, gateBehavior);
//             }
//         }

//         public void OnBlockDestructed(LevelBlockBehavior levelBlockBehavior)
//         {
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 effect.OnBlockDestructed(levelBlockBehavior);
//             }
//         }

//         public void OnBlockCollected()
//         {
//             if (isCollected) return;

//             isCollected = true;
//             BlockCollected?.Invoke();

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 effect.OnBlockCollected();
//             }

//             LevelRepresentation levelRepresentation = LevelController.LevelRepresentation;
//             List<LevelBlockBehavior> levelBlocks = levelRepresentation.ActiveBlocks;
//             foreach (LevelBlockBehavior block in levelBlocks)
//             {
//                 List<BlockEffectBehavior> blockEffects = block.Effects;
//                 foreach (BlockEffectBehavior effect in blockEffects)
//                 {
//                     if (!effect.IsActive) continue;

//                     effect.OnBlockCollectedGlobal(this);
//                 }
//             }

//             List<GateBehavior> levelGates = levelRepresentation.EnvironmentSpawner.Gates;
//             foreach (GateBehavior gate in levelGates)
//             {
//                 List<GateEffectBehavior> gateEffects = gate.Effects;
//                 foreach (GateEffectBehavior effect in gateEffects)
//                 {
//                     if (!effect.IsActive) continue;

//                     effect.OnBlockCollectedGlobal(this);
//                 }
//             }
//         }

//         public void OnBlockSplit(List<LevelBlockBehavior> newBlocks)
//         {
//             if (newBlocks.Count == 0)
//             {
//                 OnBlockCollected();

//                 return;
//             }

//             isCollected = true;
//             BlockCollected?.Invoke();

//             List<BlockEffectData> reapplicableEffects = new List<BlockEffectData>();
//             List<BlockEffectData> totalActiveEffects = new List<BlockEffectData>();
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 effect.OnBlockSplit();

//                 if (!effect.IsActive) continue;

//                 BlockEffectData effectData = effect.GetCurrentEffectData();

//                 totalActiveEffects.Add(effectData);

//                 if (!effect.CanBeReapplied()) continue;

//                 reapplicableEffects.Add(effectData);
//             }


//             LevelBlockBehavior randomBlock = newBlocks.GetRandomItem();
//             foreach (LevelBlockBehavior block in newBlocks)
//             {
//                 List<BlockEffectData> blockEffects = reapplicableEffects;
//                 if (block == randomBlock)
//                     blockEffects = totalActiveEffects;

//                 foreach (BlockEffectData effect in blockEffects)
//                 {
//                     LevelBlockEffectData effectData = LevelController.GetEffectData(effect.Type);

//                     effectData.Behavior.ApplyEffect(block, effect);
//                 }
//             }
//         }

//         public bool OnGateEntered(GateBehavior gateBehavior, GateDirection gateDirection)
//         {
//             if (!gateBehavior.CanGoThroughGate(this))
//                 return false;

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (!effect.OnGateEntered(gateBehavior, gateDirection))
//                     return false;
//             }

//             return true;
//         }

//         public BlockColor GetActiveBlockColor()
//         {
//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 BlockColor overrideColor = effect.GetOverridedBlockColor();
//                 if (overrideColor != colorData.Type)
//                     return overrideColor;
//             }

//             return colorData.Type;
//         }

//         public Vector2Int[] GetOccupiedCells()
//         {
//             Vector2Int[] occupiedCells = new Vector2Int[figure.ActivePoints];

//             Vector2Int size = figure.Size;
//             Vector2Int currentPosition = MatrixPosition;

//             int tempIndex = 0;
//             for (int y = 0; y < size.y; y++)
//             {
//                 for (int x = 0; x < size.x; x++)
//                 {
//                     int index = x + y * size.x;

//                     if (figure.Points[index].IsFilled)
//                     {
//                         occupiedCells[tempIndex] = new Vector2Int(currentPosition.x + x, currentPosition.y + y);

//                         tempIndex++;
//                     }
//                 }
//             }

//             return occupiedCells;
//         }

//         public Vector2Int GetRandomOccupiedCell()
//         {
//             int pointIndex = Random.Range(0, figure.ActivePoints);

//             Vector2Int size = figure.Size;
//             int tempIndex = 0;

//             for (int y = 0; y < size.y; y++)
//             {
//                 for (int x = 0; x < size.x; x++)
//                 {
//                     int index = x + y * size.x;

//                     if (figure.Points[index].IsFilled)
//                     {
//                         if (tempIndex == pointIndex)
//                             return new Vector2Int(x, y);

//                         tempIndex++;
//                     }
//                 }
//             }

//             return new Vector2Int(0, 0);
//         }

//         #region Effect
//         public bool HasEffect(BlockEffectType effectType)
//         {
//             if (effects.IsNullOrEmpty())
//                 return false;

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (effect.Type == effectType)
//                     return true;
//             }

//             return false;
//         }

//         public BlockEffectBehavior GetEffect(BlockEffectType effectType)
//         {
//             if (effects.IsNullOrEmpty())
//                 return null;

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (effect.Type == effectType)
//                     return effect;
//             }

//             return null;
//         }

//         public T GetEffect<T>(BlockEffectType effectType) where T : BlockEffectBehavior
//         {
//             if (effects.IsNullOrEmpty())
//                 return null;

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (!effect.IsActive) continue;

//                 if (effect.Type == effectType)
//                     return effect as T;
//             }

//             return null;
//         }

//         public void ApplyEffect(BlockEffectBehavior effect)
//         {
//             int effectsCount = effects.Count;

//             effects.Add(effect);

//             effect.SetOrder(effectsCount);
//             effect.OnCreated(this);

//             for (int i = 0; i < effectsCount; i++)
//             {
//                 effects[i].OnNewEffectAddedToBlock(effect);
//             }
//         }

//         public void DisableEffects()
//         {
//             if (effects.IsNullOrEmpty())
//                 return;

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (effect.IsActive)
//                     effect.OnDisabled(this);

//                 Destroy(effect.gameObject);
//             }

//             effects.Clear();
//         }

//         public void OnEffectDisabled(BlockEffectBehavior effect)
//         {
//             effect.OnDisabled(this);
//         }

//         public bool HasActiveEffect()
//         {
//             if (effects.IsNullOrEmpty())
//                 return false;

//             foreach (BlockEffectBehavior effect in effects)
//             {
//                 if (effect.IsActive)
//                     return true;
//             }

//             return false;
//         }
//         #endregion

//         private void OnDrawGizmos()
//         {
//             if (figure == null)
//                 return;

//             var size = figure.Size;

//             for (int x = 0; x < size.x; x++)
//             {
//                 for (int y = 0; y < size.y; y++)
//                 {
//                     int index = x + y * size.x;

//                     if (!figure.Points.IsNullOrEmpty() && figure.Points.IsInRange(index) && figure.Points[index].IsFilled)
//                     {
//                         Gizmos.color = Color.red;
//                         Gizmos.DrawWireCube(transform.position + new Vector3(x, 0.5f, y), Vector3.one);
//                     }
//                 }
//             }
//         }
//     }
// }

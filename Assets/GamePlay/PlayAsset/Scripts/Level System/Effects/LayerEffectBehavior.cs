// using System.Collections.Generic;
// using UnityEngine;

// namespace Watermelon
// {
//     public sealed class LayerEffectBehavior : BlockEffectBehavior
//     {
//         [SerializeField] float yOffset = 0.2f;
//         [SerializeField] SkinData[] skins;

//         private SkinData activeSkinData;

//         private GameObject effectObject;

//         private bool isCollected;

//         public override void OnCreated(LevelBlockBehavior blockBehavior)
//         {
//             activeSkinData = GetSelectedSkinData();
//             if (activeSkinData == null)
//             {
//                 DisableEffect();

//                 return;
//             }

//             VisualData visualData = activeSkinData.GetVisualData(blockBehavior.BlockData.Type);
//             if (visualData == null)
//             {
//                 DisableEffect();

//                 return;
//             }

//             Transform blockVisualsTransform = linkedBlock.MeshRenderer.transform;
//             transform.localPosition = blockVisualsTransform.localPosition;
//             transform.localRotation = blockVisualsTransform.localRotation;
//             transform.localScale = blockVisualsTransform.localScale;

//             effectObject = Instantiate(visualData.Prefab, transform);
//             effectObject.transform.localPosition = new Vector3(0, yOffset, 0);

//             MeshRenderer meshRenderer = effectObject.GetComponent<MeshRenderer>();
//             meshRenderer.material = linkedBlock.ColorData.Material;

//             MeshRenderer blockMeshRenderer = linkedBlock.MeshRenderer;
//             // blockMeshRenderer.material = LevelController.GetBlockColorData(effectData.layeredBlockColor).Material;
//         }

//         public override BlockColor GetOverridedBlockColor()
//         {
//             return effectData.layeredBlockColor;
//         }

//         public override bool OnGateEntered(GateBehavior gateBehavior, GateDirection gateDirection)
//         {
//             if (!isCollected)
//             {
//                 isCollected = true;

//                 effectObject.SetActive(false);

//                 LevelBlockBehavior blockBehavior = linkedBlock;

//                 // Reset the block color
//                 MeshRenderer blockMeshRenderer = blockBehavior.MeshRenderer;
//                 blockMeshRenderer.material = blockBehavior.ColorData.Material;

//                 // Spawn duplicate block, change color and move it to the gate
//                 GameObject duplicate = Instantiate(blockMeshRenderer.gameObject);

//                 // BlockColorData colorData = LevelController.GetBlockColorData(effectData.layeredBlockColor);

//                 MeshRenderer duplicateMeshRenderer = duplicate.GetComponent<MeshRenderer>();
//                 // duplicateMeshRenderer.material = colorData.Material;

//                 BlockClip blockClip = new BlockClip(duplicateMeshRenderer, gateBehavior.transform.position + gateDirection.ClipOffset, gateDirection.DirectionNormal);
//                 // BlockDestructionParticle blockDestructionParticle = new BlockDestructionParticle(blockBehavior, colorData.Material, gateBehavior, gateDirection);

//                 int elementsSize = gateDirection.GetAlignedSize(blockBehavior.Figure);

//                 Transform duplicateTransform = duplicate.transform;
//                 duplicateTransform.position = blockMeshRenderer.transform.position + new Vector3(0, 0.02f, 0);
//                 duplicateTransform.DOMove(transform.position + gateDirection.GetMoveOffset(blockBehavior.Figure), LevelController.BLOCK_MOVE_DURATION * elementsSize).SetEase(LevelController.BLOCK_MOVE_EASE_TYPE).OnComplete(() =>
//                 {
//                     blockClip.Destroy();
//                     // blockDestructionParticle.Stop();

//                     Destroy(duplicate);

//                     DisableEffect();
//                 });

//                 LevelRepresentation levelRepresentation = LevelController.LevelRepresentation;

//                 List<LevelBlockBehavior> activeBlocks = levelRepresentation.ActiveBlocks;
//                 foreach (LevelBlockBehavior block in activeBlocks)
//                 {
//                     block.OnBlockDestructed(blockBehavior);
//                 }

//                 List<GateBehavior> gates = levelRepresentation.EnvironmentSpawner.Gates;
//                 foreach (GateBehavior gate in gates)
//                 {
//                     gate.OnBlockDestructed(blockBehavior);
//                 }

//                 // LevelController.OnBlockCollected(blockBehavior, gateBehavior);
//             }

//             return false;
//         }

//         private SkinData GetSelectedSkinData()
//         {
// #if UNITY_EDITOR
//             if (!Application.isPlaying)
//             {
//                 if (skins.Length > 0)
//                     return skins[0];

//                 Debug.LogError("Effect visuals isn't configured", gameObject);

//                 return null;
//             }
// #endif

//             ISkinData selectedSkin = SkinController.Instance.GetSelectedSkin<LevelSkinDatabase>();

//             string skinID = selectedSkin.ID;
//             for (int i = 0; i < skins.Length; i++)
//             {
//                 if (skins[i].SkinID == skinID)
//                 {
//                     return skins[i];
//                 }
//             }

//             Debug.LogError($"Layer effect skin data not found for skin ID: {skinID}", gameObject);

//             return null;
//         }

//         [System.Serializable]
//         public class SkinData
//         {
//             [SkinPicker]
//             [SerializeField] string skinID;
//             public string SkinID => skinID;

//             [SerializeField] VisualData[] visualDatas;
//             public VisualData[] VisualDatas => visualDatas;

//             public VisualData GetVisualData(BlockType blockType)
//             {
//                 for (int i = 0; i < visualDatas.Length; i++)
//                 {
//                     if (visualDatas[i].BlockType == blockType)
//                     {
//                         return visualDatas[i];
//                     }
//                 }

//                 Debug.LogError($"Layer effect visual data not found for block type: {blockType}");

//                 return null;
//             }
//         }

//         [System.Serializable]
//         public class VisualData
//         {
//             [SerializeField] BlockType blockType;
//             public BlockType BlockType => blockType;

//             [SerializeField] GameObject prefab;
//             public GameObject Prefab => prefab;
//         }
//     }
// }

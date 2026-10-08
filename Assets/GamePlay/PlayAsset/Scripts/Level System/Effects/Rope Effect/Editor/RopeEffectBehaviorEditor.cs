using UnityEngine;
using UnityEditor;
using System;

namespace Watermelon
{
    [CustomEditor(typeof(RopeEffectBehavior))]
    public class RopeEffectBehaviorEditor : CustomInspector
    {
        private static BlockType blockType;

        private RopeEffectEditorHandler editHandler;
        private bool isPreviewActive => editHandler != null;

        private SerializedProperty prefabSerializedProperty;
        private SerializedProperty ropePositionDatasProperty;

        private RopeEffectBehavior ropeBehavior;

        protected override void OnEnable()
        {
            base.OnEnable();

            ropeBehavior = (RopeEffectBehavior)target;

            prefabSerializedProperty = serializedObject.FindProperty("ropePrefab");
            ropePositionDatasProperty = serializedObject.FindProperty("ropePositionDatas");

            editHandler = ropeBehavior.transform.GetComponentInChildren<RopeEffectEditorHandler>();
            if (editHandler != null)
                blockType = editHandler.BlockType;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayoutCustom.BeginBoxGroup("Configuration Tool");

            blockType = (BlockType)EditorGUILayout.EnumPopup("Block Type", blockType);

            EditorGUI.BeginDisabledGroup(isPreviewActive);
            if (GUILayout.Button("Preview"))
            {
                EnablePreview();

                if (GetRopesPositionsIndex(blockType) == -1)
                {
                    CreateNewRopesType(blockType);
                }
            }
            EditorGUI.EndDisabledGroup();

            if (isPreviewActive)
            {
                if (GUILayout.Button("Spawn Rope"))
                {
                    SpawnRope();
                }

                GUILayout.Space(8);

                if (GUILayout.Button("Save"))
                {
                    Save();
                }

                if (GUILayout.Button("Reset"))
                {
                    DisablePreview();
                }
            }

            EditorGUILayoutCustom.EndBoxGroup();
        }

        private void SpawnRope()
        {
            if (!isPreviewActive) return;

            GameObject ropePrefab = prefabSerializedProperty.objectReferenceValue as GameObject;
            GameObject ropeObject = Instantiate(ropePrefab, editHandler.RopesObject.transform);
            ropeObject.transform.localPosition = new Vector3(0, 1, 0);

            Selection.activeGameObject = ropeObject;
        }

        private int GetRopesPositionsIndex(BlockType blockType)
        {
            if (!isPreviewActive) return -1;

            int blockTypeInt = (int)blockType;
            int arraySize = ropePositionDatasProperty.arraySize;
            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty element = ropePositionDatasProperty.GetArrayElementAtIndex(i);
                int blockTypeValue = element.FindPropertyRelative("blockType").intValue;

                if (blockTypeInt == blockTypeValue)
                    return i;
            }

            return -1;
        }

        private int CreateNewRopesType(BlockType blockType)
        {
            ropePositionDatasProperty.serializedObject.Update();

            int arraySize = ropePositionDatasProperty.arraySize;

            ropePositionDatasProperty.arraySize++;

            SerializedProperty element = ropePositionDatasProperty.GetArrayElementAtIndex(arraySize);
            element.FindPropertyRelative("blockType").intValue = (int)blockType;
            element.FindPropertyRelative("transformDatas").arraySize = 0;

            ropePositionDatasProperty.serializedObject.ApplyModifiedProperties();

            return arraySize;
        }

        private void Save()
        {
            if (!isPreviewActive) return;

            int arrayIndex = GetRopesPositionsIndex(editHandler.BlockType);
            if (arrayIndex == -1)
                arrayIndex = CreateNewRopesType(editHandler.BlockType);

            ropePositionDatasProperty.serializedObject.Update();

            SerializedProperty element = ropePositionDatasProperty.GetArrayElementAtIndex(arrayIndex);
            SerializedProperty transformDatas = element.FindPropertyRelative("transformDatas");

            RopeBehavior[] ropeBehaviors = editHandler.RopesObject.GetComponentsInChildren<RopeBehavior>();
            transformDatas.arraySize = ropeBehaviors.Length;

            for (int i = 0; i < ropeBehaviors.Length; i++)
            {
                SerializedProperty transformProperty = transformDatas.GetArrayElementAtIndex(i);
                transformProperty.FindPropertyRelative("position").vector3Value = ropeBehaviors[i].transform.localPosition;
                transformProperty.FindPropertyRelative("rotation").vector3Value = ropeBehaviors[i].transform.localEulerAngles;
                transformProperty.FindPropertyRelative("scale").vector3Value = ropeBehaviors[i].transform.localScale;

                transformProperty.FindPropertyRelative("ropeType").intValue = (int)ropeBehaviors[i].RopeType;
            }

            ropePositionDatasProperty.serializedObject.ApplyModifiedProperties();

            Debug.Log($"Ropes visuals ({editHandler.BlockType}) are saved!");
        }

        private void DisablePreview()
        {
            if (!isPreviewActive) return;

            if (editHandler.BlockObject != null)
                DestroyImmediate(editHandler.BlockObject);

            if (editHandler.RopesObject != null)
                DestroyImmediate(editHandler.RopesObject);

            DestroyImmediate(editHandler.gameObject);

            editHandler = null;
        }

        private void EnablePreview()
        {
            if (isPreviewActive) return;

            BlocksVisualsData blocksVisualsData = EditorUtils.GetAsset<BlocksVisualsData>();
            if (blocksVisualsData == null)
            {
                Debug.LogError("Failed to find BlocksVisualsData.");

                return;
            }

            BlockData blockData = blocksVisualsData.GetBlockData(blockType);
            if (blockData == null)
            {
                Debug.LogError("Failed to get Block Data from BlocksVisualsData");

                return;
            }

            GameObject editHandlerObject = new GameObject("[EDIT HANDLER]");
            editHandlerObject.hideFlags = HideFlags.HideAndDontSave;
            editHandlerObject.transform.SetParent(ropeBehavior.transform);

            editHandler = editHandlerObject.AddComponent<RopeEffectEditorHandler>();
            editHandler.BlockType = blockType;

            GameObject blockObject = Instantiate(blockData.Prefab);
            blockObject.hideFlags = HideFlags.DontSave;
            blockObject.transform.SetParent(ropeBehavior.transform);

            editHandler.BlockObject = blockObject;

            LevelBlockBehavior levelBlockBehavior = blockObject.GetComponent<LevelBlockBehavior>();

            Bounds bounds = levelBlockBehavior.Figure.GetHorizontalCenterBounds();
            levelBlockBehavior.transform.localPosition = -bounds.center;

            GameObject ropesGameObject = new GameObject("[ROPES]");
            ropesGameObject.hideFlags = HideFlags.DontSave;
            ropesGameObject.transform.SetParent(ropeBehavior.transform);

            editHandler.RopesObject = ropesGameObject;

            RopeEffectBehavior.RopePositionData positionData = ropeBehavior.GetPositionData(blockType);
            if (positionData != null)
            {
                GameObject ropePrefab = prefabSerializedProperty.objectReferenceValue as GameObject;
                foreach (RopeEffectBehavior.RopeTransform transformData in positionData.TransformDatas)
                {
                    GameObject ropeObject = Instantiate(ropePrefab, ropesGameObject.transform);
                    Transform ropeTransform = ropeObject.transform;
                    RopeBehavior ropeBehavior = ropeObject.GetComponent<RopeBehavior>();

                    ropeTransform.localPosition = transformData.Position;
                    ropeTransform.localEulerAngles = transformData.Rotation;
                    ropeTransform.localScale = transformData.Scale;

                    SerializedObject ropeSerializedObject = new SerializedObject(ropeBehavior);
                    ropeSerializedObject.Update();
                    ropeSerializedObject.FindProperty("ropeType").enumValueIndex = (int)transformData.RopeType;
                    ropeSerializedObject.ApplyModifiedProperties();

                    ropeBehavior.OnRopeTypeChanged();
                }
            }
        }
    }
}

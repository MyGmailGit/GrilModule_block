// #pragma warning disable 649

// using System;
// using System.Collections.Generic;
// using UnityEngine;

// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEditor.SceneManagement;
// #endif

// namespace Watermelon
// {
//     public class EditorSceneController : MonoBehaviour
//     {
// #if UNITY_EDITOR
//         private static EditorSceneController instance;
//         public static EditorSceneController Instance { get => instance; }
//         private const string PREFS_WIDTH_KEY = "EditorCameraOverrideWidth";

//         [SerializeField] GameObject container;
//         [SerializeField] EnvironmentData environmentData;
//         [SerializeField] LevelDatabase levelDatabase;
//         [SerializeField] LevelSkinDatabase skinDatabase;
//         [SerializeField, SkinPicker] string skinId;
//         [SerializeField] Camera mainCamera;

//         [Space]
//         [SerializeField] Vector3 handlesCubeOffset;
//         [SerializeField] BlocksVisualsData blocksVisuals;
//         [SerializeField] SelectedBlock selectedBlockEditor;
//         [SerializeField] BlockEffect effectEditor;

//         private LevelRepresentation levelRepresentation;
//         private bool isInitiazed;
//         private List<BlockMovementData> blockMovementData;
//         private List<GateData> gateData;
//         private BlockMovementData selectedBlock;
//         private int selectedBlockMovementBehaviourIndex;
//         private BlockMovementManager movementManager;
//         private bool levelPreviewInitialized;
//         private Color backupHandlesColor;
//         private Action<Vector2Int, Vector2Int> handleBlockChangeCallback;
//         private Action<Vector2Int , Vector2Int[], BlockType > handleBlockSpawnCallback;
//         private Action<Vector2Int> handleBlockDeleteCallback;
//         private Action<Vector2Int, BlockColor> handleBlockColorChangeCallback;
//         private Action<Vector2Int, LevelFigure[], BlockType[], Action<object>> handleCreateFigureSelectionPopup;
//         private Action<Vector2Int> handleCreateEffectEditor;
//         private Action<Vector2Int> handleUpdateEffectData;
//         private LevelData levelData;
//         private int selectedGateIndex;
//         private bool isMovementRestricted;
//         private string[] blockColorNames;
//         private BlockColor[] blockoColorValues;
//         private int selectedBlockColorValue;
//         private MonoBehaviorInspector handlesEditor;
//         private int inspectorHeight;

//         //private bool levelChanged;
//         //private ItemSave[] itemsCached;

//         public GameObject Container { get => container; set => container = value; }
//         public SelectedBlock SelectedBlockEditor { get => selectedBlockEditor; set => selectedBlockEditor = value; }
//         public BlockEffect EffectEditor { get => effectEditor; set => effectEditor = value; }
//         public MonoBehaviorInspector HandlesEditor { get => handlesEditor; set => handlesEditor = value; }

//         public EditorSceneController()
//         {
//             instance = this;
//             isMovementRestricted = true;
//         }



//         [Button]
//         public void SaveCameraPosition()
//         {
//             PlayerPrefs.SetFloat(PREFS_WIDTH_KEY, SceneView.lastActiveSceneView.size);
//         }

//         [Button]
//         public void SetUpCamera()
//         {
//             SceneView sceneView = SceneView.lastActiveSceneView;
//             sceneView.AlignViewToObject(mainCamera.transform);

//             if (PlayerPrefs.HasKey(PREFS_WIDTH_KEY))
//             {
//                 sceneView.size = PlayerPrefs.GetFloat(PREFS_WIDTH_KEY, 6f);
//             }
//             else
//             {
//                 sceneView.size = PlayerPrefs.GetFloat("CameraScalerWidth", 6f);
//             }


//             SceneView.RepaintAll();
//         }


//         public void LoadLevel(LevelData levelData, Action<Vector2Int, Vector2Int> handleBlockChange, Action<Vector2Int, Vector2Int[], BlockType> handleBlockSpawnCallback, Action<Vector2Int> handleBlockDelete, Action<Vector2Int, BlockColor> handleBlockColorChange, Action<Vector2Int, LevelFigure[], BlockType[], Action<object>> createWindow, Action<Vector2Int> handleCreateEffectEditor, Action<Vector2Int> handleUpdateEffectData)
//         {
//             if (!isInitiazed)
//             {
//                 ChainManager.Init();
//                 RopesManager.Init();
//                 levelDatabase.Init();
//                 LevelSkinData levelSkinData =  skinDatabase.GetSkinData(skinId) as LevelSkinData;
//                 ReflectionUtils.InjectStaticComponent<LevelController>("skinData", levelSkinData);
//                 ReflectionUtils.InjectStaticComponent<LevelController>("levelDatabase", levelDatabase);
//                 SceneView.duringSceneGui += DuringSceneGui;
//                 isInitiazed = true;
//             }

//             this.handleBlockChangeCallback = handleBlockChange;
//             this.handleBlockSpawnCallback = handleBlockSpawnCallback;
//             this.handleBlockDeleteCallback = handleBlockDelete;
//             this.handleBlockColorChangeCallback = handleBlockColorChange;
//             this.handleCreateFigureSelectionPopup = createWindow;
//             this.handleCreateEffectEditor = handleCreateEffectEditor;
//             this.handleUpdateEffectData = handleUpdateEffectData;
//             this.levelData = levelData;

//             if (levelRepresentation != null)
//             {
//                 DestroyImmediate(levelRepresentation.LevelTransform.gameObject);
//             }

//             //removing old level representations
//             GameObject[] rootGameObjects = this.gameObject.scene.GetRootGameObjects();

//             for (int i = 0; i < rootGameObjects.Length; i++)
//             {
//                 if (rootGameObjects[i].name.Equals("[LEVEL]"))
//                 {
//                     DestroyImmediate(rootGameObjects[i]);
//                 }
//             }

//             levelRepresentation = new LevelRepresentation(levelData, environmentData);
//             levelRepresentation.SpawnEnvironment();
//             levelRepresentation.SpawnInteractiveObjects();
//             levelRepresentation.SpawnBlocks();
//             SceneVisibilityManager.instance.DisablePicking(levelRepresentation.LevelTransform.gameObject, true);

//             PrepareGateData(levelData.Size);
//             PrepareForBlocksMovement();
//             PrepareForHandlesUI();
//         }



//         private void PrepareGateData(Vector2Int levelSize)
//         {
//             GateBehavior[] gateBehaviourBehaviors = levelRepresentation.LevelTransform.GetComponentsInChildren<GateBehavior>();
//             gateData = new List<GateData>();

//             for (int i = 0; i < gateBehaviourBehaviors.Length; i++)
//             {
//                 gateData.Add(new GateData(gateBehaviourBehaviors[i], levelSize));
//             }
//         }

//         private void PrepareForBlocksMovement()
//         {
//             LevelBlockBehavior[] levelBlockBehaviors = levelRepresentation.LevelTransform.GetComponentsInChildren<LevelBlockBehavior>();
//             selectedBlockMovementBehaviourIndex = -1;
//             selectedBlock = null;
//             blockMovementData = new List<BlockMovementData>();

//             for (int i = 0; i < levelBlockBehaviors.Length; i++)
//             {
//                 blockMovementData.Add(new BlockMovementData(levelBlockBehaviors[i], GetVector2IntPosition(levelBlockBehaviors[i].transform.position)));
//             }

//             movementManager = new BlockMovementManager();
//             movementManager.SetLevelRepresentation(levelRepresentation);

//             levelPreviewInitialized = true;
//         }

//         private void PrepareForHandlesUI()
//         {
//             blockColorNames = Enum.GetNames(typeof(BlockColor));
//             blockoColorValues = (BlockColor[])Enum.GetValues(typeof(BlockColor));
//             selectedBlockColorValue = -1;
//         }

//         public void SelectBlock(Vector2Int index)
//         {
//             for (int i = 0; i < blockMovementData.Count; i++)
//             {
//                 if(blockMovementData[i].recordedPosition + blockMovementData[i].pivotPoint == index)
//                 {
//                     selectedBlockMovementBehaviourIndex = i;
//                     selectedBlock = blockMovementData[i];
//                     movementManager.Enable(selectedBlock.levelBlock);
//                     handleCreateEffectEditor?.Invoke(selectedBlock.recordedPosition + selectedBlock.pivotPoint);
//                     return;
//                 }
//             }
//         }

//         private void DuringSceneGui(SceneView view)
//         {
//             if (!levelPreviewInitialized)
//             {
//                 return;
//             }

//             if (EditorSceneManager.GetActiveScene().name != "Level Editor")
//             {
//                 Unsubscribe();
//                 return;
//             }

//             if (selectedBlockMovementBehaviourIndex == (-1))
//             {
//                 backupHandlesColor = Handles.color;
//                 Handles.color = new Color(0, 0, 0, 0);
//                 DrawGateButtons();
//                 DrawBlockButtons();

//                 Handles.color = backupHandlesColor;
//             }
//             else
//             {
//                 HandleMouseEvents();
//                 DrawHandlesMenu();
//                 DrawFigureHandles();
//             }
//         }

//         private void HandleMouseEvents()
//         {
//             if ((Event.current.isMouse) && (Event.current.type == EventType.MouseUp))
//             {
//                 if (Event.current.button == 0)
//                 {
//                     Save();
//                 }
//                 else
//                 {
//                     Cancel();
//                 }
//             }
//         }

//         private void DrawGateButtons()
//         {
//             for (int i = 0; i < gateData.Count; i++)
//             {
//                 for (int k = 0; k < gateData[i].points.Length; k++)
//                 {
//                     if (Handles.Button(gateData[i].points[k] + handlesCubeOffset, Quaternion.identity, 1.05f, 1.05f, Handles.CubeHandleCap))
//                     {
//                         selectedGateIndex = i;
//                         List<LevelFigure> figures = new List<LevelFigure>();
//                         List<BlockType> types = new List<BlockType>();
//                         LevelFigure tempBlockFigure;

//                         for (int j = 0; j < blocksVisuals.Blocks.Length; j++)
//                         {
//                             tempBlockFigure = blocksVisuals.Blocks[j].Prefab.GetComponent<LevelBlockBehavior>().Figure;

//                             if (gateData[i].vertical)
//                             {
//                                 if (tempBlockFigure.Size.x <= gateData[i].points.Length)
//                                 {
//                                     figures.Add(tempBlockFigure);
//                                     types.Add(blocksVisuals.Blocks[j].Type);
//                                 }
//                             }
//                             else
//                             {
//                                 if (tempBlockFigure.Size.y <= gateData[i].points.Length)
//                                 {
//                                     figures.Add(tempBlockFigure);
//                                     types.Add(blocksVisuals.Blocks[j].Type);
//                                 }
//                             }
//                         }

//                         handleCreateFigureSelectionPopup?.Invoke(gateData[selectedGateIndex].blockPosition, figures.ToArray(), types.ToArray(), SpawnBlockMenu);
//                         Handles.color = backupHandlesColor;
//                         return;
//                     }
//                 }
//             }
//         }

//         private void SpawnBlockMenu(object userData)
//         {
//             BlockType blockType = (BlockType)userData;
//             BlockData blockData = null;

//             for (int i = 0; i < blocksVisuals.Blocks.Length; i++)
//             {
//                 if (blocksVisuals.Blocks[i].Type == blockType)
//                 {
//                     blockData = blocksVisuals.Blocks[i];
//                     break;
//                 }
//             }

//             LevelFigure figure = blockData.Prefab.GetComponent<LevelBlockBehavior>().Figure;
//             Vector2Int pos1 = gateData[selectedGateIndex].blockSpawnPosition;
//             Vector2Int pos2 = gateData[selectedGateIndex].blockAlternativeSpawnPosition;
//             List<Vector2Int> result = new List<Vector2Int>();
//             int offset = 0;


//             if (gateData[selectedGateIndex].vertical)
//             {
//                 pos1 = pos1.AddToY(-(figure.Size.y - 1));
//                 pos1 += figure.PivotPoint;
//                 pos2 += figure.PivotPoint;
//                 offset = gateData[selectedGateIndex].gateSize - figure.Size.x;
//                 result.Add(pos1);
//                 result.Add(pos2);

//                 if (offset > 0)
//                 {
//                     for (int i = 1; i <= offset; i++)
//                     {
//                         result.Add(pos1.AddToX(i));
//                         result.Add(pos2.AddToX(i));
//                     }
//                 }
//             }
//             else
//             {
//                 pos1 = pos1.AddToX(-(figure.Size.x - 1));
//                 pos1 += figure.PivotPoint;
//                 pos2 += figure.PivotPoint;
//                 offset = gateData[selectedGateIndex].gateSize - figure.Size.y;
//                 result.Add(pos1);
//                 result.Add(pos2);

//                 if (offset > 0)
//                 {
//                     for (int i = 1; i <= offset; i++)
//                     {
//                         result.Add(pos1.AddToY(i));
//                         result.Add(pos2.AddToY(i));
//                     }
//                 }
//             }

//             handleBlockSpawnCallback?.Invoke(gateData[selectedGateIndex].blockPosition, result.ToArray(),  blockType);
//         }

//         private void DrawBlockButtons()
//         {
//             for (int i = 0; i < blockMovementData.Count; i++)
//             {
//                 for (int j = 0; j < blockMovementData[i].points.Length; j++)
//                 {
//                     if (Handles.Button(blockMovementData[i].position + blockMovementData[i].points[j] + handlesCubeOffset, Quaternion.identity, 1.05f, 1.05f, Handles.CubeHandleCap))
//                     {
//                         selectedBlockMovementBehaviourIndex = i;
//                         selectedBlock = blockMovementData[i];

//                         movementManager.Enable(selectedBlock.levelBlock);
//                         handleCreateEffectEditor?.Invoke(selectedBlock.recordedPosition + selectedBlock.pivotPoint);
//                         Handles.color = backupHandlesColor;
//                         return;
//                     }
//                 }
//             }
//         }


//         private void DrawHandlesMenu()
//         {
//             Handles.BeginGUI();

//             GUILayout.BeginArea(new Rect(100, 10, 300, 134 + inspectorHeight), "Handles menu", GUI.skin.window);
//             GUILayout.Label("Selected #" + (selectedBlockMovementBehaviourIndex + 1));

//             if (GUILayout.Button("Cancel selection"))
//             {
//                 Cancel();
//             }

//             if(GUILayout.Button("Delete block"))
//             {
//                 Vector2Int pos = selectedBlock.recordedPosition + selectedBlock.pivotPoint;
//                 Cancel();
//                 handleBlockDeleteCallback?.Invoke(pos);
//             }

//             selectedBlockColorValue = EditorGUILayout.Popup("Color change:", selectedBlockColorValue, blockColorNames);

//             if(selectedBlockColorValue != -1)
//             {
//                 Vector2Int pos = selectedBlock.recordedPosition + selectedBlock.pivotPoint;
//                 BlockColor blockColor = blockoColorValues[selectedBlockColorValue];
//                 selectedBlockColorValue = -1;
//                 Cancel();
//                 handleBlockColorChangeCallback?.Invoke(pos, blockColor);
//             }

//             isMovementRestricted = EditorGUILayout.ToggleLeft("Is movement restricted", isMovementRestricted);

//             EditorGUI.BeginChangeCheck();
//             EditorGUILayout.BeginVertical();
//             handlesEditor.OnInspectorGUI();
//             EditorGUILayout.EndVertical();

//             if(Event.current.type == EventType.Repaint)
//             {
//                 inspectorHeight =  Mathf.CeilToInt(GUILayoutUtility.GetLastRect().height);
//             }

//             if (EditorGUI.EndChangeCheck())
//             {
//                 handleUpdateEffectData?.Invoke(selectedBlock.recordedPosition + selectedBlock.pivotPoint);
//             }

//             GUILayout.EndArea();
//             Handles.EndGUI();
//         }

//         private void Cancel()
//         {
//             selectedBlock.currentPosition = selectedBlock.recordedPosition;
//             selectedBlock.UpdatePosition();
//             movementManager.ReleaseObject();
//             selectedBlock = null;
//             selectedBlockMovementBehaviourIndex = -1;
//         }

//         private void Save()
//         {
//             if (isMovementRestricted)
//             {
//                 movementManager.ReleaseObject();
//                 selectedBlock.currentPosition = GetVector2IntPosition(selectedBlock.levelBlock.transform.position);
//                 selectedBlock.UpdatePosition();
//                 movementManager.Enable(selectedBlock.levelBlock);
//                 handleBlockChangeCallback?.Invoke(selectedBlock.recordedPosition + selectedBlock.pivotPoint, selectedBlock.currentPosition + selectedBlock.pivotPoint);
//                 selectedBlock.recordedPosition = selectedBlock.currentPosition;
//             }
//             else
//             {
//                 selectedBlock.currentPosition = GetVector2IntPosition(selectedBlock.levelBlock.transform.position);
//                 selectedBlock.UpdatePosition();
//                 Vector2Int cur = selectedBlock.currentPosition;
//                 Vector2Int size = levelData.Size;

//                 if((cur.x >= 0) && (cur.y >= 0) && (cur.x < size.x) && (cur.y < size.y))
//                 {
//                     handleBlockChangeCallback?.Invoke(selectedBlock.recordedPosition + selectedBlock.pivotPoint, selectedBlock.currentPosition + selectedBlock.pivotPoint);
//                     selectedBlock.recordedPosition = selectedBlock.currentPosition;
//                 }
//             }
//         }

//         private void DrawFigureHandles()
//         {
//             //handle cancel selection button
//             if(selectedBlock == null)
//             {
//                 return;
//             }

//             for (int i = 0; i < selectedBlock.points.Length; i++)
//             {
//                 Vector3 newPosition = Handles.Slider2D(
//                     selectedBlock.levelBlock.transform.position + selectedBlock.points[i] + Vector3.up,
//                     Vector3.up,
//                     Vector3.right,
//                     Vector3.forward,
//                     0.5f,
//                     Handles.RectangleHandleCap,
//                     0f);

//                 if(!Mathf.Approximately(Vector3.Distance(selectedBlock.levelBlock.transform.position + selectedBlock.points[i] + Vector3.up,newPosition), 0))
//                 {
//                     if (isMovementRestricted)
//                     {
//                         movementManager.MoveToPosition(newPosition - selectedBlock.points[i]);
//                     }
//                     else
//                     {
//                         selectedBlock.levelBlock.transform.position = newPosition - selectedBlock.points[i];
//                     }

//                 }
//             }
//         }

//         private void OnDestroy()
//         {
//             Unsubscribe();
//         }

//         public void Unsubscribe()
//         {
//             isInitiazed = false;
//             levelPreviewInitialized = false;
//             SceneView.duringSceneGui -= DuringSceneGui;
//         }



//         public void SelectGameObject(GameObject selectedGameObject)
//         {
//             Selection.activeGameObject = selectedGameObject;
//         }

//         public void Clear()
//         {
//             if(container == null)
//             {
//                 return;
//             }

//             for (int i = container.transform.childCount - 1; i >= 0; i--)
//             {
//                 DestroyImmediate(container.transform.GetChild(i).gameObject);
//             }
//         }

//         private Vector2Int GetVector2IntPosition(Vector3 position)
//         {
//             return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
//         }

//         private class GateData
//         {
//             public GateBehavior gate;
//             public bool vertical;
//             public Vector2Int blockSpawnPosition;
//             public Vector2Int blockAlternativeSpawnPosition;
//             public Vector2Int blockPosition;
//             public int gateSize;
//             public Vector3[] points;

//             public GateData(GateBehavior gate, Vector2Int levelSize)
//             {
//                 this.gate = gate;
//                 vertical = !Mathf.Approximately(gate.transform.rotation.eulerAngles.y, 0);
//                 blockPosition = new Vector2Int(Mathf.RoundToInt(gate.transform.position.x), Mathf.RoundToInt(gate.transform.position.z));
//                 CollectGatePoints();
//                 CollectSpawnPositions();
//             }

//             // in game max gate size is 3 but we add cases for 4 and 5 to show how to expand size if you need it
//             private void CollectGatePoints() 
//             {
//                 gateSize = gate.Data.UnifiedElements.Count;
//                 points = new Vector3[gateSize];

//                 if (gateSize == 1)
//                 {
//                     points[0] = gate.transform.position;
//                 }
//                 else
//                 {
//                     if (vertical)
//                     {
//                         if (gateSize % 2 == 0)
//                         {
//                             points[1] = gate.transform.position.AddToX(0.5f);
//                             points[0] = gate.transform.position.AddToX(-0.5f);

//                             if (gateSize < 4)
//                             {
//                                 return;
//                             }

//                             points[2] = gate.transform.position.AddToX(1.5f);
//                             points[3] = gate.transform.position.AddToX(-1.5f);
//                         }
//                         else
//                         {
//                             points[0] = gate.transform.position;
//                             points[1] = gate.transform.position.AddToX(1);
//                             points[2] = gate.transform.position.AddToX(-1);

//                             if(gateSize < 5)
//                             {
//                                 return;
//                             }

//                             points[3] = gate.transform.position.AddToX(2);
//                             points[4] = gate.transform.position.AddToX(-2);
//                         }
//                     }
//                     else
//                     {
//                         if (gateSize % 2 == 0)
//                         {
//                             points[0] = gate.transform.position.AddToZ(0.5f);
//                             points[1] = gate.transform.position.AddToZ(-0.5f);

//                             if (gateSize < 4)
//                             {
//                                 return;
//                             }

//                             points[2] = gate.transform.position.AddToZ(1.5f);
//                             points[3] = gate.transform.position.AddToZ(-1.5f);
//                         }
//                         else
//                         {
//                             points[0] = gate.transform.position;
//                             points[1] = gate.transform.position.AddToZ(1);
//                             points[2] = gate.transform.position.AddToZ(-1);

//                             if (gateSize < 5)
//                             {
//                                 return;
//                             }

//                             points[3] = gate.transform.position.AddToZ(2);
//                             points[4] = gate.transform.position.AddToZ(-2);
//                         }
//                     }
//                 }
//             }

//             private void CollectSpawnPositions()
//             {
//                 Array.Sort(points, Compare);

//                 if (vertical)
//                 {
//                     blockSpawnPosition = new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].z) - 1);
//                     blockAlternativeSpawnPosition = new Vector2Int(Mathf.RoundToInt(points[0].x), Mathf.RoundToInt(points[0].z) + 1);
//                 }
//                 else
//                 {
//                     blockSpawnPosition = new Vector2Int(Mathf.RoundToInt(points[0].x) - 1, Mathf.RoundToInt(points[0].z));
//                     blockAlternativeSpawnPosition = new Vector2Int(Mathf.RoundToInt(points[0].x) + 1, Mathf.RoundToInt(points[0].z));
//                 }
//             }

//             private int Compare(Vector3 first, Vector3 second)
//             {
//                 if(first.x.CompareTo(second.x) == 0)
//                 {
//                     return first.z.CompareTo(second.z);
//                 }
//                 else
//                 {
//                     return first.x.CompareTo(second.x);
//                 }
//             }
//         }

//         private class BlockMovementData
//         {
//             public LevelBlockBehavior levelBlock;
//             public Vector2Int recordedPosition;
//             public Vector2Int currentPosition;
//             public Vector2Int pivotPoint;
//             public Vector3[] points;

//             public Vector3 position => new Vector3(currentPosition.x, levelBlock.transform.position.y, currentPosition.y);


//             public BlockMovementData(LevelBlockBehavior levelBlock, Vector2Int recordedPosition)
//             {
//                 this.levelBlock = levelBlock;
//                 pivotPoint = levelBlock.Figure.PivotPoint;
//                 this.recordedPosition = recordedPosition;
//                 this.currentPosition = recordedPosition;

//                 List<Vector3> list = new List<Vector3>();
//                 int index = 0;

//                 for (int y = 0; y < levelBlock.Figure.Size.y; y++)
//                 {
//                     for (int x = 0; x < levelBlock.Figure.Size.x; x++)
//                     {
//                         if (levelBlock.Figure.Points[index].IsFilled)
//                         {
//                             list.Add(new Vector3(x, 0, y));
//                         }

//                         index++;
//                     }
//                 }

//                 points = list.ToArray();
//             }

//             public void UpdatePosition()
//             {
//                 levelBlock.transform.position = new Vector3(currentPosition.x, 0, currentPosition.y);
//             }
//         }


// #endif
//         }
//     }

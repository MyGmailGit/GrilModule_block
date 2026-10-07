// #pragma warning disable 649

// using UnityEngine;
// using UnityEditor;
// using System;
// using System.Collections.Generic;
// using UnityEditor.SceneManagement;
// // using static Watermelon.LevelElementData;


// namespace Watermelon
// {
//     public class LevelEditorWindow : LevelEditorBase
//     {

//         private const string GAME_SCENE_PATH = "Assets/GamePlay/PlayAsset/Scenes/Game.unity";
//         private const string EDITOR_SCENE_PATH = "Assets/GamePlay/PlayAsset/Scenes/Level Editor.unity";
//         private static string EDITOR_SCENE_NAME = "Level Editor";

//         //used variables
//         private const string LEVELS_PROPERTY_NAME = "levels";
//         private const string CELLS_PROPERTY_NAME = "cells";
//         private const string EDITOR_COLORS_DATA_PROPERTY_NAME = "editorColorData";
//         private const string TYPE_PROPERTY_NAME = "type";
//         private const string COLOR_PROPERTY_NAME = "color";
//         private const string TEXTURE_PROPERTY_NAME = "texture";

//         private SerializedProperty levelsSerializedProperty;
//         private SerializedProperty cellsSerializedProperty;
//         private SerializedProperty editorColorsDataSerializedProperty;
//         private LevelRepresentation selectedLevelRepresentation;
//         private bool needUpdateLevelPreview;
//         private LevelsHandler levelsHandler;
//         private CellTypesHandler cellTypeHandler;
//         private CellTypesHandler gateColorHandler;

//         //sidebar
//         private const int SIDEBAR_WIDTH = 320;
//         //PlayerPrefs
//         private const string PREFS_LEVEL = "editor_level_index";
//         private const string PREFS_WIDTH = "editor_sidebar_width";
//         private const string PREFS_MIN_ELEMENT_SIZE = "editor_min_grid_cell_size";

//         //instructions
//         private const string LEVEL_INSTRUCTION = "Draw environment using buttons on the bottom left. Left click on canvas to draw.";
//         private const string RIGHT_CLICK_INSTRUCTION = "Spawn block by clickin on the gate in Scene view. Right click to deselect the block.";
//         private const int INFO_HEIGH = 122; //found out using Debug.Log(infoRect) on worst case scenario
//         private const string LEVEL_PASSED_VALIDATION = "Level passed validation.";
//         private const string OPEN_GAME_SCENE_LABEL = "Open \"Game\" scene";
//         private const string LOAD_LEVEL_BY_HASH = "Load level by hash";
//         private const string TEST_LEVEL = "Test Level";
//         private Rect infoRect;

//         //level drawing
//         private Rect drawRect;
//         private float xSize;
//         private float ySize;
//         private float elementSize;
//         private Event currentEvent;
//         private Vector2 elementUnderMouseIndex;
//         private Vector2Int elementPosition;
//         private int invertedY;
//         private float buttonRectX;
//         private float buttonRectY;
//         private Rect buttonRect;
//         private BlockEffectType tempBlockEffect;
//         private readonly Color GRID_COLOR = new Color(0.4f, 0.4f, 0.4f);

//         private Rect separatorRect;
//         private bool separatorIsDragged;
//         private int currentSideBarWidth;
//         private bool lastActiveLevelOpened;
//         private List<Vector2Int> positions;
//         private SerializedProperty tempCellProperty;
//         private Vector2Int tempCellPosition;
//         private TabHandler tabHandler;
//         private Texture2D tempTexture;

//         //cellTypeButton
//         private ElementType[] cellTypeButtons = { ElementType.Empty, ElementType.InnerTile, ElementType.Border, ElementType.Obstacle, ElementType.InteractableObject, ElementType.Gate };
//         private GUIStyle cellTypeLabelStyle;
//         private Rect cellTypeButtonsDrawRect;
//         private int cellTypeButtonWidth;
//         private int cellTypeButtonTextHeight;
//         private int cellTypeButtonOffset;
//         private int buttonsPerRow;
//         private int rows;
//         private float currentX;
//         private float currentY;
//         private Rect labelRect;
//         private Rect textureRect;
//         private List<FigureRepresentation> figureRepresentations;
//         private Dictionary<Vector2Int, int> figuresDictionary;
//         private Rect lineRect;
//         private Vector2Int selectedBlockPosition;
//         private SerializedProperty selectedBlockProperty;
//         private List<SerializedProperty> selectedGateNeighbours;
//         private bool isBlockSelected;
//         private bool isSelectedBlockGate;
//         private int minGridCellSize;
//         private MonoBehaviorInspector selectedBlockCustomInspector;
//         private bool needToSelectLevelBlock;
//         private Vector2Int levelBlockPosition;
//         private bool tempDrawBlockEffectLabel;
//         private string tempBlockEffectLabel;
//         private MonoBehaviorInspector effectEditor;
//         private Vector2Int effectEditorPosition;
//         private string selectedBlockLabel;

//         public bool IsBlockSelected
//         {
//             get => isBlockSelected; set
//             {
//                 isBlockSelected = value;
//                 HandleSelectedBlockEditor();
//             }
//         }



//         private void HandleSelectedBlockEditor()
//         {
//             if (IsBlockSelected)
//             {
//                 if (selectedBlockCustomInspector != null)
//                 {
//                     DestroyImmediate(selectedBlockCustomInspector);
//                 }

//                 EditorSceneController.Instance.SelectedBlockEditor.Data = (LevelElementData)selectedBlockProperty.boxedValue;
//                 selectedBlockCustomInspector = (MonoBehaviorInspector)Editor.CreateEditor(EditorSceneController.Instance.SelectedBlockEditor, typeof(MonoBehaviorInspector));
//             }
//             else
//             {
//                 selectedBlockPosition = new Vector2Int(-1, -1);

//                 if (selectedBlockCustomInspector != null)
//                 {
//                     DestroyImmediate(selectedBlockCustomInspector);
//                 }
//             }
//         }

//         protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
//         {
//             return builder.SetWindowMinSize(new Vector2(700, 500)).Build();
//         }

//         protected override Type GetLevelsDatabaseType()
//         {
//             return typeof(LevelDatabase);
//         }

//         public override Type GetLevelType()
//         {
//             return typeof(LevelData);
//         }

//         protected override void ReadLevelDatabaseFields()
//         {
//             levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
//             cellsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(CELLS_PROPERTY_NAME);
//             editorColorsDataSerializedProperty = levelsDatabaseSerializedObject.FindProperty(EDITOR_COLORS_DATA_PROPERTY_NAME);
//         }

//         protected override void InitializeVariables()
//         {
//             HandleCellsInitialization();
//             tabHandler = new TabHandler();
//             tabHandler.AddTab(new TabHandler.Tab("Levels", DisplayLevelsTab));
//             tabHandler.AddTab(new TabHandler.Tab("Editor", DisplayEditorTab));
//             currentSideBarWidth = PlayerPrefs.GetInt(PREFS_WIDTH, SIDEBAR_WIDTH);
//             positions = new List<Vector2Int>();
//             EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
//             AssemblyReloadEvents.beforeAssemblyReload += UnloadEditor;
//             minGridCellSize = PlayerPrefs.GetInt(PREFS_MIN_ELEMENT_SIZE, 18);
//             selectedGateNeighbours = new List<SerializedProperty>();
//         }

//         private void OnPlayModeStateChanged(PlayModeStateChange change)
//         {
//             if (EditorSceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
//             {
//                 return;
//             }

//             if (change != PlayModeStateChange.ExitingEditMode)
//             {
//                 return;
//             }

//             if (levelsHandler.SelectedLevelIndex == -1)
//             {
//                 OpenScene(GAME_SCENE_PATH);
//             }
//             else
//             {
//                 TestLevel();
//             }
//         }


//         private void HandleCellsInitialization()
//         {
//             cellTypeHandler = new CellTypesHandler();
//             gateColorHandler = new CellTypesHandler();

//             //some validation
//             string[] names = Enum.GetNames(typeof(ElementType));
//             cellsSerializedProperty.arraySize = names.Length;
//             Color color;

//             for (int i = 0; i < cellsSerializedProperty.arraySize; i++)
//             {
//                 cellsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_NAME).intValue = i;
//                 color = cellsSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLOR_PROPERTY_NAME).colorValue;
//                 cellTypeHandler.AddCellType(new CellTypesHandler.CellType(i, names[i], color));
//             }

//             cellTypeHandler.GetCellType((int)ElementType.Block).extraPropsEnabled = true;
//             cellTypeHandler.GetCellType((int)ElementType.InteractableObject).label = "Interractable";

//             //more validation
//             names = Enum.GetNames(typeof(BlockColor));
//             editorColorsDataSerializedProperty.arraySize = names.Length;

//             for (int i = 0; i < editorColorsDataSerializedProperty.arraySize; i++)
//             {
//                 editorColorsDataSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(TYPE_PROPERTY_NAME).intValue = i;
//                 color = editorColorsDataSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLOR_PROPERTY_NAME).colorValue;
//                 gateColorHandler.AddCellType(new CellTypesHandler.CellType(i, names[i], color));
//             }

//             names = Enum.GetNames(typeof(BlockType));
//             cellTypeHandler.AddExtraProp(new CellTypesHandler.ExtraProp(0, names[0], false));

//             for (int i = 1; i < names.Length; i++)
//             {
//                 cellTypeHandler.AddExtraProp(new CellTypesHandler.ExtraProp(i, names[i]));
//             }

//             BlocksVisualsData blocksVisualsData = EditorUtils.GetAsset<BlocksVisualsData>();
//             figureRepresentations = new List<FigureRepresentation>();
//             figuresDictionary = new Dictionary<Vector2Int, int>();

//             if (blocksVisualsData == null)
//             {
//                 Debug.LogError("BlocksVisualsData not found");
//                 return;
//             }

//             for (int i = 0; i < blocksVisualsData.Blocks.Length; i++)
//             {
//                 figureRepresentations.Add(new FigureRepresentation(blocksVisualsData.Blocks[i].Prefab.GetComponent<LevelBlockBehavior>().Figure));
//             }

//         }

//         private Texture2D GetTexture(int value)
//         {
//             return cellsSerializedProperty.GetArrayElementAtIndex(value).FindPropertyRelative(TEXTURE_PROPERTY_NAME).objectReferenceValue as Texture2D;
//         }

//         private void OpenLastActiveLevel()
//         {
//             if (!lastActiveLevelOpened)
//             {
//                 if ((levelsSerializedProperty.arraySize > 0) && PlayerPrefs.HasKey(PREFS_LEVEL))
//                 {
//                     int levelIndex = Mathf.Clamp(PlayerPrefs.GetInt(PREFS_LEVEL, 0), 0, levelsSerializedProperty.arraySize - 1);
//                     levelsHandler.CustomList.SelectedIndex = levelIndex;
//                     levelsHandler.OpenLevel(levelIndex);
//                 }

//                 lastActiveLevelOpened = true;
//             }
//         }


//         protected override void Styles()
//         {
//             if (cellTypeHandler != null)
//             {
//                 cellTypeHandler.SetDefaultLabelStyle();
//             }

//             if (gateColorHandler != null)
//             {
//                 gateColorHandler.SetDefaultLabelStyle();
//             }

//             if (tabHandler != null)
//             {
//                 tabHandler.SetDefaultToolbarStyle();
//             }

//             if (levelsDatabase != null)
//             {
//                 levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
//             }

//             cellTypeLabelStyle = new GUIStyle(EditorCustomStyles.labelSmallBold);
//             cellTypeLabelStyle.alignment = TextAnchor.MiddleCenter;


//         }

//         public override void OpenLevel(UnityEngine.Object levelObject, int index)
//         {
//             PlayerPrefs.SetInt(PREFS_LEVEL, index);
//             PlayerPrefs.Save();
//             AssetDatabase.SaveAssets();
//             IsBlockSelected = false;
//             selectedLevelRepresentation = new LevelRepresentation(levelObject);
//             needUpdateLevelPreview = true;
//             EditorSceneController.Instance.SetUpCamera();
//         }

//         public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
//         {
//             return new LevelRepresentation(levelObject).GetLevelLabel(index, stringBuilder);
//         }

//         public override void ClearLevel(UnityEngine.Object levelObject)
//         {
//             new LevelRepresentation(levelObject).Clear();
//         }
//         public override void LogErrorsForGlobalValidation(UnityEngine.Object levelObject, int index)
//         {
//             LevelRepresentation level = new LevelRepresentation(levelObject);
//             level.ValidateLevel();

//             if (!level.IsLevelCorrect)
//             {
//                 Debug.Log("Logging validation errors for level #" + (index + 1) + " :");

//                 foreach (string error in level.errorLabels)
//                 {
//                     Debug.LogWarning(error);
//                 }
//             }
//             else
//             {
//                 Debug.Log($"Level # {(index + 1)} passed validation.");
//             }
//         }

//         protected override void DrawContent()
//         {
//             if (EditorSceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
//             {
//                 DrawOpenEditorScene();
//                 return;
//             }

//             tabHandler.DisplayTab();
//         }

//         private void DrawOpenEditorScene()
//         {
//             EditorGUILayout.BeginVertical();
//             EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

//             if (GUILayout.Button("Open \"" + EDITOR_SCENE_NAME + "\" scene"))
//             {
//                 OpenScene(EDITOR_SCENE_PATH);
//             }

//             EditorGUILayout.EndVertical();
//         }

//         private void DisplayLevelsTab()
//         {
//             EditorGUILayout.BeginVertical();
//             EditorGUILayout.Space();
//             EditorGUILayout.BeginHorizontal();
//             DisplayListArea();
//             HandleChangingSideBar();
//             DisplayMainArea();
//             EditorGUILayout.EndHorizontal();
//             EditorGUILayout.Space();
//             EditorGUILayout.EndVertical();
//         }

//         private void HandleChangingSideBar()
//         {
//             separatorRect = EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(0), GUILayout.ExpandHeight(true));
//             EditorGUILayout.EndHorizontal();
//             separatorRect.xMin -= GUI.skin.box.margin.right;
//             separatorRect.xMax += GUI.skin.box.margin.left;
//             EditorGUIUtility.AddCursorRect(separatorRect, MouseCursor.ResizeHorizontal);


//             if (separatorRect.Contains(Event.current.mousePosition))
//             {
//                 if (Event.current.type == EventType.MouseDown)
//                 {
//                     separatorIsDragged = true;
//                     levelsHandler.IgnoreDragEvents = true;
//                     Event.current.Use();
//                 }
//             }

//             if (separatorIsDragged)
//             {
//                 if (Event.current.type == EventType.MouseUp)
//                 {
//                     separatorIsDragged = false;
//                     levelsHandler.IgnoreDragEvents = false;
//                     PlayerPrefs.SetInt(PREFS_WIDTH, currentSideBarWidth);
//                     PlayerPrefs.Save();
//                     Event.current.Use();
//                 }
//                 else if (Event.current.type == EventType.MouseDrag)
//                 {
//                     currentSideBarWidth = Mathf.RoundToInt(Event.current.delta.x) + currentSideBarWidth;
//                     Event.current.Use();
//                 }
//             }
//         }

//         private void DisplayListArea()
//         {
//             OpenLastActiveLevel();
//             EditorGUILayout.BeginVertical(GUILayout.Width(currentSideBarWidth));
//             levelsHandler.DisplayReordableList();
//             levelsHandler.DrawRenameLevelsButton();

//             if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, EditorCustomStyles.button))
//             {
//                 UnloadEditor();
//                 OpenScene(GAME_SCENE_PATH);
//             }

//             if (IsBlockSelected)
//             {
//                 DrawSelectedBlock();
//             }
//             else
//             {
//                 DrawButtons();
//                 DrawGateButtons();
//             }

//             EditorGUILayout.EndVertical();
//         }

//         private void LoadLevelByHash()
//         {
//             EditorApplication.delayCall -= LoadLevelByHash;
//             LoadByHashModalWindow popupWindow = EditorWindow.GetWindow<LoadByHashModalWindow>();
//             popupWindow.SetData(this);
//             popupWindow.minSize = new Vector2(300, EditorGUIUtility.singleLineHeight * 2);
//             popupWindow.maxSize = new Vector2(Screen.width, EditorGUIUtility.singleLineHeight * 2);
//             popupWindow.titleContent = new GUIContent("Load level by hash");
//             popupWindow.ShowModalUtility();
//         }

//         private void UnloadEditor()
//         {
//             selectedLevelRepresentation = null;
//             levelsHandler.ClearSelection();
//             lastActiveLevelOpened = false;
//             EditorSceneController.Instance.Unsubscribe();
//             IsBlockSelected = false;
//         }

//         private void DrawSelectedBlock()
//         {
//             EditorGUILayout.BeginHorizontal();

//             if (GUILayout.Button("X"))
//             {
//                 IsBlockSelected = false;
//                 EditorGUILayout.EndHorizontal();
//                 return;
//             }

//             EditorGUILayout.LabelField(selectedBlockLabel);

//             EditorGUILayout.EndHorizontal();

//             EditorGUI.BeginChangeCheck();
//             SerializedProperty iterator = selectedBlockProperty.Copy();
//             selectedBlockCustomInspector.OnInspectorGUI();
//             selectedBlockProperty.boxedValue = EditorSceneController.Instance.SelectedBlockEditor.Data;
//             //copy values to neighbours
//             if (isSelectedBlockGate && (selectedGateNeighbours.Count > 0))
//             {
//                 for (int i = 0; i < selectedGateNeighbours.Count; i++)
//                 {
//                     iterator = selectedBlockProperty.Copy();
//                     SerializedProperty iterator2 = selectedGateNeighbours[i].Copy();
//                     iterator.NextVisible(true);
//                     iterator2.NextVisible(true);

//                     do
//                     {
//                         if ((iterator.name != TYPE_PROPERTY_NAME) && (iterator.name != LevelRepresentation.POSITION_PROPERTY_NAME))
//                         {
//                             try
//                             {
//                                 if (iterator.isArray)
//                                 {
//                                     iterator2.arraySize = iterator.arraySize;

//                                     for (int j = 0; j < iterator.arraySize; j++)
//                                     {
//                                         iterator2.GetArrayElementAtIndex(j).boxedValue = iterator.GetArrayElementAtIndex(j).boxedValue;
//                                     }
//                                 }
//                                 else
//                                 {
//                                     iterator2.boxedValue = iterator.boxedValue;
//                                 }
//                             }
//                             catch
//                             {
//                             }
//                         }

//                         iterator2.NextVisible(false);

//                     } while (iterator.NextVisible(false) && iterator.propertyPath.Contains(selectedBlockProperty.propertyPath));

//                 }
//             }

//             if (EditorGUI.EndChangeCheck())
//             {
//                 needUpdateLevelPreview = true;
//             }
//         }

//         private void DrawButtons()
//         {
//             cellTypeButtonsDrawRect = EditorGUILayout.BeginVertical();
//             cellTypeButtonWidth = 64;
//             cellTypeButtonTextHeight = 14;
//             cellTypeButtonOffset = 8;
//             buttonsPerRow = Mathf.FloorToInt(currentSideBarWidth / ((cellTypeButtonWidth + cellTypeButtonOffset) * 1f));
//             rows = Mathf.CeilToInt(cellTypeButtons.Length / (buttonsPerRow * 1f));
//             currentX = cellTypeButtonsDrawRect.x;
//             currentY = cellTypeButtonsDrawRect.y;
//             GUILayout.Space(rows * (cellTypeButtonWidth + cellTypeButtonTextHeight + cellTypeButtonOffset));


//             for (int i = 0; i < cellTypeButtons.Length; i++)
//             {
//                 if (currentX + cellTypeButtonWidth + cellTypeButtonOffset > cellTypeButtonsDrawRect.x + currentSideBarWidth)
//                 {
//                     currentX = cellTypeButtonsDrawRect.x;
//                     currentY += cellTypeButtonWidth + cellTypeButtonTextHeight + cellTypeButtonOffset;

//                 }

//                 buttonRect = new Rect(currentX, currentY, cellTypeButtonWidth, cellTypeButtonWidth + cellTypeButtonTextHeight);
//                 currentX += cellTypeButtonOffset + cellTypeButtonWidth;

//                 CellTypesHandler.CellType cellType = cellTypeHandler.GetCellType((int)cellTypeButtons[i]);
//                 Texture texture = GetTexture(cellType.value);


//                 if (cellType.value == cellTypeHandler.selectedCellTypeValue)
//                 {
//                     GUI.DrawTexture(buttonRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.white, 2, 0);
//                 }
//                 else
//                 {
//                     GUI.DrawTexture(buttonRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.gray, 2, 0);
//                 }

//                 labelRect = new Rect(buttonRect);
//                 labelRect.yMin = labelRect.yMax - cellTypeButtonTextHeight - 2f;

//                 if (texture != null)
//                 {
//                     textureRect = new Rect(buttonRect);
//                     textureRect.yMax -= cellTypeButtonTextHeight - 2f;
//                     textureRect.xMax -= 2f;
//                     textureRect.xMin += 2f;
//                     textureRect.yMin += 2f;
//                     GUI.DrawTexture(textureRect, texture);
//                 }

//                 GUI.Label(labelRect, cellType.label, cellTypeLabelStyle);

//                 if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
//                 {
//                     cellTypeHandler.selectedCellTypeValue = cellType.value;
//                 }
//             }

//             EditorGUILayout.EndVertical();
//         }

//         private void DrawGateButtons()
//         {
//             if (cellTypeHandler.selectedCellTypeValue != (int)ElementType.Gate)
//             {
//                 return;
//             }

//             BlockColor[] colors = (BlockColor[])Enum.GetValues(typeof(BlockColor));
//             cellTypeButtonsDrawRect = EditorGUILayout.BeginVertical();
//             cellTypeButtonWidth = 24;
//             cellTypeButtonOffset = 6;
//             buttonsPerRow = Mathf.FloorToInt(currentSideBarWidth / ((cellTypeButtonWidth + cellTypeButtonOffset) * 1f));
//             rows = Mathf.CeilToInt(colors.Length / (buttonsPerRow * 1f));
//             currentX = cellTypeButtonsDrawRect.x;
//             currentY = cellTypeButtonsDrawRect.y;
//             GUILayout.Space(rows * (cellTypeButtonWidth + cellTypeButtonOffset));

//             for (int i = 0; i < colors.Length; i++)
//             {
//                 if (currentX + cellTypeButtonWidth + cellTypeButtonOffset > cellTypeButtonsDrawRect.x + currentSideBarWidth)
//                 {
//                     currentX = cellTypeButtonsDrawRect.x;
//                     currentY += cellTypeButtonWidth + cellTypeButtonOffset;

//                 }

//                 buttonRect = new Rect(currentX, currentY, cellTypeButtonWidth, cellTypeButtonWidth);
//                 currentX += cellTypeButtonOffset + cellTypeButtonWidth;


//                 CellTypesHandler.CellType cellType = gateColorHandler.GetCellType((int)colors[i]);
//                 DrawColorRect(buttonRect, cellType.color);

//                 if (cellType.value == gateColorHandler.selectedCellTypeValue)
//                 {
//                     GUI.DrawTexture(buttonRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.white, 2, 0);
//                 }
//                 else
//                 {
//                     GUI.DrawTexture(buttonRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.gray, 2, 0);
//                 }

//                 if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
//                 {
//                     cellTypeHandler.selectedCellTypeValue = cellType.value;
//                     gateColorHandler.selectedCellTypeValue = i;
//                     cellTypeHandler.selectedCellTypeValue = (int)ElementType.Gate;
//                 }
//             }

//             EditorGUILayout.EndVertical();
//         }

//         private void DisplayMainArea()
//         {

//             if (levelsHandler.SelectedLevelIndex == -1)
//             {
//                 return;
//             }

//             EditorGUILayout.BeginVertical(GUI.skin.box);

//             if (IsPropertyChanged(levelsHandler.SelectedLevelProperty, new GUIContent("File")))
//             {
//                 IsBlockSelected = false;
//                 levelsHandler.ReopenLevel();
//             }

//             if (selectedLevelRepresentation.NullLevel)
//             {
//                 EditorGUILayout.EndVertical();
//                 return;
//             }


//             DisplayLevelSettings();

//             if (IsPropertyChanged(selectedLevelRepresentation.sizeProperty))
//             {
//                 IsBlockSelected = false;
//                 selectedLevelRepresentation.HandleSizePropertyChange();
//                 needUpdateLevelPreview = true;
//             }

//             DrawLevel();

//             levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
//             selectedLevelRepresentation.ApplyChanges();


//             if (needUpdateLevelPreview)
//             {
//                 needUpdateLevelPreview = false;
//                 LoadLevelPreview();
//             }

//             DrawTipsAndWarnings();

//             EditorGUILayout.BeginHorizontal();

//             if (GUILayout.Button(LOAD_LEVEL_BY_HASH, EditorCustomStyles.button, GUILayout.Height(30f)))
//             {
//                 EditorApplication.delayCall += LoadLevelByHash;
//             }

//             GUILayout.FlexibleSpace();

//             if (GUILayout.Button("Get Level Hash", GUILayout.Width(EditorGUIUtility.labelWidth), GUILayout.Height(30f)))
//             {
//                 LevelData levelData = levelsHandler.SelectedLevelProperty.objectReferenceValue as LevelData;
//                 if (levelData != null)
//                 {
//                     Debug.Log("Level Hash copied to clipboard");

//                     EditorGUIUtility.systemCopyBuffer = levelData.GetCompressedLevelString();
//                 }
//             }

//             if (GUILayout.Button(TEST_LEVEL, GUILayout.Width(EditorGUIUtility.labelWidth), GUILayout.Height(30f)))
//             {
//                 TestLevel();
//             }

//             EditorGUILayout.EndHorizontal();

//             EditorGUILayout.EndVertical();
//         }

//         private void DisplayLevelSettings()
//         {
//             selectedLevelRepresentation.DisplayProperties();
//             selectedLevelRepresentation.ApplyChanges();
//         }


//         private void TestLevel()
//         {
//             EditorSceneController.Instance.Unsubscribe();
//             ActiveSession.SetEditorLevelIndex(levelsHandler.SelectedLevelIndex);
//             UnloadEditor();
//             OpenScene(GAME_SCENE_PATH);
//             EditorApplication.isPlaying = true;

//         }

//         private void LoadLevelPreview()
//         {
//             LevelData levelData = levelsHandler.SelectedLevelProperty.objectReferenceValue as LevelData;
//             EditorSceneController.Instance.LoadLevel(levelData, HandleBlockChange, HandleBlockSpawn, HandleBlockDelete, HandleBlockColorChange, HandleDisplayWindow, HandleCreateEffectEditor, HandleUpdateEffectData);

//             if (needToSelectLevelBlock)
//             {
//                 needToSelectLevelBlock = false;
//                 EditorSceneController.Instance.SelectBlock(levelBlockPosition);
//             }
//         }

//         public void HandleCreateEffectEditor(Vector2Int oldPosition)
//         {
//             if (effectEditorPosition == oldPosition)
//             {
//                 return;
//             }

//             if (effectEditor != null)
//             {
//                 DestroyImmediate(effectEditor);
//             }

//             effectEditorPosition = oldPosition;
//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(oldPosition.x, oldPosition.y));
//             SerializedProperty effectsArrayProperty = oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME);
//             BlockEffectData[] data = new BlockEffectData[effectsArrayProperty.arraySize];

//             for (int i = 0; i < data.Length; i++)
//             {
//                 data[i] = (BlockEffectData)effectsArrayProperty.GetArrayElementAtIndex(i).boxedValue;
//             }

//             EditorSceneController.Instance.EffectEditor.data = data;
//             effectEditor = (MonoBehaviorInspector)Editor.CreateEditor(EditorSceneController.Instance.EffectEditor, typeof(MonoBehaviorInspector));
//             effectEditor.SetScriptFieldState(false);
//             EditorSceneController.Instance.HandlesEditor = effectEditor;
//         }

//         public void HandleUpdateEffectData(Vector2Int newPosition)
//         {
//             effectEditorPosition = newPosition;
//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(newPosition.x, newPosition.y));
//             SerializedProperty effectsArrayProperty = oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME);
//             effectsArrayProperty.arraySize = EditorSceneController.Instance.EffectEditor.data.Length;

//             for (int i = 0; i < effectsArrayProperty.arraySize; i++)
//             {
//                 effectsArrayProperty.GetArrayElementAtIndex(i).boxedValue = EditorSceneController.Instance.EffectEditor.data[i];
//             }

//             selectedLevelRepresentation.ApplyChanges();
//             needUpdateLevelPreview = true;
//             levelBlockPosition = newPosition;
//             needToSelectLevelBlock = true;
//             Repaint();
//         }


//         public void HandleDisplayWindow(Vector2Int oldPosition, LevelFigure[] figures, BlockType[] types, Action<object> action)
//         {
//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(oldPosition.x, oldPosition.y));
//             CellTypesHandler.CellType cellType = gateColorHandler.GetCellType(oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue);
//             FigureSelectorWindow.CreateWindow(figures, types, cellType.color, action);
//         }

//         public void HandleBlockColorChange(Vector2Int oldPosition, BlockColor blockColor)
//         {
//             if (selectedBlockPosition == oldPosition)
//             {
//                 IsBlockSelected = false;
//                 Debug.LogWarning("Block unselected to prevent data corruption.");
//             }

//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(oldPosition.x, oldPosition.y));
//             oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue = (int)blockColor;
//             selectedLevelRepresentation.ApplyChanges();
//             needUpdateLevelPreview = true;
//             levelBlockPosition = oldPosition;
//             needToSelectLevelBlock = true;
//             Repaint();
//         }

//         public void HandleBlockChange(Vector2Int oldPosition, Vector2Int newPosition)
//         {
//             if (oldPosition == newPosition)
//             {
//                 return;
//             }

//             if (selectedBlockPosition == oldPosition)
//             {
//                 IsBlockSelected = false;
//                 Debug.LogWarning("Block unselected to prevent data corruption.");
//             }

//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(oldPosition.x, oldPosition.y));
//             SerializedProperty newProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(newPosition.x, newPosition.y));

//             if (newProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue != (int)ElementType.InnerTile)
//             {
//                 Debug.LogWarning("Can place block only on inner tile.");
//                 needUpdateLevelPreview = true;
//                 levelBlockPosition = oldPosition;
//                 needToSelectLevelBlock = true;
//                 Repaint();
//                 return;
//             }


//             newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_TYPE_PROPERTY_NAME).intValue = oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_TYPE_PROPERTY_NAME).intValue;
//             newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue = oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue;
//             newProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue = oldProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue;
//             selectedLevelRepresentation.CopyEffects(oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME), newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME));
//             oldProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue = (int)ElementType.InnerTile;
//             oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME).arraySize = 0;
//             selectedLevelRepresentation.ApplyChanges();
//             Repaint();
//         }

//         public void HandleBlockSpawn(Vector2Int oldPosition, Vector2Int[] newPositions, BlockType type)
//         {
//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(oldPosition.x, oldPosition.y));
//             bool found = false;
//             int index;
//             Vector2Int newPosition = Vector2Int.zero;

//             SerializedProperty newProperty = null;

//             for (int i = 0; i < newPositions.Length; i++)
//             {
//                 newPosition = newPositions[i];
//                 index = selectedLevelRepresentation.GetIndex(newPositions[i].x, newPosition.y);

//                 if (index == -1)
//                 {
//                     continue;
//                 }

//                 newProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(index);

//                 if (newProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue != (int)ElementType.InnerTile)
//                 {
//                     continue;
//                 }

//                 found = true;
//                 break;
//             }

//             if (!found)
//             {
//                 Debug.LogWarning("Index from 'newPositions' is not inner tile or outside field. Block not spawned.");
//                 return;
//             }

//             newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_TYPE_PROPERTY_NAME).intValue = (int)type;
//             newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue = oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue;
//             newProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue = (int)ElementType.Block;
//             newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME).arraySize = 0;

//             for (int i = 0; i < oldProperty.FindPropertyRelative(LevelRepresentation.GATE_EFFECTS_PROPERTY_NAME).arraySize; i++)
//             {
//                 if (oldProperty.FindPropertyRelative(LevelRepresentation.GATE_EFFECTS_PROPERTY_NAME).GetArrayElementAtIndex(i).FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue == (int)GateEffectType.Stars)
//                 {
//                     newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME).arraySize++;
//                     SerializedProperty starEffectProperty = newProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME).GetArrayElementAtIndex(0);
//                     starEffectProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue = (int)BlockEffectType.Stars;
//                     break;
//                 }
//             }


//             selectedLevelRepresentation.ApplyChanges();
//             effectEditorPosition = new Vector2Int(-1, -1);
//             needUpdateLevelPreview = true;
//             levelBlockPosition = newPosition;
//             needToSelectLevelBlock = true;
//             cellTypeHandler.selectedCellTypeValue = (int)ElementType.InnerTile;
//             Repaint();
//         }

//         public void HandleBlockDelete(Vector2Int oldPosition)
//         {
//             if (selectedBlockPosition == oldPosition)
//             {
//                 IsBlockSelected = false;
//                 Debug.LogWarning("Block unselected to prevent data corruption.");
//             }

//             SerializedProperty oldProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(oldPosition.x, oldPosition.y));
//             oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_TYPE_PROPERTY_NAME).intValue = 0;
//             oldProperty.FindPropertyRelative(LevelRepresentation.GATE_EFFECTS_PROPERTY_NAME).arraySize = 0;
//             oldProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME).arraySize = 0;
//             oldProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue = (int)ElementType.InnerTile;
//             selectedLevelRepresentation.ApplyChanges();
//             needUpdateLevelPreview = true;
//             Repaint();
//         }

//         private void DrawLevel()
//         {
//             drawRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
//             xSize = Mathf.Floor(drawRect.width / selectedLevelRepresentation.sizeProperty.vector2IntValue.x);
//             ySize = Mathf.Floor(drawRect.height / selectedLevelRepresentation.sizeProperty.vector2IntValue.y);
//             elementSize = Mathf.Max(minGridCellSize, Mathf.Min(xSize, ySize));
//             GUILayout.Space(elementSize * selectedLevelRepresentation.sizeProperty.vector2IntValue.y);
//             currentEvent = Event.current;
//             CellTypesHandler.CellType cellType;
//             CellTypesHandler.CellType gateType;
//             CellTypesHandler.ExtraProp extraProp;

//             if (currentEvent.type == EventType.MouseUp)
//             {
//                 if (positions.Count != 0)
//                 {
//                     positions.Clear();
//                 }
//             }

//             //handle drag
//             if ((!IsBlockSelected) && (currentEvent.type == EventType.MouseDrag) && (currentEvent.button == 0) && hasFocus && (FigureSelectorWindow.window == null))
//             {
//                 elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);
//                 elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), selectedLevelRepresentation.sizeProperty.vector2IntValue.y - 1 - Mathf.FloorToInt(elementUnderMouseIndex.y));

//                 if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y) && (!positions.Contains(elementPosition)))
//                 {
//                     positions.Add(elementPosition);
//                     selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, cellTypeHandler.selectedCellTypeValue, Mathf.Clamp(gateColorHandler.selectedCellTypeValue, 0, int.MaxValue));
//                     needUpdateLevelPreview = true;
//                     Repaint();
//                 }
//             }

//             //Handle  click
//             if ((currentEvent.type == EventType.MouseDown) && (hasFocus) && (FigureSelectorWindow.window == null))
//             {
//                 elementUnderMouseIndex = (currentEvent.mousePosition - drawRect.position) / (elementSize);

//                 elementPosition = new Vector2Int(Mathf.FloorToInt(elementUnderMouseIndex.x), selectedLevelRepresentation.sizeProperty.vector2IntValue.y - 1 - Mathf.FloorToInt(elementUnderMouseIndex.y));

//                 if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y))
//                 {
//                     if ((!IsBlockSelected) && (currentEvent.button == 0))
//                     {
//                         selectedLevelRepresentation.SetItemsValue(elementPosition.x, elementPosition.y, cellTypeHandler.selectedCellTypeValue, gateColorHandler.selectedCellTypeValue);
//                         positions.Add(elementPosition);
//                         currentEvent.Use();
//                         needUpdateLevelPreview = true;
//                     }
//                     else if ((currentEvent.button == 1) && (currentEvent.type == EventType.MouseDown))
//                     {
//                         int index = selectedLevelRepresentation.GetIndex(elementPosition.x, elementPosition.y);
//                         tempCellProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(index);

//                         if ((tempCellProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue == (int)ElementType.Block) || (tempCellProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue == (int)ElementType.Gate) || (tempCellProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue == (int)ElementType.InteractableObject))
//                         {
//                             if (IsBlockSelected && (selectedBlockPosition.x == elementPosition.x) && (selectedBlockPosition.y == elementPosition.y))
//                             {
//                                 IsBlockSelected = false;
//                             }
//                             else
//                             {
//                                 selectedBlockPosition = new Vector2Int(elementPosition.x, elementPosition.y);
//                                 selectedBlockProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(index);
//                                 IsBlockSelected = true;
//                                 isSelectedBlockGate = (tempCellProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue == (int)ElementType.Gate);

//                                 if (isSelectedBlockGate)
//                                 {
//                                     CollectNearbyGates();
//                                 }

//                                 selectedBlockLabel = $"{(isSelectedBlockGate ? "Gate Block" : "Block")} #{selectedBlockProperty.GetPropertyArrayIndex()} {selectedBlockPosition.ToString()}";
//                             }
//                         }
//                         else // reset selection
//                         {
//                             IsBlockSelected = false;
//                             selectedBlockPosition = new Vector2Int(elementPosition.x, elementPosition.y); // we memorize block position anyway
//                         }

//                         Repaint();
//                     }
//                 }
//             }

//             //draw
//             for (int i = 0; i < selectedLevelRepresentation.itemsProperty.arraySize; i++)
//             {
//                 tempCellProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(i);
//                 tempCellPosition = tempCellProperty.FindPropertyRelative(LevelRepresentation.POSITION_PROPERTY_NAME).vector2IntValue;
//                 cellType = cellTypeHandler.GetCellType(tempCellProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue);
//                 extraProp = cellTypeHandler.GetExtraProp(tempCellProperty.FindPropertyRelative(LevelRepresentation.BLOCK_TYPE_PROPERTY_NAME).intValue);
//                 buttonRect = GetPositionRect(tempCellPosition);

//                 if ((cellType.value == (int)ElementType.Gate) || (cellType.value == (int)ElementType.Block))
//                 {
//                     gateType = gateColorHandler.GetCellType(Mathf.Clamp(tempCellProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue, 0, int.MaxValue));
//                     DrawColorRect(buttonRect, gateType.color);
//                 }
//                 else
//                 {
//                     DrawColorRect(buttonRect, cellType.color);
//                 }

//                 tempTexture = GetTexture(cellType.value);

//                 if (tempTexture != null)
//                 {
//                     textureRect = new Rect(buttonRect);
//                     textureRect.xMin += 2f;
//                     textureRect.yMin += 2f;
//                     GUI.DrawTexture(textureRect, tempTexture);
//                 }
//             }

//             figuresDictionary.Clear();

//             //Second draw for block data
//             for (int i = 0; i < selectedLevelRepresentation.itemsProperty.arraySize; i++)
//             {
//                 tempCellProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(i);
//                 cellType = cellTypeHandler.GetCellType(tempCellProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue);

//                 if (cellType.value != (int)ElementType.Block)
//                 {
//                     continue;
//                 }

//                 tempCellPosition = tempCellProperty.FindPropertyRelative(LevelRepresentation.POSITION_PROPERTY_NAME).vector2IntValue;
//                 extraProp = cellTypeHandler.GetExtraProp(tempCellProperty.FindPropertyRelative(LevelRepresentation.BLOCK_TYPE_PROPERTY_NAME).intValue);
//                 gateType = gateColorHandler.GetCellType(Mathf.Clamp(tempCellProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue, 0, int.MaxValue));
//                 buttonRect = GetPositionRect(tempCellPosition);

//                 if (tempCellProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME).arraySize > 0)
//                 {
//                     tempDrawBlockEffectLabel = true;
//                     tempBlockEffectLabel = selectedLevelRepresentation.GetBlockEffectLabel(tempCellProperty.FindPropertyRelative(LevelRepresentation.BLOCK_EFFECTS_PROPERTY_NAME));
//                 }
//                 else
//                 {
//                     tempDrawBlockEffectLabel = false;
//                 }

//                 if (IsBlockSelected && (tempCellPosition == selectedBlockPosition))
//                 {
//                     GUI.DrawTexture(buttonRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 8, 0);
//                     //draw circle for pivot
//                     DrawCircle(buttonRect, 0.7f, Color.red);

//                     if (tempDrawBlockEffectLabel)
//                     {
//                         GUI.Label(buttonRect, tempBlockEffectLabel, cellTypeHandler.GetLabelStyle(gateType.color));
//                     }

//                     //draw neigbours
//                     for (int j = 0; j < figureRepresentations[extraProp.value].offsets.Length; j++)
//                     {
//                         elementPosition = tempCellPosition + figureRepresentations[extraProp.value].offsets[j];

//                         if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y))
//                         {
//                             Rect tempElementRect = GetPositionRect(elementPosition);
//                             DrawColorRect(tempElementRect, gateType.color);

//                             if (tempDrawBlockEffectLabel)
//                             {
//                                 GUI.Label(tempElementRect, tempBlockEffectLabel, cellTypeHandler.GetLabelStyle(gateType.color));
//                             }

//                             if (!figuresDictionary.TryAdd(elementPosition, 1))
//                             {
//                                 Debug.LogWarning("Figures are overlapping.Check figure at : " + tempCellPosition.ToString());
//                                 figuresDictionary[elementPosition]++;
//                                 DrawCircle(tempElementRect, 0.5f, Color.blue, true);
//                                 GUI.Label(tempElementRect, figuresDictionary[elementPosition].ToString(), cellTypeHandler.GetLabelStyle(gateType.color));
//                             }

//                             //select all part of figure
//                             GUI.DrawTexture(tempElementRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Color.white, 8, 0);

//                             //Drawing line
//                             Handles.BeginGUI();
//                             Handles.color = Color.white;
//                             Handles.DrawLine(tempElementRect.center, buttonRect.center);
//                             Handles.EndGUI();
//                         }
//                         else
//                         {
//                             Debug.LogWarning("Figure outside level bounds check figure at :" + tempCellPosition.ToString());
//                         }
//                     }
//                 }
//                 else
//                 {
//                     //draw circle for pivot
//                     DrawCircle(buttonRect, 0.7f, Color.red);

//                     if (tempDrawBlockEffectLabel)
//                     {
//                         GUI.Label(buttonRect, tempBlockEffectLabel, cellTypeHandler.GetLabelStyle(gateType.color));
//                     }

//                     //draw neigbours
//                     for (int j = 0; j < figureRepresentations[extraProp.value].offsets.Length; j++)
//                     {
//                         elementPosition = tempCellPosition + figureRepresentations[extraProp.value].offsets[j];

//                         if ((elementPosition.x >= 0) && (elementPosition.x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x) && (elementPosition.y >= 0) && (elementPosition.y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y))
//                         {
//                             if (selectedBlockPosition == elementPosition) // we select figure
//                             {
//                                 selectedBlockProperty = tempCellProperty.Copy();
//                                 selectedBlockPosition = tempCellPosition;
//                                 IsBlockSelected = true;
//                                 isSelectedBlockGate = false;
//                                 selectedBlockLabel = $"{(isSelectedBlockGate ? "Gate Block" : "Block")} #{selectedBlockProperty.GetPropertyArrayIndex()} {selectedBlockPosition.ToString()}";
//                             }


//                             Rect tempElementRect = GetPositionRect(elementPosition);
//                             DrawColorRect(tempElementRect, gateType.color);

//                             if (tempDrawBlockEffectLabel)
//                             {
//                                 GUI.Label(tempElementRect, tempBlockEffectLabel, cellTypeHandler.GetLabelStyle(gateType.color));
//                             }

//                             if (!figuresDictionary.TryAdd(elementPosition, 1))
//                             {
//                                 Debug.LogWarning("Figures are overlapping.Check figure at : " + tempCellPosition.ToString());
//                                 figuresDictionary[elementPosition]++;
//                                 DrawCircle(tempElementRect, 0.5f, Color.blue, true);
//                                 GUI.Label(tempElementRect, figuresDictionary[elementPosition].ToString(), cellTypeHandler.GetLabelStyle(gateType.color));
//                             }

//                             //Drawing line
//                             Handles.BeginGUI();
//                             Handles.color = Color.white;
//                             Handles.DrawLine(tempElementRect.center, buttonRect.center);
//                             Handles.EndGUI();
//                         }
//                         else
//                         {
//                             Debug.LogWarning("Figure outside level bounds check figure at :" + tempCellPosition.ToString());
//                         }
//                     }
//                 }
//             }

//             //draw grid
//             for (int x = 0; x < selectedLevelRepresentation.sizeProperty.vector2IntValue.x + 1; x++)
//             {
//                 lineRect = new Rect(drawRect.x + (x * elementSize), drawRect.y, 2, selectedLevelRepresentation.sizeProperty.vector2IntValue.y * elementSize);
//                 DrawColorRect(lineRect, GRID_COLOR);
//             }

//             for (int y = 0; y < selectedLevelRepresentation.sizeProperty.vector2IntValue.y + 1; y++)
//             {
//                 lineRect = new Rect(drawRect.x, drawRect.y + (y * elementSize), selectedLevelRepresentation.sizeProperty.vector2IntValue.x * elementSize, 2);
//                 DrawColorRect(lineRect, GRID_COLOR);
//             }

//             EditorGUILayout.Space();
//             EditorGUILayout.EndVertical();
//         }

//         private void CollectNearbyGates()
//         {
//             selectedGateNeighbours.Clear();
//             Vector2Int tempPosition = selectedBlockPosition;
//             int colorValue = selectedBlockProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue;
//             Vector2Int[] directions = { Vector2Int.left, Vector2Int.up, Vector2Int.down, Vector2Int.right };
//             int directionIndex = 0;
//             SerializedProperty blockProperty;

//             while (directionIndex < directions.Length)
//             {
//                 tempPosition += directions[directionIndex];

//                 if ((tempPosition.x < 0) || (tempPosition.x >= selectedLevelRepresentation.sizeProperty.vector2IntValue.x) || (tempPosition.y < 0) || (tempPosition.y >= selectedLevelRepresentation.sizeProperty.vector2IntValue.y))
//                 {
//                     tempPosition = selectedBlockPosition;
//                     directionIndex++;
//                     continue;
//                 }

//                 blockProperty = selectedLevelRepresentation.itemsProperty.GetArrayElementAtIndex(selectedLevelRepresentation.GetIndex(tempPosition.x, tempPosition.y));

//                 if ((blockProperty.FindPropertyRelative(LevelRepresentation.BLOCK_COLOR_PROPERTY_NAME).intValue != colorValue) || (blockProperty.FindPropertyRelative(LevelRepresentation.TYPE_PROPERTY_NAME).intValue != (int)ElementType.Gate))
//                 {
//                     tempPosition = selectedBlockPosition;
//                     directionIndex++;
//                 }
//                 else
//                 {
//                     selectedGateNeighbours.Add(blockProperty.Copy());
//                 }
//             }
//         }

//         private Rect GetPositionRect(Vector2Int position)
//         {
//             invertedY = selectedLevelRepresentation.sizeProperty.vector2IntValue.y - 1 - position.y;

//             buttonRectX = drawRect.position.x + position.x * elementSize;
//             buttonRectY = drawRect.position.y + invertedY * elementSize;

//             return new Rect(buttonRectX, buttonRectY, elementSize, elementSize);
//         }

//         private void DrawCircle(Rect parent, float radius, Color color, bool drawFullCircle = false)
//         {
//             textureRect = new Rect(parent);

//             float size = parent.width * radius;

//             float x = parent.x + (buttonRect.width - size) / 2f;
//             float y = parent.y + (buttonRect.height - size) / 2f;

//             textureRect = new Rect(x, y, size, size);

//             if (drawFullCircle)
//             {
//                 GUI.DrawTexture(textureRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, size, 30);
//             }
//             else
//             {
//                 GUI.DrawTexture(textureRect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 4, 30);
//             }

//         }

//         private void DrawTipsAndWarnings()
//         {
//             infoRect = EditorGUILayout.BeginVertical(GUILayout.MinHeight(INFO_HEIGH));
//             EditorGUILayout.HelpBox(LEVEL_INSTRUCTION, MessageType.Info);
//             EditorGUILayout.HelpBox(RIGHT_CLICK_INSTRUCTION, MessageType.Info);

//             //if (selectedLevelRepresentation.IsLevelCorrect)
//             //{
//             //    EditorGUILayout.HelpBox(LEVEL_PASSED_VALIDATION, MessageType.Info);
//             //}
//             //else
//             //{
//             //    EditorGUILayout.HelpBox(selectedLevelRepresentation.errorLabels[0], MessageType.Error);
//             //}

//             EditorGUILayout.EndVertical();
//             //Debug.Log(infoRect.height);
//         }

//         private void DisplayEditorTab()
//         {
//             EditorGUI.BeginChangeCheck();
//             minGridCellSize = EditorGUILayout.IntField(new GUIContent("minGridCellSize", "Minimum size of cell in level grid"), minGridCellSize);
//             if (EditorGUI.EndChangeCheck())
//             {
//                 EditorPrefs.SetInt(PREFS_MIN_ELEMENT_SIZE, minGridCellSize);
//             }

//             EditorGUILayout.LabelField("Cell types:", EditorCustomStyles.labelLargeBold);

//             for (int i = 0; i < cellsSerializedProperty.arraySize; i++)
//             {
//                 EditorGUILayout.LabelField(cellTypeHandler.GetCellType(i).label, EditorCustomStyles.labelBold);
//                 EditorGUI.indentLevel++;

//                 SerializedProperty serializedProperty = cellsSerializedProperty.GetArrayElementAtIndex(i);

//                 SerializedProperty iterator = cellsSerializedProperty.GetArrayElementAtIndex(i).Copy();

//                 while (iterator.NextVisible(true) && iterator.propertyPath.Contains(serializedProperty.propertyPath))
//                 {
//                     if (iterator.name != TYPE_PROPERTY_NAME)
//                     {
//                         EditorGUILayout.PropertyField(iterator);
//                     }

//                 }

//                 EditorGUI.indentLevel--;
//             }

//             EditorGUILayout.LabelField("Gate colors:", EditorCustomStyles.labelLargeBold);

//             for (int i = 0; i < editorColorsDataSerializedProperty.arraySize; i++)
//             {
//                 EditorGUILayout.PropertyField(editorColorsDataSerializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative(COLOR_PROPERTY_NAME), new GUIContent(gateColorHandler.GetCellType(i).label));
//             }

//             DisplayProperties();
//         }

//         public override void OnBeforeAssemblyReload()
//         {
//             lastActiveLevelOpened = false;
//         }


//         public override bool WindowClosedInPlaymode()
//         {
//             return false;
//         }

//         private void OnDestroy()
//         {
//             EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
//             AssemblyReloadEvents.beforeAssemblyReload -= UnloadEditor;
//             AssetDatabase.SaveAssets();

//             try
//             {
//                 EditorSceneController.Instance.Unsubscribe();
//                 UnloadEditor();
//             }
//             catch
//             {
//             }

//             if (!EditorApplication.isPlayingOrWillChangePlaymode)
//             {
//                 OpenScene(GAME_SCENE_PATH);
//             }
//         }


//         protected class FigureRepresentation
//         {
//             public Vector2Int[] offsets;
//             private LevelFigure figure;

//             public FigureRepresentation(LevelFigure figure)
//             {
//                 this.figure = figure;
//                 List<Vector2Int> list = new List<Vector2Int>();
//                 int index = 0;

//                 for (int y = 0; y < figure.Size.y; y++)
//                 {
//                     for (int x = 0; x < figure.Size.x; x++)
//                     {
//                         if (figure.Points[index].IsFilled)
//                         {
//                             if (!((figure.PivotPoint.x == x) && (figure.PivotPoint.y == y)))
//                             {
//                                 list.Add(new Vector2Int(x - figure.PivotPoint.x, y - figure.PivotPoint.y));
//                             }
//                         }

//                         index++;
//                     }
//                 }

//                 offsets = list.ToArray();
//             }
//         }

//         private class LoadByHashModalWindow : EditorWindow
//         {
//             public LevelsHandler levelsHandler;
//             LevelEditorWindow levelEditorWindow;
//             string currentHash;

//             public void SetData(LevelEditorWindow levelEditorWindow)
//             {
//                 this.levelEditorWindow = levelEditorWindow;
//             }

//             private void OnGUI()
//             {
//                 EditorGUI.BeginChangeCheck();
//                 currentHash = EditorGUILayout.TextField("Hash", currentHash);

//                 if (EditorGUI.EndChangeCheck())
//                 {
//                     currentHash = currentHash.Trim();
//                     LevelData data = levelEditorWindow.levelsHandler.SelectedLevelProperty.objectReferenceValue as LevelData;

//                     data = data.DecompressLevel(currentHash);
//                     EditorUtility.SetDirty(data);

//                     levelEditorWindow.lastActiveLevelOpened = false;
//                     Close();
//                 }
//             }
//         }


//         protected class LevelRepresentation : LevelRepresentationBase
//         {
//             private const string SIZE_PROPERTY_NAME = "size";
//             private const string ITEMS_PROPERTY_NAME = "levelElements";

//             //LevelElement
//             public const string TYPE_PROPERTY_NAME = "type";
//             public const string POSITION_PROPERTY_NAME = "position";
//             public const string BLOCK_TYPE_PROPERTY_NAME = "blockType";
//             public const string BLOCK_COLOR_PROPERTY_NAME = "blockColor";
//             public const string BLOCK_EFFECTS_PROPERTY_NAME = "blockEffects";
//             public const string GATE_EFFECTS_PROPERTY_NAME = "gateEffects";
//             public const string INTERACTABLE_OBJECT_DATA_PROPERTY_NAME = "interactableObjectData";

//             public SerializedProperty sizeProperty;
//             public SerializedProperty itemsProperty;
//             private SerializedProperty tempElement;
//             private Vector2Int tempPosition;

//             public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
//             {
//             }

//             protected override void ReadFields()
//             {
//                 sizeProperty = serializedLevelObject.FindProperty(SIZE_PROPERTY_NAME);
//                 itemsProperty = serializedLevelObject.FindProperty(ITEMS_PROPERTY_NAME);
//             }

//             public override void Clear()
//             {
//                 sizeProperty.vector2IntValue = Vector2Int.one;
//                 itemsProperty.arraySize = 1;
//                 ApplyChanges();
//             }

//             public int GetItemsValue(int index1, int index2)
//             {
//                 int index = GetIndex(index1, index2);
//                 return itemsProperty.GetArrayElementAtIndex(index).FindPropertyRelative(TYPE_PROPERTY_NAME).intValue;
//             }

//             public void SetItemsValue(int index1, int index2, int newCellTypeValue, int newGateTypeValue)
//             {
//                 int index = GetIndex(index1, index2);
//                 SerializedProperty element = itemsProperty.GetArrayElementAtIndex(index);
//                 element.FindPropertyRelative(GATE_EFFECTS_PROPERTY_NAME).arraySize = 0;
//                 element.FindPropertyRelative(BLOCK_EFFECTS_PROPERTY_NAME).arraySize = 0;
//                 element.FindPropertyRelative(INTERACTABLE_OBJECT_DATA_PROPERTY_NAME).FindPropertyRelative(TYPE_PROPERTY_NAME).intValue = 0;

//                 SerializedProperty cellTypeProperty = element.FindPropertyRelative(TYPE_PROPERTY_NAME);
//                 SerializedProperty blockColorProperty = element.FindPropertyRelative(BLOCK_COLOR_PROPERTY_NAME);

//                 if (cellTypeProperty.intValue == newCellTypeValue)
//                 {
//                     if (blockColorProperty.intValue == newGateTypeValue)
//                     {
//                         cellTypeProperty.intValue = (int)ElementType.InnerTile;
//                         blockColorProperty.intValue = 0;
//                     }
//                     else
//                     {
//                         blockColorProperty.intValue = newGateTypeValue;
//                     }


//                 }
//                 else
//                 {
//                     cellTypeProperty.intValue = newCellTypeValue;
//                     blockColorProperty.intValue = newGateTypeValue;
//                 }
//             }

//             public int GetExtraPropsValue(int index1, int index2)
//             {
//                 return itemsProperty.GetArrayElementAtIndex(GetIndex(index1, index2)).FindPropertyRelative(BLOCK_TYPE_PROPERTY_NAME).intValue;
//             }

//             public void SetExtraPropsValue(int index1, int index2, int newValue)
//             {
//                 itemsProperty.GetArrayElementAtIndex(GetIndex(index1, index2)).FindPropertyRelative(BLOCK_TYPE_PROPERTY_NAME).intValue = newValue;
//             }

//             public int GetIndex(int index1, int index2)
//             {
//                 tempPosition.Set(index1, index2);

//                 for (int i = 0; i < itemsProperty.arraySize; i++)
//                 {
//                     tempElement = itemsProperty.GetArrayElementAtIndex(i);

//                     if (tempElement.FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue.Equals(tempPosition))
//                     {
//                         return i;
//                     }
//                 }

//                 return -1;
//             }

//             public void HandleSizePropertyChange()
//             {
//                 if (sizeProperty.vector2IntValue.x < 2)
//                 {
//                     sizeProperty.vector2IntValue = new Vector2Int(2, sizeProperty.vector2IntValue.y);
//                 }

//                 if (sizeProperty.vector2IntValue.y < 2)
//                 {
//                     sizeProperty.vector2IntValue = new Vector2Int(sizeProperty.vector2IntValue.x, 2);
//                 }

//                 itemsProperty.arraySize = 0;
//                 SerializedProperty element;

//                 for (int y = 0; y < sizeProperty.vector2IntValue.y; y++)
//                 {
//                     for (int x = 0; x < sizeProperty.vector2IntValue.x; x++)
//                     {
//                         itemsProperty.arraySize++;
//                         element = itemsProperty.GetArrayElementAtIndex(itemsProperty.arraySize - 1);
//                         element.FindPropertyRelative(POSITION_PROPERTY_NAME).vector2IntValue = new Vector2Int(x, y);
//                         element.FindPropertyRelative(BLOCK_TYPE_PROPERTY_NAME).intValue = (int)BlockType.Single;
//                         element.FindPropertyRelative(BLOCK_EFFECTS_PROPERTY_NAME).arraySize = 0;
//                         element.FindPropertyRelative(GATE_EFFECTS_PROPERTY_NAME).arraySize = 0;
//                         element.FindPropertyRelative(INTERACTABLE_OBJECT_DATA_PROPERTY_NAME).FindPropertyRelative(TYPE_PROPERTY_NAME).intValue = 0;

//                         if ((x == 0) || (y == 0) || (x == sizeProperty.vector2IntValue.x - 1) || (y == sizeProperty.vector2IntValue.y - 1))
//                         {
//                             element.FindPropertyRelative(TYPE_PROPERTY_NAME).intValue = (int)ElementType.Border;
//                         }
//                         else
//                         {
//                             element.FindPropertyRelative(TYPE_PROPERTY_NAME).intValue = (int)ElementType.InnerTile;
//                         }
//                     }
//                 }
//             }

//             public string GetBlockEffectLabel(BlockEffectData data)
//             {
//                 switch (data.Type)
//                 {
//                     case BlockEffectType.Ice: return "Ice " + data.iceTurnsAmount;
//                     case BlockEffectType.FixedDirection: return data.horizontalDirection ? "↔" : "↕";
//                     case BlockEffectType.Layered: return "layer:" + data.layeredBlockColor;
//                     case BlockEffectType.Stars: return "star";
//                     case BlockEffectType.Chain: return "chain " + data.keysAmount;
//                     case BlockEffectType.Key: return "key";
//                     case BlockEffectType.Combines: return "combId:" + data.combineGroupID;
//                     case BlockEffectType.Bomb: return "bomb " + data.bombDuration;
//                     case BlockEffectType.Ropes: return "ropes " + data.ropesColors.Length;
//                     case BlockEffectType.Scissors: return "scissors:" + data.scissorsColor;
//                     default:
//                         Debug.LogWarning("Unknown effect found: " + data.Type);
//                         return "some effect";
//                 }
//             }

//             public string GetBlockEffectLabel(SerializedProperty arrayProperty)
//             {
//                 BlockEffectData effectData;

//                 if (arrayProperty.arraySize > 1)
//                 {
//                     string[] effects = new string[arrayProperty.arraySize];

//                     for (int i = 0; i < arrayProperty.arraySize; i++)
//                     {
//                         effectData = (BlockEffectData)arrayProperty.GetArrayElementAtIndex(i).boxedValue;
//                         effects[i] = GetBlockEffectLabel(effectData);
//                     }

//                     return string.Join(",", effects);
//                 }
//                 else
//                 {
//                     effectData = (BlockEffectData)arrayProperty.GetArrayElementAtIndex(0).boxedValue;
//                     return GetBlockEffectLabel(effectData);
//                 }
//             }


//             public void CopyEffects(SerializedProperty oldArrayProperty, SerializedProperty arrayProperty)
//             {
//                 arrayProperty.arraySize = oldArrayProperty.arraySize;

//                 for (int i = 0; i < oldArrayProperty.arraySize; i++)
//                 {
//                     arrayProperty.GetArrayElementAtIndex(i).boxedValue = oldArrayProperty.GetArrayElementAtIndex(i).boxedValue;
//                 }
//             }
//         }
//     }
// }
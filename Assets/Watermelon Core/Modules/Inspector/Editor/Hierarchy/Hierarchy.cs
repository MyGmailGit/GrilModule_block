using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Watermelon
{
    public class Hierarchy
    {
        private static Type sceneHierarchyWindowType;
        private static Type sceneHierarchyType;
        private static Type treeViewControllerType;
        private static Type treeViewGUIType;

        private static PropertyInfo sceneHierarchyProperty;
        private static FieldInfo hierarchyTreeViewField;
        private static PropertyInfo treeViewGUIProperty;

        private static FieldInfo iconWidthField;
        private static FieldInfo iconSpaceField;

        private EditorWindow window;

        private object sceneHierarchy;
        private object treeViewController;
        private object treeViewGUI;

        private readonly float defaultIconWidth;
        private readonly float defaultSpaceBeforeIcon;

        private static PropertyInfo lastInteractedHierarchyWindow;

        private static Func<object> getLastInteractedHierarchyWindow;

        private static Dictionary<object, Hierarchy> hierarchies =
            new Dictionary<object, Hierarchy>();

        [InitializeOnLoadMethod]
        private static void PrepareData()
        {
            try
            {
                Assembly editorAssembly = typeof(Editor).Assembly;

                sceneHierarchyWindowType =
                    editorAssembly.GetType("UnityEditor.SceneHierarchyWindow");

                if (sceneHierarchyWindowType == null)
                {
                    Debug.LogWarning(
                        "[Watermelon] SceneHierarchyWindow type not found."
                    );

                    return;
                }

                sceneHierarchyProperty =
                    sceneHierarchyWindowType.GetProperty(
                        "sceneHierarchy",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance
                    );

                sceneHierarchyType =
                    editorAssembly.GetType("UnityEditor.SceneHierarchy");

                if (sceneHierarchyType == null)
                {
                    Debug.LogWarning(
                        "[Watermelon] SceneHierarchy type not found."
                    );

                    return;
                }

                hierarchyTreeViewField =
                    sceneHierarchyType.GetField(
                        "m_TreeView",
                        BindingFlags.NonPublic |
                        BindingFlags.Instance
                    );

                treeViewControllerType =
                    typeof(TreeViewState).Assembly.GetType(
                        "UnityEditor.IMGUI.Controls.TreeViewController"
                    );

                if (treeViewControllerType == null)
                {
                    Debug.LogWarning(
                        "[Watermelon] TreeViewController type not found."
                    );

                    return;
                }

                treeViewGUIProperty =
                    treeViewControllerType.GetProperty(
                        "gui",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Instance
                    );

                treeViewGUIType =
                    typeof(TreeView).Assembly.GetType(
                        "UnityEditor.IMGUI.Controls.TreeViewGUI"
                    );

                if (treeViewGUIType == null)
                {
                    Debug.LogWarning(
                        "[Watermelon] TreeViewGUI type not found."
                    );

                    return;
                }

                iconWidthField =
                    treeViewGUIType.GetField(
                        "k_IconWidth",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static |
                        BindingFlags.Instance
                    );

                iconSpaceField =
                    treeViewGUIType.GetField(
                        "k_SpaceBetweenIconAndText",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static |
                        BindingFlags.Instance
                    );

                lastInteractedHierarchyWindow =
                    sceneHierarchyWindowType.GetProperty(
                        "lastInteractedHierarchyWindow",
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static
                    );

                if (lastInteractedHierarchyWindow != null)
                {
                    // 不再使用 Expression.Property + Compile。
                    // Unity 6 内部类型变化后，这种写法容易触发
                    // GetterAdapterFrame / InvalidCastException。
                    getLastInteractedHierarchyWindow = () =>
                    {
                        try
                        {
                            return lastInteractedHierarchyWindow.GetValue(
                                null,
                                null
                            );
                        }
                        catch
                        {
                            return null;
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public Hierarchy(EditorWindow window)
        {
            this.window = window;

            if (window == null)
                return;

            try
            {
                sceneHierarchy =
                    sceneHierarchyProperty?.GetValue(
                        window,
                        null
                    );

                if (sceneHierarchy == null)
                    return;

                treeViewController =
                    hierarchyTreeViewField?.GetValue(
                        sceneHierarchy
                    );

                if (treeViewController == null)
                    return;

                treeViewGUI =
                    treeViewGUIProperty?.GetValue(
                        treeViewController,
                        null
                    );

                if (treeViewGUI == null)
                    return;

                object iconWidth =
                    iconWidthField?.GetValue(treeViewGUI);

                object iconSpace =
                    iconSpaceField?.GetValue(treeViewGUI);

                if (iconWidth is float)
                {
                    defaultIconWidth = (float)iconWidth;
                }

                if (iconSpace is float)
                {
                    defaultSpaceBeforeIcon = (float)iconSpace;
                }

                SetIconWidth(0, 18);
            }
            catch (Exception e)
            {
                Debug.LogWarning(
                    $"[Watermelon] Failed to initialize custom hierarchy: {e}"
                );
            }
        }

        private void SetIconWidth(float iconWidth, float spaceBeforeIcon)
        {
            if (treeViewGUI == null)
                return;

            try
            {
                iconWidthField?.SetValue(
                    treeViewGUI,
                    iconWidth
                );

                iconSpaceField?.SetValue(
                    treeViewGUI,
                    spaceBeforeIcon
                );
            }
            catch
            {
                // Unity Editor internal API changed.
                // Ignore custom hierarchy modification.
            }
        }

        private void ResetIconWidth()
        {
            SetIconWidth(
                defaultIconWidth,
                defaultSpaceBeforeIcon
            );
        }

        public void DrawElementGUI(
            int instanceID,
            Rect selectionRect
        )
        {
            GameObject instanceObject =
                EditorUtility.InstanceIDToObject(instanceID)
                as GameObject;

            if (!instanceObject)
                return;

            Texture texture =
                EditorCustomHierarchy.GetTexture(
                    instanceObject
                );

            if (texture == null)
                return;

            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                ResetIconWidth();
                return;
            }

            SetIconWidth(0, 18);

            Rect iconRect =
                new Rect(selectionRect)
                {
                    width = 16,
                    height = 16
                };

            iconRect.y +=
                (iconRect.height - 16) / 2;

            using (new ColorScope(
                EditorCustomStyles.HIERARCHY_COLOR))
            {
                GUI.DrawTexture(
                    iconRect,
                    texture
                );
            }
        }

        public static Hierarchy GetLastHierarchy()
        {
            if (getLastInteractedHierarchyWindow == null)
                return null;

            object lastHierarchyWindow;

            try
            {
                lastHierarchyWindow =
                    getLastInteractedHierarchyWindow();
            }
            catch
            {
                return null;
            }

            if (lastHierarchyWindow == null)
                return null;

            if (!hierarchies.TryGetValue(
                lastHierarchyWindow,
                out var hierarchy))
            {
                EditorWindow editorWindow =
                    lastHierarchyWindow as EditorWindow;

                if (editorWindow == null)
                    return null;

                hierarchy =
                    new Hierarchy(editorWindow);

                hierarchies.Add(
                    lastHierarchyWindow,
                    hierarchy
                );
            }

            return hierarchy;
        }

        public static void ClearHierarchies()
        {
            if (hierarchies.Count == 0)
                return;

            foreach (Hierarchy hierarchy in hierarchies.Values)
            {
                if (hierarchy != null)
                {
                    hierarchy.ResetIconWidth();
                }
            }

            hierarchies.Clear();
        }
    }
}
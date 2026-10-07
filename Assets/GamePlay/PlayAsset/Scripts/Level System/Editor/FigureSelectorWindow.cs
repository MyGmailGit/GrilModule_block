using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Watermelon
{
    public class FigureSelectorWindow : EditorWindow
    {
        public static FigureSelectorWindow window;
        private LevelFigure[] levelFigures;
        private BlockType[] blockTypes;
        private Color color;
        private Action<object> selectFigure;

        private Rect buttonRect;
        private Rect figurePivotRect;
        private Rect figurePointRect;
        private int lines;
        private Vector2 scrollVector;

        public static void CreateWindow(LevelFigure[] levelFigures, BlockType[] blockTypes, Color color, Action<object> selectFigure)
        {
            if(window != null)
            {
                window.Close();
                window = null;
            }

            window = (FigureSelectorWindow)EditorWindow.GetWindow(typeof(FigureSelectorWindow));
            window.levelFigures = levelFigures;
            window.blockTypes = blockTypes;
            window.color = color;
            window.selectFigure = selectFigure;

            if (levelFigures.Length > 25)
            {
                window.lines = 3;
            }
            else if ( levelFigures.Length > 10)
            {
                window.lines = 2;
            }
            else
            {
                window.lines = 1;
            }

            window.maxSize = new Vector2(750, 400);
            window.minSize = new Vector2(250, 120);
            window.ShowAuxWindow();
            
        }

        private void OnGUI()
        {
            scrollVector = EditorGUILayout.BeginScrollView(scrollVector);
            EditorGUILayout.BeginVertical();

            if (lines == 1)
            {
                for (int i = 0; i < levelFigures.Length; i++)
                {

                    DrawFigureButton(i);
                }
            }
            else 
            {
                int index = 0;

                do
                {
                    EditorGUILayout.BeginHorizontal();

                    for (int i = 0; i < lines; i++)
                    {
                        if (index + i < levelFigures.Length)
                        {
                            DrawFigureButton(index + i);
                        }
                        else
                        {
                            GUILayout.FlexibleSpace();
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    index += lines;

                } while (index < levelFigures.Length);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        private void DrawFigureButton(int figureIndex)
        {
            buttonRect = EditorGUILayout.BeginHorizontal(GUI.skin.box);

            figurePivotRect = GUILayoutUtility.GetRect(24, 24); // for 3*3 figures

            int index = 0;

            for (int y = levelFigures[figureIndex].Size.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < levelFigures[figureIndex].Size.x; x++)
                {
                    if (levelFigures[figureIndex].Points[index].IsFilled)
                    {
                        figurePointRect = new Rect(figurePivotRect.x + 2 + (x * 8), figurePivotRect.y + 2 + (y * 8), 8, 8);
                        LevelEditorBase.DrawColorRect(figurePointRect, color);
                    }

                    index++;
                }
            }

            EditorGUILayout.LabelField(blockTypes[figureIndex].ToString());

            if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none))
            {
                selectFigure?.Invoke(blockTypes[figureIndex]);
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            window = null;
        }
    }
}

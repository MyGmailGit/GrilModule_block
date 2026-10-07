using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace Watermelon
{
    public static class SceneLoadingActionsMenu
    {
        [MenuItem("Actions/Init Scene", priority = 100)]
        private static void InitScene()
        {
            EditorSceneManager.OpenScene(Path.Combine(CoreEditor.FOLDER_SCENES, "Init.unity"));
        }
        [MenuItem("Actions/Init Scene", true)]
        private static bool InitSceneValidation()
        {
            return !Application.isPlaying;
        }


        [MenuItem("Actions/Game Scene", priority = 100)]
        private static void GameScene()
        {
            EditorSceneManager.OpenScene(Path.Combine(CoreEditor.FOLDER_SCENES, "Game.unity"));
        }

        [MenuItem("Actions/Game Scene", true)]
        private static bool GameSceneValidation()
        {
            return !Application.isPlaying;
        }

        [MenuItem("Actions/Menu Scene", priority = 100)]
        private static void MenuScene()
        {
            EditorSceneManager.OpenScene(Path.Combine(CoreEditor.FOLDER_SCENES, "Menu.unity"));
        }

        [MenuItem("Actions/Menu Scene", true)]
        private static bool MenuSceneValidation()
        {
            return !Application.isPlaying;
        }
    }
}
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Watermelon
{
    public static class SaveActionsMenu
    {
        private const string VIDEO_CACHE_FOLDER_NAME = "MenuBackgroundVideoCache";

        [MenuItem("Actions/Open Save Folder", priority = 2)]
        private static void OpenSaveFolder()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("Actions/Remove Save", priority = 1)]
        [MenuItem("Edit/Clear Save", priority = 270)]
        private static void RemoveSave()
        {
            PlayerPrefs.DeleteAll();
            SaveController.DeleteSaveFile();

            string videoCacheDirectory = Path.Combine(Application.persistentDataPath, VIDEO_CACHE_FOLDER_NAME);
            if (Directory.Exists(videoCacheDirectory))
            {
                Directory.Delete(videoCacheDirectory, true);
            }

            Debug.Log("Save files are removed!");
        }

        [MenuItem("Actions/Remove Save", true)]
        private static bool RemoveSaveValidation()
        {
            return !Application.isPlaying;
        }
    }
}

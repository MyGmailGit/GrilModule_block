using UnityEditor;
using UnityEngine;
using System.IO;

public static class SimpleDataCleaner
{
    [MenuItem("Tools/DataCleaner/清理All数据")]
    public static void CleanAllData()
    {
        CleanPlayerPrefsData();
        CleanPersistentDataPathData();
    }


    [MenuItem("Tools/DataCleaner/清理PlayerPrefs数据")]
    public static void CleanPlayerPrefsData()
    {
        // 删除PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("已清理所有PlayerPrefs");
    }
    [MenuItem("Tools/DataCleaner/清理沙盒数据")]
    public static void CleanPersistentDataPathData()
    {

        // 删除沙盒路径下的所有文件
        string persistentPath = Application.persistentDataPath;
        if (Directory.Exists(persistentPath))
        {
            // 删除所有文件
            string[] files = Directory.GetFiles(persistentPath);
            foreach (string file in files)
            {
                File.Delete(file);
            }

            // 删除所有子文件夹
            string[] dirs = Directory.GetDirectories(persistentPath);
            foreach (string dir in dirs)
            {
                Directory.Delete(dir, true);
            }
        }

        Debug.Log("已清理沙盒数据");
        // EditorUtility.DisplayDialog("完成", "数据已清理完成", "确定");
    }
}
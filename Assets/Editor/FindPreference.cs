using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FindReference : Editor
{
    [MenuItem("Tools/Find References in Project", false, 25)]
    private static void FindReferences()
    {
        // 获取用户在 Project 窗口中选中的资源
        string selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(selectedPath))
        {
            Debug.LogWarning("请先在 Project 窗口中选择一个资源。");
            return;
        }

        // 开始查找引用
        Debug.Log($"正在查找引用：{selectedPath}");
        string[] dependencies = AssetDatabase.GetDependencies(selectedPath);
        List<string> references = new List<string>();

        // 遍历所有资源，检查是否依赖于选中的资源
        string[] allAssetsPath = AssetDatabase.GetAllAssetPaths();
        foreach (string assetPath in allAssetsPath)
        {
            // 跳过文件夹和自身
            if (assetPath == selectedPath || AssetDatabase.IsValidFolder(assetPath))
                continue;

            string[] assetDependencies = AssetDatabase.GetDependencies(assetPath, false);
            foreach (string dep in assetDependencies)
            {
                if (dep == selectedPath)
                {
                    references.Add(assetPath);
                    break;
                }
            }
        }

        // 输出结果
        if (references.Count == 0)
        {
            Debug.Log($"在项目中未找到任何引用 '{selectedPath}' 的资源。");
        }
        else
        {
            Debug.Log($"找到 {references.Count} 个资源引用了 '{selectedPath}':");
            foreach (string refPath in references)
            {
                Debug.Log($"  - {refPath}");
            }
        }
    }
}
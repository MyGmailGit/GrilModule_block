using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FindDuplicateAssets
{
    private static StringBuilder logBuilder = new StringBuilder();
    // 缓存所有可序列化文件的路径，避免重复IO
    private static List<string> _cachedSerializableAssets;

    // ==========================================
    // 功能 1: 分析选中的文件 (新增)
    // ==========================================
    [MenuItem("Assets/工具/分析选中文件(MD5)", false, 1000)]
    public static void AnalyzeSelectedAssets()
    {
        // 获取选中的对象
        UnityEngine.Object[] selectedObjects = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("请先在 Project 窗口中选择一个或多个文件。");
            return;
        }

        // 准备选中文件的路径列表
        List<string> selectedPaths = new List<string>();
        foreach (var obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            // 排除文件夹，只处理文件
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                selectedPaths.Add(path);
            }
        }

        if (selectedPaths.Count == 0)
        {
            Debug.LogWarning("未找到有效的文件路径。");
            return;
        }

        // 执行分析
        RunAnalysis(selectedPaths);
    }

    // 菜单验证，确保只有在选中资源时才显示
    [MenuItem("Assets/工具/分析选中文件(MD5)", true)]
    public static bool AnalyzeSelectedAssetsValidation()
    {
        return Selection.activeObject != null;
    }

    // ==========================================
    // 功能 2: 扫描整个项目 (保留原有功能)
    // ==========================================
    [MenuItem("Tools/查找重复资源并分析引用(MD5)")]
    public static void FindDuplicatesInProject()
    {
        string[] allFilePaths = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);
        List<string> validPaths = new List<string>();

        foreach (string path in allFilePaths)
        {
            if (!path.EndsWith(".meta"))
            {
                validPaths.Add("Assets" + path.Replace(Application.dataPath, "").Replace("\\", "/"));
            }
        }

        RunAnalysis(validPaths);
    }

    // ==========================================
    // 核心逻辑
    // ==========================================
    private static void RunAnalysis(List<string> targetPaths)
    {
        logBuilder.Clear();
        logBuilder.AppendLine("===== 资源 MD5 与引用分析报告 =====");
        logBuilder.AppendLine($"分析时间: {DateTime.Now}");
        logBuilder.AppendLine($"分析文件数量: {targetPaths.Count}");
        logBuilder.AppendLine("==========================================");

        // 1. 计算选中文件的 MD5
        Dictionary<string, string> pathToMd5 = new Dictionary<string, string>();
        foreach (string path in targetPaths)
        {
            if (EditorUtility.DisplayCancelableProgressBar("正在计算 MD5", path, 0f))
            {
                EditorUtility.ClearProgressBar();
                return;
            }
            string md5 = CalculateMD5(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(md5))
            {
                pathToMd5[path] = md5;
            }
        }

        // 2. 查找重复 (在选中文件内部查找，或者你可以修改逻辑去对比全项目)
        // 这里逻辑是：看选中的这些文件里，有没有互相重复的
        // 如果你想看“选中的文件”是否和“项目里其他文件”重复，逻辑需要调整。
        // 通常“分析选中文件”是为了看这几个文件本身是不是重复的，或者看它们的引用。

        // 修正逻辑：我们找出选中文件里，哪些是重复的。
        // 同时，无论是否重复，都分析它们的引用。

        var md5Groups = new Dictionary<string, List<string>>();
        foreach (var kvp in pathToMd5)
        {
            if (!md5Groups.ContainsKey(kvp.Value))
            {
                md5Groups[kvp.Value] = new List<string>();
            }
            md5Groups[kvp.Value].Add(kvp.Key);
        }

        // 3. 分析引用 (针对所有选中的文件)
        Dictionary<string, List<string>> referenceMap = AnalyzeReferences(targetPaths);

        // 4. 打印结果
        int duplicateCount = 0;
        LogMessage($"\n--- 重复检测结果 ---");

        foreach (var group in md5Groups)
        {
            if (group.Value.Count > 1)
            {
                duplicateCount++;
                LogMessage($"\n[发现重复组 {duplicateCount}] (MD5: {group.Key})", LogType.Warning);
                foreach (string path in group.Value)
                {
                    LogMessage($"  - {path}", LogType.Warning);
                    PrintReferences(path, referenceMap);
                }
            }
        }

        if (duplicateCount == 0)
        {
            LogMessage("选中的文件中未发现重复。");
        }

        // 5. 单独列出引用 (即使不重复也显示引用)
        if (targetPaths.Count > 0)
        {
            LogMessage($"\n--- 引用详情 ---");
            foreach (string path in targetPaths)
            {
                // 如果它不是重复文件，但你想看它的引用，可以在这里打印
                // 这里为了简洁，只在上面打印了重复文件的引用
                // 如果需要打印所有选中文件的引用，可以取消下面的注释
                /*
                if (!IsDuplicate(path, md5Groups)) 
                {
                    LogMessage($"\n[非重复文件] {path}");
                    PrintReferences(path, referenceMap);
                }
                */
            }
        }

        SaveLog();
        EditorUtility.ClearProgressBar();
        Debug.Log("分析完成。");
    }

    private static bool IsDuplicate(string path, Dictionary<string, List<string>> groups)
    {
        foreach (var list in groups.Values)
        {
            if (list.Count > 1 && list.Contains(path)) return true;
        }
        return false;
    }

    private static void PrintReferences(string path, Dictionary<string, List<string>> map)
    {
        if (map.ContainsKey(path) && map[path].Count > 0)
        {
            LogMessage($"    [被引用] 被 {map[path].Count} 个资源引用:");
            foreach (string refPath in map[path])
            {
                LogMessage($"      > {refPath}");
            }
        }
        else
        {
            LogMessage($"    [未被引用] 这是一个孤立资源。");
        }
    }

    private static Dictionary<string, List<string>> AnalyzeReferences(List<string> targetPaths)
    {
        Dictionary<string, List<string>> referenceMap = new Dictionary<string, List<string>>();

        // 缓存可序列化资源，提高性能
        if (_cachedSerializableAssets == null)
        {
            _cachedSerializableAssets = new List<string>();
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (string path in allAssetPaths)
            {
                string ext = Path.GetExtension(path).ToLower();
                if (ext == ".unity" || ext == ".prefab" || ext == ".mat" || ext == ".asset" || ext == ".controller")
                {
                    _cachedSerializableAssets.Add(path);
                }
            }
        }

        for (int i = 0; i < targetPaths.Count; i++)
        {
            string targetPath = targetPaths[i];
            string targetGuid = AssetDatabase.AssetPathToGUID(targetPath);
            List<string> referrers = new List<string>();

            if (EditorUtility.DisplayCancelableProgressBar("正在分析引用", targetPath, (float)i / targetPaths.Count))
            {
                return null;
            }

            foreach (string assetPath in _cachedSerializableAssets)
            {
                string fullPath = Path.GetFullPath(assetPath);
                // 简单的文本匹配，检查是否包含 GUID
                // 注意：这可能会误判（如果GUID恰好出现在注释里），但对于查找引用足够快且准
                if (File.Exists(fullPath))
                {
                    string content = File.ReadAllText(fullPath);
                    if (content.Contains(targetGuid))
                    {
                        referrers.Add(assetPath);
                    }
                }
            }

            referenceMap[targetPath] = referrers;
        }

        return referenceMap;
    }

    private static string CalculateMD5(string filePath)
    {
        try
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"无法计算 MD5: {filePath}, 错误: {e.Message}");
            return null;
        }
    }

    private static void LogMessage(string message, LogType type = LogType.Log)
    {
        logBuilder.AppendLine(message);
        switch (type)
        {
            case LogType.Warning: Debug.LogWarning(message); break;
            case LogType.Error: Debug.LogError(message); break;
            default: Debug.Log(message); break;
        }
    }

    private static void SaveLog()
    {
        string logPath = Path.Combine(Application.dataPath, "../DuplicateAssetsReport.txt");
        File.WriteAllText(logPath, logBuilder.ToString());
        Debug.Log($"报告已保存: {logPath}");
    }
}
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Watermelon;
using Utility;

public class LevelDataImporter : EditorWindow
{
    private TextAsset csvFile;
    private string folderPath = "Assets/LevelData";
    private string encryptionKeyPrefix = "_ball_";

    [MenuItem("Tools/Level Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<LevelDataImporter>("Level Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Data Importer", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // CSV文件选择
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        // 输出文件夹路径
        folderPath = EditorGUILayout.TextField("Output Folder", folderPath);

        if (GUILayout.Button("Select Output Folder"))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 转换为相对路径
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    folderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a folder inside the Assets folder!", "OK");
                }
            }
        }

        EditorGUILayout.Space();

        GUI.enabled = csvFile != null;
        if (GUILayout.Button("Import Level Data", GUILayout.Height(30)))
        {
            ImportLevelData();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear All Level Data", GUILayout.Height(25)))
        {
            ClearAllLevelData();
        }
    }

    private void ImportLevelData()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a CSV file!", "OK");
            return;
        }

        // 确保输出文件夹存在
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 读取CSV内容
        string[] lines = csvFile.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

        int successCount = 0;
        int failCount = 0;
        List<string> errors = new List<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            // 跳过注释行（以#开头）
            if (line.StartsWith("#"))
                continue;

            // 按逗号分割
            string[] parts = line.Split(',');

            if (parts.Length < 6)
            {
                errors.Add($"Line {i + 1}: Insufficient columns (expected at least 6, got {parts.Length})");
                failCount++;
                continue;
            }

            try
            {
                // 解析数据，处理可能的空格
                int levelID = int.Parse(parts[0].Trim());
                int tubeCount = int.Parse(parts[1].Trim());
                string puzzleConfig = parts[2].Trim();
                bool isHideLiquid = bool.Parse(parts[3].Trim());
                int emptyTubeCount = int.Parse(parts[4].Trim());
                int difficulty = int.Parse(parts[5].Trim());

                // 创建LevelData资产
                LevelData levelData = ScriptableObject.CreateInstance<LevelData>();

                // 使用反射设置私有字段（因为字段是SerializeField private）
                var levelIDField = typeof(LevelData).GetField("levelID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var tubeCountField = typeof(LevelData).GetField("tubeCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var puzzleConfigField = typeof(LevelData).GetField("puzzleConfig", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isHideLiquidField = typeof(LevelData).GetField("isHideLiquid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var emptyTubeCountField = typeof(LevelData).GetField("emptyTubeCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var difficultyField = typeof(LevelData).GetField("difficulty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                levelIDField.SetValue(levelData, levelID);
                tubeCountField.SetValue(levelData, tubeCount);

                // 对puzzleConfig进行异或加密
                string encryptedConfig = XorEncrypt(puzzleConfig, $"{levelID}{encryptionKeyPrefix}{levelID}");
                puzzleConfigField.SetValue(levelData, encryptedConfig);

                UnityEngine.Debug.Log(XorDecrypt(encryptedConfig, $"{levelID}{encryptionKeyPrefix}{levelID}"));

                isHideLiquidField.SetValue(levelData, isHideLiquid);
                emptyTubeCountField.SetValue(levelData, emptyTubeCount);
                difficultyField.SetValue(levelData, difficulty);

                // 保存资产
                string assetPath = Path.Combine(folderPath, $"LevelData_{levelID:D4}.asset");
                assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

                AssetDatabase.CreateAsset(levelData, assetPath);
                successCount++;

                EditorUtility.DisplayProgressBar("Importing Level Data", $"Importing level {levelID}...", (float)i / lines.Length);
            }
            catch (System.Exception ex)
            {
                errors.Add($"Line {i + 1}: {ex.Message}");
                failCount++;
            }
        }

        EditorUtility.ClearProgressBar();

        // 刷新Asset数据库
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 显示结果
        string message = $"Import completed!\nSuccess: {successCount}\nFailed: {failCount}";
        if (errors.Count > 0)
        {
            message += $"\n\nErrors:\n{string.Join("\n", errors)}";
        }

        EditorUtility.DisplayDialog("Import Result", message, "OK");

        if (failCount > 0)
        {
            Debug.LogError($"Level Data Import failed for {failCount} items.\n{string.Join("\n", errors)}");
        }
        else
        {
            Debug.Log($"Successfully imported {successCount} level data assets.");
        }
    }

    private void ClearAllLevelData()
    {
        if (!EditorUtility.DisplayDialog("Confirm Clear",
            $"Are you sure you want to delete all .asset files in:\n{folderPath}\n\nThis action cannot be undone!",
            "Yes, Clear All", "Cancel"))
        {
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            EditorUtility.DisplayDialog("Info", "Folder does not exist!", "OK");
            return;
        }

        string[] assetFiles = Directory.GetFiles(folderPath, "*.asset");
        int deletedCount = 0;

        foreach (string file in assetFiles)
        {
            string relativePath = file.Replace("\\", "/");
            if (relativePath.StartsWith(Application.dataPath))
            {
                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
                AssetDatabase.DeleteAsset(relativePath);
                deletedCount++;
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Clear Complete", $"Deleted {deletedCount} level data assets.", "OK");
    }

    /// <summary>
    /// XOR加密/解密方法
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="key">密钥</param>
    /// <returns>加密/解密后的字符串</returns>
    private string XorEncrypt(string input, string key)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        // byte[] resultBytes = new byte[inputBytes.Length];

        // for (int i = 0; i < inputBytes.Length; i++)
        // {
        //     // 使用循环密钥进行XOR
        //     byte keyByte = keyBytes[i % keyBytes.Length];
        //     resultBytes[i] = (byte)(inputBytes[i] ^ keyByte);
        // }
        // 转换为Base64以便存储
        return System.Convert.ToBase64String(CryptoHelper.XorAll(inputBytes, keyBytes));
    }

    /// <summary>
    /// 解密方法（供运行时使用）
    /// </summary>
    public static string XorDecrypt(string encrypted, string key)
    {
        if (string.IsNullOrEmpty(encrypted))
            return encrypted;

        try
        {
            byte[] encryptedBytes = System.Convert.FromBase64String(encrypted);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] resultBytes = CryptoHelper.XorAll(encryptedBytes, keyBytes);

            return Encoding.UTF8.GetString(resultBytes);
        }
        catch
        {
            Debug.LogError("Failed to decrypt puzzle config!");
            return encrypted;
        }
    }
}
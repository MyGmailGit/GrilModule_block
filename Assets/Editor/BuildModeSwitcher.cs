using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildModeSwitcher : EditorWindow
{
    private const string TEST_MODE_DEFINE = "TEST_MODE";
    private const string RELEASE_MODE_DEFINE = "RELEASE_MODE";

    private static readonly string DevPanelSettingsPath = "Assets/GamePlay/Data/Dev Panel Settings.asset"; // 根据实际路径修改
    private static readonly string MonetizationSettingsPath = "Assets/GamePlay/Data/Monetization Settings.asset"; // 根据实际路径修改

    [MenuItem("BuildTools/Build Mode/Switch to Test Mode")]
    public static void SwitchToTestMode()
    {
        // 1. 添加测试宏，移除发布宏
        AddDefineSymbol(TEST_MODE_DEFINE);
        RemoveDefineSymbol(RELEASE_MODE_DEFINE);

        // 2. 修改 DevPanelSettings.asset 中的值
        SetDevPanelSettings(true, true);
        SetMonetizationSettings(true);

        Debug.Log("[Build Mode] Switched to TEST MODE. isEnabled=1, isForceToVideo=1");
    }

    [MenuItem("BuildTools/Build Mode/Switch to Release Mode")]
    public static void SwitchToReleaseMode()
    {
        // 1. 添加发布宏，移除测试宏
        AddDefineSymbol(RELEASE_MODE_DEFINE);
        RemoveDefineSymbol(TEST_MODE_DEFINE);

        // 2. 修改 DevPanelSettings.asset 中的值
        SetDevPanelSettings(false, false);
        SetMonetizationSettings(false);

        Debug.Log("[Build Mode] Switched to RELEASE MODE. isEnabled=0, isForceToVideo=0");
    }

    // 添加宏定义（避免重复添加）
    private static void AddDefineSymbol(string symbol)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (!string.IsNullOrEmpty(defines))
        {
            string[] allDefines = defines.Split(';');
            if (System.Array.IndexOf(allDefines, symbol) >= 0)
                return; // 已存在，不重复添加
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup,
            string.IsNullOrEmpty(defines) ? symbol : defines + ";" + symbol);
    }

    // 移除宏定义
    private static void RemoveDefineSymbol(string symbol)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

        if (string.IsNullOrEmpty(defines))
            return;

        string[] allDefines = defines.Split(';');
        string newDefines = "";

        foreach (string def in allDefines)
        {
            if (def != symbol)
            {
                if (!string.IsNullOrEmpty(newDefines))
                    newDefines += ";";
                newDefines += def;
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
    }

    // 修改 DevPanelSettings.asset 中的两个 bool 字段
    private static void SetDevPanelSettings(bool isEnabledValue, bool isForceToVideoValue)
    {
        // 确保资源文件存在
        if (!File.Exists(DevPanelSettingsPath))
        {
            Debug.LogError($"DevPanelSettings.asset not found at path: {DevPanelSettingsPath}");
            return;
        }

        // 加载资源
        var settingsAsset = AssetDatabase.LoadAssetAtPath<Watermelon.DevPanelSettings>(DevPanelSettingsPath);
        if (settingsAsset == null)
        {
            Debug.LogError($"Failed to load DevPanelSettings.asset from {DevPanelSettingsPath}");
            return;
        }

        // 使用 SerializedObject 修改私有序列化字段
        SerializedObject so = new SerializedObject(settingsAsset);

        // 注意：字段名是 isEnabled 和 isForceToVideo（都是私有的，但通过 SerializedObject 可以访问）
        SerializedProperty isEnabledProp = so.FindProperty("isEnabled");
        SerializedProperty isForceToVideoProp = so.FindProperty("isForceToVideo");

        bool hasError = false;

        if (isEnabledProp != null)
        {
            if (isEnabledProp.propertyType == SerializedPropertyType.Boolean)
            {
                isEnabledProp.boolValue = isEnabledValue;
                Debug.Log($"Set isEnabled to {isEnabledValue}");
            }
            else
            {
                Debug.LogError($"isEnabled field type mismatch. Expected bool, got {isEnabledProp.propertyType}");
                hasError = true;
            }
        }
        else
        {
            Debug.LogError("isEnabled field not found in DevPanelSettings.asset. Make sure the field name is exactly 'isEnabled'");
            hasError = true;
        }

        if (isForceToVideoProp != null)
        {
            if (isForceToVideoProp.propertyType == SerializedPropertyType.Boolean)
            {
                isForceToVideoProp.boolValue = isForceToVideoValue;
                Debug.Log($"Set isForceToVideo to {isForceToVideoValue}");
            }
            else
            {
                Debug.LogError($"isForceToVideo field type mismatch. Expected bool, got {isForceToVideoProp.propertyType}");
                hasError = true;
            }
        }
        else
        {
            Debug.LogError("isForceToVideo field not found in DevPanelSettings.asset. Make sure the field name is exactly 'isForceToVideo'");
            hasError = true;
        }

        if (!hasError)
        {
            // 应用修改并保存
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settingsAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"DevPanelSettings updated successfully: isEnabled={isEnabledValue}, isForceToVideo={isForceToVideoValue}");
        }
        else
        {
            Debug.LogError("Failed to update DevPanelSettings due to field errors");
        }
    }

    private static void SetMonetizationSettings(bool isEnabledLog)
    {
        // 确保资源文件存在
        if (!File.Exists(MonetizationSettingsPath))
        {
            Debug.LogError($"MonetizationSettings.asset not found at path: {MonetizationSettingsPath}");
            return;
        }

        // 加载资源
        var settingsAsset = AssetDatabase.LoadAssetAtPath<Watermelon.MonetizationSettings>(MonetizationSettingsPath);
        if (settingsAsset == null)
        {
            Debug.LogError($"Failed to load MonetizationSettings.asset from {MonetizationSettingsPath}");
            return;
        }

        // 使用 SerializedObject 修改私有序列化字段
        SerializedObject so = new SerializedObject(settingsAsset);

        // 注意：字段名是 isEnabled 和 isForceToVideo（都是私有的，但通过 SerializedObject 可以访问）
        SerializedProperty isEnabledProp = so.FindProperty("verboseLogging");

        bool hasError = false;

        if (isEnabledProp != null)
        {
            if (isEnabledProp.propertyType == SerializedPropertyType.Boolean)
            {
                isEnabledProp.boolValue = isEnabledLog;
                Debug.Log($"Set verboseLogging to {isEnabledLog}");
            }
            else
            {
                Debug.LogError($"verboseLogging field type mismatch. Expected bool, got {isEnabledProp.propertyType}");
                hasError = true;
            }
        }
        else
        {
            Debug.LogError("verboseLogging field not found in MonetizationSettings.asset. Make sure the field name is exactly 'verboseLogging'");
            hasError = true;
        }

        if (!hasError)
        {
            // 应用修改并保存
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(settingsAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"MonetizationSettings updated successfully: verboseLogging={isEnabledLog}");
        }
        else
        {
            Debug.LogError("Failed to update MonetizationSettings due to field errors");
        }
    }

    // 可选：添加菜单状态验证（显示当前激活的模式）
    [MenuItem("BuildTools/Build Mode/Switch to Test Mode", true)]
    public static bool ValidateTestMode()
    {
        Menu.SetChecked("BuildTools/Build Mode/Switch to Test Mode", IsDefineSet(TEST_MODE_DEFINE));
        return true;
    }

    [MenuItem("BuildTools/Build Mode/Switch to Release Mode", true)]
    public static bool ValidateReleaseMode()
    {
        Menu.SetChecked("BuildTools/Build Mode/Switch to Release Mode", IsDefineSet(RELEASE_MODE_DEFINE));
        return true;
    }

    private static bool IsDefineSet(string symbol)
    {
        BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (string.IsNullOrEmpty(defines)) return false;
        string[] allDefines = defines.Split(';');
        return System.Array.IndexOf(allDefines, symbol) >= 0;
    }




}
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using System.Globalization;

public static class ChangeBundleVersionCode
{
    [MenuItem("BuildTools/Bundle Version Code")]
    public static void SChangeBundleVersionCode()
    {
        var versionList = Application.version.Split('.').Select(a =>
        {
            return int.TryParse(a, out var r) ? r : 0;
        }).ToArray();
#if TEST_MODE
        PlayerSettings.bundleVersion = $"{versionList[0]}.{versionList[1]}.{versionList[2]}.{DateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)}";
#else
        PlayerSettings.bundleVersion = $"{versionList[0]}.{versionList[1]}.{versionList[2]}";
#endif
        var versionCode = versionList[0] * 100000 + versionList[1] * 1000 + versionList[2] * 10;

#if UNITY_ANDROID
        PlayerSettings.Android.bundleVersionCode = versionCode;
#elif UNITY_IOS
            PlayerSettings.iOS.buildNumber = versionCode.ToString();
#endif

    }
}
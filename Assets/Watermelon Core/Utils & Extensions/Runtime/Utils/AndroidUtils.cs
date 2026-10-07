using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AndroidUtils
{
    /// <summary>
    /// 禁止截屏
    /// </summary>
    public static void EnableSecure()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer =
               new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject window =
                activity.Call<AndroidJavaObject>("getWindow");

            AndroidJavaClass layoutParams =
                new AndroidJavaClass("android.view.WindowManager$LayoutParams");

            int FLAG_SECURE =
                layoutParams.GetStatic<int>("FLAG_SECURE");

            window.Call("addFlags", FLAG_SECURE);
        }
#endif
    }
    public static string GetCountryCode()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
            using var localeClass = new AndroidJavaClass("java.util.Locale");
            using var locale = localeClass.CallStatic<AndroidJavaObject>("getDefault");

            return locale.Call<string>("getCountry");
#else
        // 编辑器或其他平台使用备用方案
        return System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;
#endif
    }

}

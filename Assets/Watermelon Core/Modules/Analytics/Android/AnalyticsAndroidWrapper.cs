
#if UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Watermelon
{
    public static class AnalyticsAndroidWrapper
    {

        public static int GetSimState()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

        using var unityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        using var activity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        using var simUtil =
            new AndroidJavaClass("com.my.helper.AndroidUtil");

        return simUtil.CallStatic<int>("getSimState", activity);

#else
            return 0;
#endif
        }

        public static string GetGAID()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

        using var unityPlayer =
            new AndroidJavaClass("com.unity3d.player.UnityPlayer");

        using var activity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        using var util =
            new AndroidJavaClass("com.my.helper.AndroidUtil");

        return util.CallStatic<string>("getGAID", activity);

#else
            return "";
#endif
        }

        public static bool IsVPNActive()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            using (AndroidJavaClass unityPlayer =
                   new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity =
                       unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (AndroidJavaClass vpnUtil =
                           new AndroidJavaClass("com.my.helper.AndroidUtil"))
                    {
                        return vpnUtil.CallStatic<bool>(
                            "isVpnActive",
                            activity
                        );
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            return false;
        }
#else
            return false;
#endif
        }

    }
}
#endif
using System.Collections;
using System.Collections.Generic;
using Firebase.RemoteConfig;
using UnityEngine;
using Watermelon;
namespace FirebaseRemote
{
    public class ServerRemoteMgr : MonoBehaviour
    {
        public static ServerRemoteMgr Instance = null;

        public static ServerRemoteMgr Create()
        {
            GameObject gameObject = new GameObject("Remote Server");
            return gameObject.AddComponent<ServerRemoteMgr>();
        }

        private FirebaseWrapper firebaseRemoteWrap;
        private bool isRemoteGet = false;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }
            isRemoteGet = false;
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Start is called before the first frame update
        void Start()
        {
#if !UNITY_EDITOR
            firebaseRemoteWrap = gameObject.AddComponent<FirebaseWrapper>();
            firebaseRemoteWrap.remoteGetAction += () => { OnRemoteGet(); };
#endif
        }
        private void OnRemoteGet()
        {
            // isRemoteGet = true;
            // Write_InterstitialShowingDelay();
            // Write_ForceToA();
            StartCoroutine(YiedToMainThead());
        }

        IEnumerator YiedToMainThead()
        {
            isRemoteGet = true;

            yield return null;

            // Write_InterstitialShowingDelay();
            // Write_ForceToA();
            // Write_VideoListAB_IsA();
        }

        private void Write_InterstitialShowingDelay()
        {
            try
            {
                var stringtim = FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteKey.InterTimeDelay).LongValue;
                Debug.Log("[FirebaseWrapper] Remote Interstitial Showing Delay: " + stringtim);

                Watermelon.AdsManager.intShowingDelay = (int)stringtim;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[FirebaseWrapper] Failed to get Interstitial Showing Delay: " + ex.Message);
            }
        }
        private void Write_ForceToA()
        {
            try
            {
                var forceToA = FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteKey.IFForceToA).BooleanValue;
                Debug.Log("[FirebaseWrapper] Remote Force To A: " + forceToA);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[FirebaseWrapper] Failed to get Force To A: " + ex.Message);
            }
        }


        public int Remote_InterstitialShowingDelay()
        {
            if (isRemoteGet)
            {
                try
                {
                    var stringtim = FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteKey.InterTimeDelay).LongValue;
                    return (int)stringtim;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[FirebaseWrapper] Failed to get Interstitial Showing Delay: " + ex.Message);
                    return 60;
                }
            }
            return 60;
        }

        // public bool Remote_ForceToA()
        // {
        //     if (isRemoteGet)
        //     {
        //         try
        //         {
        //             return FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteKey.IFForceToA).BooleanValue;
        //         }
        //         catch (System.Exception ex)
        //         {
        //             Debug.LogError("[FirebaseWrapper] Failed to get Force To A: " + ex.Message);
        //             return true; // Return default value in case of error
        //         }
        //     }
        //     return true;
        // }

        public int Remote_ShowInterstitialMinLevel()
        {
            if (isRemoteGet)
            {
                try
                {
                    return (int)FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteKey.InterAdMinLevel).LongValue;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[FirebaseWrapper] Failed to get Interstitial Minimum Level: " + ex.Message);
                    return 5; // Return default value in case of error
                }
            }
            return 5;
        }



        #region 使用云函数替换的

        private bool? IsA = null;
        public void SetForceADataByWeb(bool isA)
        {
            IsA = isA;
        }
        private string key = "a7b3c9e2f4d1a5c8b6e9f0d4c2a1";
        public bool GetAB_VideoIsB()
        {
            // 上次进游戏走的是0:A, 1:B ,只要走过B那就是B
            var lastTimeEnterGo = PlayerPrefs.GetInt(key, 0);
            if (lastTimeEnterGo == 1)
            {
                Watermelon.AnalyticsController.OnUserPerportyString(Watermelon.AnalyticsEventType.user_video_b, true.ToString());

                FirebaseAnalyticsModule.Instance.SetUserProperty(FirebaseAnalyticsModule.UserPropertyType.user_video_b, true.ToString());

                // FirebaseAnalyticsModule.Instance.SendUserBSetEvent();
                Debug.Log("[FirebaseWrapper] GetAB_VideoIsB 1 true");
                return true;
            }

            // 凡是AdjustOrganic 或者没连上网 或者force为A的情况 都不走视频
            string enterType = PlayerPrefs.GetString(Watermelon.AnalyticsEventType.ad_network.ToString(), Watermelon.AdjustAnalyticsModule.AdjustOrganic);
            if (string.Equals(enterType, Watermelon.AdjustAnalyticsModule.AdjustOrganic, System.StringComparison.OrdinalIgnoreCase)
                || IsA == null || IsA.Value == true)
            {
                Watermelon.AnalyticsController.OnUserPerportyString(Watermelon.AnalyticsEventType.user_video_b, false.ToString());

                FirebaseAnalyticsModule.Instance.SetUserProperty(FirebaseAnalyticsModule.UserPropertyType.user_video_b, false.ToString());
                Debug.Log("[FirebaseWrapper] GetAB_VideoIsB 2 false");
                return false;
            }
            else
            {
                Debug.Log("[FirebaseWrapper] GetAB_VideoIsB 3 true");
                Watermelon.AnalyticsController.OnUserPerportyString(Watermelon.AnalyticsEventType.user_video_b, true.ToString());

                FirebaseAnalyticsModule.Instance.SetUserProperty(FirebaseAnalyticsModule.UserPropertyType.user_video_b, true.ToString());

                // FirebaseAnalyticsModule.Instance.SendUserBSetEvent();

                PlayerPrefs.SetInt(key, 1);
                return true;
            }

        }

        #endregion

    }
}

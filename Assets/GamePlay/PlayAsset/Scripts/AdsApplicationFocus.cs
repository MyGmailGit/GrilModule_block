using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

public class AdsApplicationFocus : MonoBehaviour
{
    private static AdsApplicationFocus instance;
    public static AdsApplicationFocus Instance => instance;
    // public static void Create()
    // {
    //     if (instance != null) return;

    //     var obj = new GameObject("GameNotDestory");
    //     instance = obj.AddComponent<GameNotDestory>();
    // }

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationFocus(bool focus)
    {
        /*
        if (focus)
        {
#if MODULE_MONETIZATION //&& !UNITY_EDITOR
            if (AdsManager.isEnterAdShow) return;
            Watermelon.AdsManager.ShowInterstitial((result) =>
            {
                if (result)
                {
                }
                else
                {
                }
            }, Watermelon.AnalyticsStr.inter_application_focus);
#endif

        }
        else
        {
            // Game lost focus - pause automatically
            // PauseGame();
        }
        */
    }

}

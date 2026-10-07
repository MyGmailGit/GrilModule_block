using System;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [StaticUnload]
    public static class AnalyticsController
    {
        public static event AnalyticsEventCallback EventFired;

        private static void AddUserPerportyInt(AnalyticsEventType analyticsEventType, int addValue)
        {
            var value_ads_amout = PlayerPrefs.GetInt(AnalyticsEventType.ads_amout.ToString(), 0);
            var value_rewardvideo_amout = PlayerPrefs.GetInt(AnalyticsEventType.rewardvideo_amout.ToString(), 0);
            var value_interstitial_amout = PlayerPrefs.GetInt(AnalyticsEventType.interstitial_amout.ToString(), 0);

            if (analyticsEventType == AnalyticsEventType.InterstitialDisplayed)
            {
                value_interstitial_amout += addValue;
                OnUserPerportyInt(AnalyticsEventType.InterstitialDisplayed, value_interstitial_amout);
            }
            else if (analyticsEventType == AnalyticsEventType.RVDisplayed)
            {
                value_rewardvideo_amout += addValue;
                OnUserPerportyInt(AnalyticsEventType.RVDisplayed, value_rewardvideo_amout);
            }

            value_ads_amout = value_rewardvideo_amout + value_interstitial_amout;

            OnUserPerportyInt(AnalyticsEventType.ads_amout, value_ads_amout);

            PlayerPrefs.SetInt(AnalyticsEventType.ads_amout.ToString(), value_ads_amout);
            PlayerPrefs.SetInt(AnalyticsEventType.rewardvideo_amout.ToString(), value_rewardvideo_amout);
            PlayerPrefs.SetInt(AnalyticsEventType.interstitial_amout.ToString(), value_interstitial_amout);

        }

        public static void OnUserPerportyInt(AnalyticsEventType analyticsEventType, int value)
        {
            UserPropertyInt userPropertyInt = new UserPropertyInt() { count = value };
            TrackEvent(analyticsEventType, userPropertyInt);
        }
        public static void OnUserPerportyString(AnalyticsEventType analyticsEventType, string value)
        {
            UserPropertyString userPropertyInt = new UserPropertyString() { data = value };
            TrackEvent(analyticsEventType, userPropertyInt);
        }
        // inters AD
        public static void OnInterstitialDisplayed()//string source)
        {
            // AnalyticsInterstitialData analyticsData = new AnalyticsInterstitialData();
            // analyticsData.Source = source;
            // TrackEvent(AnalyticsEventType.InterstitialDisplayed, analyticsData);

            // 记录user Perporty
            AddUserPerportyInt(AnalyticsEventType.InterstitialDisplayed, 1);
        }
        // reward AD
        public static void OnRVDisplayed()//string source)
        {
            // AnalyticsRVData analyticsData = new AnalyticsRVData();
            // analyticsData.Source = source;
            // TrackEvent(AnalyticsEventType.RVDisplayed, analyticsData);

            // 记录user Perporty
            AddUserPerportyInt(AnalyticsEventType.RVDisplayed, 1);
        }

        public static void OnLoadingEnd()
        {
            AnalyticsLoadingEndData analyticsData = new AnalyticsLoadingEndData();
            analyticsData.deltaTime = (int)Time.realtimeSinceStartup;

            TrackEvent(AnalyticsEventType.loading_end, analyticsData);
        }

        public static void OnAdRevenue(string countryCode, double revenue, string networkName, string adUnitId, string adFormat, string mediation, int networkfirmid, string placement, string adsource)
        {
            AnalyticsAdRevenueData analyticsAdRevenueData = new AnalyticsAdRevenueData();
            analyticsAdRevenueData.countryCode = countryCode;
            analyticsAdRevenueData.revenue = revenue;
            analyticsAdRevenueData.networkName = networkName;
            analyticsAdRevenueData.adUnitId = adUnitId;
            analyticsAdRevenueData.adFormat = adFormat;
            analyticsAdRevenueData.mediation = mediation;
            analyticsAdRevenueData.networkfirmid = networkfirmid;
            analyticsAdRevenueData.placement = placement;
            analyticsAdRevenueData.adsource = adsource;

            TrackEvent(AnalyticsEventType.ad_revenue, analyticsAdRevenueData);
        }

        public static void OnAdMaxTrack(string step, string adtype, string fromArg, string errorCode, string networkName)
        {
            AnalyticsAdMaxTrackData analyticsAdMaxTrackData = new AnalyticsAdMaxTrackData();
            analyticsAdMaxTrackData.step = step;
            analyticsAdMaxTrackData.adtype = adtype;
            analyticsAdMaxTrackData.fromArg = fromArg;
            analyticsAdMaxTrackData.errorCode = errorCode;
            analyticsAdMaxTrackData.networkName = networkName;
            TrackEvent(AnalyticsEventType.MaxTrack, analyticsAdMaxTrackData);
        }
        public static void OnGameLevelChange(string level)
        {
            AnalyticsGameLevelChangeData analyticsData = new AnalyticsGameLevelChangeData();
            analyticsData.level = level;
            TrackEvent(AnalyticsEventType.GameLevel, analyticsData);
        }

        public static void OnHttpErrorChange(string action_name, string res_code, string res_msg)
        {
            AnalyticsHttpErrorData analyticsData = new AnalyticsHttpErrorData();
            analyticsData.action_name = action_name;
            analyticsData.res_code = res_code;
            analyticsData.res_msg = res_msg;

            TrackEvent(AnalyticsEventType.http_error, analyticsData);
        }


        public static void OnAdNetworkVideoChange(string ad_network)
        {
            AnalyticsAdNetworkVideoData analyticsData = new AnalyticsAdNetworkVideoData();
            analyticsData.ad_network = ad_network;
            TrackEvent(AnalyticsEventType.ad_network_video, analyticsData);
        }


        public static void OnIapEventChange(IAPStatus status, string product_id, string failure_reason, string isoCurrencyCode, float localizedPrice, string token = null)
        {
            AnalyticsIAPEventTrackData analyticsData = new AnalyticsIAPEventTrackData();
            analyticsData.status = status;
            analyticsData.product_id = product_id;
            analyticsData.failure_reason = failure_reason;
            analyticsData.isoCurrencyCode = isoCurrencyCode;
            analyticsData.localizedPrice = localizedPrice;
            analyticsData.token = token;
            TrackEvent(AnalyticsEventType.iap_event, analyticsData);
        }

        public static void OnAdjAttributionGet(float startupTimeDelta, string ad_network)
        {
            AnalyticsAdjAttributionGetData analyticsData = new AnalyticsAdjAttributionGetData();
            analyticsData.time_startup = startupTimeDelta.ToString();
            analyticsData.ad_network = ad_network;
            TrackEvent(AnalyticsEventType.adj_attribution_get, analyticsData);
        }

        // public static void OnCurrencySource(string source, Dictionary<CurrencyType, int> currenciesDelta)
        // {
        //     AnalyticsCurrencyData analyticsCurrencyData = new AnalyticsCurrencyData();
        //     analyticsCurrencyData.Source = source;
        //     analyticsCurrencyData.CurrenciesDelta = currenciesDelta;

        //     TrackEvent(AnalyticsEventType.CurrencySource, analyticsCurrencyData);
        // }

        // public static void OnCurrencySink(string sink, Dictionary<CurrencyType, int> currenciesDelta)
        // {
        //     AnalyticsCurrencyData analyticsCurrencyData = new AnalyticsCurrencyData();
        //     analyticsCurrencyData.Source = sink;
        //     analyticsCurrencyData.CurrenciesDelta = currenciesDelta;

        //     TrackEvent(AnalyticsEventType.CurrencySink, analyticsCurrencyData);
        // }

        // public static void OnIAPClicked(IAPItem item)
        // {
        //     AnalyticsIAPData analyticsData = new AnalyticsIAPData();
        //     analyticsData.Item = item;

        //     TrackEvent(AnalyticsEventType.IAPClicked, analyticsData);
        // }



#if MODULE_IAP
        // public static void OnIAPPurchased(AnalyticsIAPData analyticsIAPData)
        // {
        //     // TrackEvent(AnalyticsEventType.IAPPurchased, analyticsIAPData);
        // }

        // public static void OnIAPFailed(IAPItem item, Watermelon.PurchaseFailureReason failureReason)
        // {
        //     AnalyticsIAPFailData analyticsData = new AnalyticsIAPFailData();
        //     analyticsData.Item = item;
        //     analyticsData.FailureReason = failureReason;

        //     // TrackEvent(AnalyticsEventType.IAPFailed, analyticsData);
        // }
#endif

        public static void TrackEvent(AnalyticsEventType analyticsEventType, IAnalyticsEventData eventData = null)
        {
#if TEST_MODE
            Debug.Log(string.Format("[Analytics]: Event <b>\"{0}\"</b> fired.", analyticsEventType));
#endif

            EventFired?.Invoke(analyticsEventType, eventData);
        }

        private static void UnloadStatic()
        {
            EventFired = null;
        }

        public delegate void AnalyticsEventCallback(AnalyticsEventType type, IAnalyticsEventData analyticsEventData);
    }
}

using System;
using System.Collections.Generic;
using ThinkingData.Analytics;
using UnityEngine;

namespace Watermelon
{
    public class ThinkingDataAnalyticsModule : BaseAnalyticsModule
    {
        private const string AppId = "591b324c7132436fba844a1a8d432158";
        private const string ServerUrl = "https://global-receiver-ta.thinkingdata.cn";
        private const string AppCodeGP = "gp@LT007";

        private readonly Dictionary<string, object> dataKeyValue = new Dictionary<string, object>();

        public override Dictionary<AnalyticsEventType, Action<IAnalyticsEventData>> GetHandlers()
        {
            return new Dictionary<AnalyticsEventType, Action<IAnalyticsEventData>>
            {
                { AnalyticsEventType.ads_amout, data => TrackUserProperties(AnalyticsEventType.ads_amout.ToString(), (UserPropertyInt)data) },
                { AnalyticsEventType.rewardvideo_amout, data => TrackUserProperties(AnalyticsEventType.rewardvideo_amout.ToString(), (UserPropertyInt)data) },
                { AnalyticsEventType.interstitial_amout, data => TrackUserProperties(AnalyticsEventType.interstitial_amout.ToString(), (UserPropertyInt)data) },

                { AnalyticsEventType.ad_network, data => TrackUserProperties(AnalyticsEventType.ad_network.ToString(), (UserPropertyString)data) },
                { AnalyticsEventType.campaign_id, data => TrackUserProperties(AnalyticsEventType.campaign_id.ToString(), (UserPropertyString)data) },
                { AnalyticsEventType.campaign_name, data => TrackUserProperties(AnalyticsEventType.campaign_name.ToString(), (UserPropertyString)data) },
                { AnalyticsEventType.creative_name, data => TrackUserProperties(AnalyticsEventType.creative_name.ToString(), (UserPropertyString)data) },
                { AnalyticsEventType.site_id, data => TrackUserProperties(AnalyticsEventType.site_id.ToString(), (UserPropertyString)data) },

                { AnalyticsEventType.loading_end, data => TrackLoadingEndEvent((AnalyticsLoadingEndData)data) },
                { AnalyticsEventType.ad_revenue, data => TrackAdRevenueEvent((AnalyticsAdRevenueData)data) },
                { AnalyticsEventType.MaxTrack, data => TrackAdMaxTrackEvent((AnalyticsAdMaxTrackData)data) },

                { AnalyticsEventType.GameLevel, data => TrackGameLevelEvent((AnalyticsGameLevelChangeData)data) },

                { AnalyticsEventType.http_error, data => TrackHttpErrorChangeEvent((AnalyticsHttpErrorData)data) },

                { AnalyticsEventType.ad_network_video, data => TrackAdNetworkVideoEvent((AnalyticsAdNetworkVideoData)data) },

                { AnalyticsEventType.iap_event, data => TrackIAPEventEvent((AnalyticsIAPEventTrackData)data) },

                { AnalyticsEventType.adj_attribution_get, data => TrackAdjAttributionGetEvent((AnalyticsAdjAttributionGetData)data) }


            // { AnalyticsEventType.CurrencySource, data => TrackCurrencyEvent("currency_source", (AnalyticsCurrencyData)data) },
            // { AnalyticsEventType.CurrencySink, data => TrackCurrencyEvent("currency_sink", (AnalyticsCurrencyData)data) },
            // { AnalyticsEventType.IAPClicked, data => TrackIAPClick((AnalyticsIAPData)data) },
            // { AnalyticsEventType.IAPPurchased, data => TrackIAPPurchase((AnalyticsIAPData)data) },
            // { AnalyticsEventType.IAPFailed, data => TrackIAPFailed((AnalyticsIAPFailData)data) },
            // { AnalyticsEventType.IAPFirstPurchase, data => TrackCustomEvent("iap_first_purchase", null) },
            // { AnalyticsEventType.AdFreePeriodExpired, data => TrackCustomEvent("ad_free_period_expired", null) },
            // { AnalyticsEventType.RVClicked, data => TrackAdEvent("rewarded_video_click", (AnalyticsRVData)data) },
            // { AnalyticsEventType.InterstitialDisplayed, data => TrackAdEvent("interstitial_displayed", (AnalyticsIntData)data) },
        };
        }

        public override void OnInitialized()
        {
#if MODULE_THINKINGDATA
            InitializeThinkingData();
#else
            Debug.Log("[ThinkingDataAnalyticsModule]: THINKINGDATA define not found. ThinkingData will not be initialized.");
#endif
        }

#if MODULE_THINKINGDATA
        private void InitializeThinkingData()
        {

            TDAnalytics.SetSuperProperties(new Dictionary<string, object>
            {
#if UNITY_ANDROID
                { "appcode", AppCodeGP},
#elif UNITY_IOS
                // { "appcode", AppCodeGP},
#endif
            });

            dataKeyValue.Clear();
            // dataKeyValue.Add("#distinct_id", TDAnalytics.GetDistinctId());

            dataKeyValue.Add("deviceid", TDAnalytics.GetDeviceId() ?? string.Empty);

            dataKeyValue.Add("isShieldUser", false);
#if UNITY_ANDROID
            dataKeyValue.Add("isVpnUser", AnalyticsAndroidWrapper.IsVPNActive());
#elif UNITY_IOS
#endif

#if UNITY_ANDROID
            dataKeyValue.Add("simState", AnalyticsAndroidWrapper.GetSimState() == 1);
#elif UNITY_IOS
#endif

#if UNITY_ANDROID
            dataKeyValue.Add("appcode", AppCodeGP);
#elif UNITY_IOS
                // { "appcode", AppCodeGP},
#endif
            dataKeyValue.Add(AnalyticsEventType.ads_amout.ToString(), PlayerPrefs.GetInt(AnalyticsEventType.ads_amout.ToString(), 0));
            dataKeyValue.Add(AnalyticsEventType.rewardvideo_amout.ToString(), PlayerPrefs.GetInt(AnalyticsEventType.rewardvideo_amout.ToString(), 0));
            dataKeyValue.Add(AnalyticsEventType.interstitial_amout.ToString(), PlayerPrefs.GetInt(AnalyticsEventType.interstitial_amout.ToString(), 0));

#if UNITY_ANDROID
            string gad = AnalyticsAndroidWrapper.GetGAID();
            if (!string.IsNullOrEmpty(gad))
                dataKeyValue.Add("gaid", gad);
#elif UNITY_IOS
#endif

            dataKeyValue.Add(AnalyticsEventType.ad_network.ToString(), PlayerPrefs.GetString(AnalyticsEventType.ad_network.ToString(), AdjustAnalyticsModule.AdjustOrganic)); //"organic"));
            dataKeyValue.Add(AnalyticsEventType.campaign_id.ToString(), PlayerPrefs.GetString(AnalyticsEventType.campaign_id.ToString(), "unknown"));
            dataKeyValue.Add(AnalyticsEventType.campaign_name.ToString(), PlayerPrefs.GetString(AnalyticsEventType.campaign_name.ToString(), "unknown"));
            dataKeyValue.Add(AnalyticsEventType.creative_name.ToString(), PlayerPrefs.GetString(AnalyticsEventType.creative_name.ToString(), "unknown"));
            dataKeyValue.Add(AnalyticsEventType.site_id.ToString(), PlayerPrefs.GetString(AnalyticsEventType.site_id.ToString(), "unknown"));

            TDAnalytics.UserSet(dataKeyValue);

#if !UNITY_EDITOR

            TDAnalytics.EnableAutoTrack(TDAutoTrackEventType.All);
            TDAnalytics.Init(AppId, ServerUrl);
#endif

            Debug.Log("[ThinkingDataAnalyticsModule]: ThinkingData initialization placeholder executed.");

#if TEST_MODE
            Debug.Log($"[ThinkingDataAnalyticsModule]: UserSet:{SerializeProperties(dataKeyValue)}");
#endif


            TrackCustomEvent(AnalyticsEventType.loading_start.ToString(), null);
        }
#endif

        private void TrackUserProperties(string propName, UserPropertyInt data)
        {
            dataKeyValue.Clear();
            dataKeyValue.Add(propName, data.count);
            TDAnalytics.UserSet(dataKeyValue);

            Debug.Log($"[ThinkingDataAnalyticsModule] User Property: {propName} with properties: {SerializeProperties(dataKeyValue)}");
            // TDAnalytics.UserSet(new Dictionary<string, object>
            // {
            //     {propName, data.count}
            // });
        }
        private void TrackUserProperties(string propName, UserPropertyString data)
        {
            dataKeyValue.Clear();
            AddString(propName, data.data);
            TDAnalytics.UserSet(dataKeyValue);
            Debug.Log($"[ThinkingDataAnalyticsModule] User Property: {propName} with properties: {SerializeProperties(dataKeyValue)}");
            // TDAnalytics.UserSet(new Dictionary<string, object>
            // {
            //     {propName, data.data}
            // });
        }

        private void AddString(string key, string value)
        {
            dataKeyValue.Add(key, value ?? string.Empty);
        }

        private void TrackLoadingEndEvent(AnalyticsLoadingEndData data)
        {
            dataKeyValue.Clear();
            dataKeyValue.Add("duration", data.deltaTime);

            TrackCustomEvent(AnalyticsEventType.loading_end.ToString(), dataKeyValue);
        }
        private void TrackAdRevenueEvent(AnalyticsAdRevenueData data)
        {
            dataKeyValue.Clear();
            AddString("countryCode", data.countryCode);
            dataKeyValue.Add("revenue", data.revenue);
            AddString("networkName", data.networkName);
            AddString("adUnitId", data.adUnitId);
            AddString("adFormat", data.adFormat);
            AddString("mediation", data.mediation);
            dataKeyValue.Add("networkfirmid", data.networkfirmid);
            AddString("placement", data.placement);
#if UNITY_ANDROID
            dataKeyValue.Add("isvpn", AnalyticsAndroidWrapper.IsVPNActive());
#elif UNITY_IOS
#endif
            AddString("adsource", data.adsource);

            TrackCustomEvent(AnalyticsEventType.ad_revenue.ToString(), dataKeyValue);
        }

        private void TrackAdMaxTrackEvent(AnalyticsAdMaxTrackData data)
        {
            dataKeyValue.Clear();

            AddString("step", data.step);
            AddString("adtype", data.adtype);
            AddString("from", data.fromArg);
            AddString("errorCode", data.errorCode);
            AddString("networkName", data.networkName);

            TrackCustomEvent(AnalyticsEventType.MaxTrack.ToString(), dataKeyValue);
        }

        private void TrackGameLevelEvent(AnalyticsGameLevelChangeData data)
        {
            dataKeyValue.Clear();
            dataKeyValue.Add("level", data.level);

            TrackCustomEvent(AnalyticsEventType.GameLevel.ToString(), dataKeyValue);
            TrackCustomEvent("level_change", dataKeyValue);
        }

        private void TrackHttpErrorChangeEvent(AnalyticsHttpErrorData data)
        {
            dataKeyValue.Clear();
            AddString("action_name", data.action_name);
            AddString("res_code", data.res_code);
            AddString("res_msg", data.res_msg);

            TrackCustomEvent(AnalyticsEventType.http_error.ToString(), dataKeyValue);
        }

        private void TrackAdNetworkVideoEvent(AnalyticsAdNetworkVideoData data)
        {
            dataKeyValue.Clear();
            AddString("ad_network", data.ad_network);

            TrackCustomEvent(AnalyticsEventType.ad_network_video.ToString(), dataKeyValue);
        }
        private void TrackIAPEventEvent(AnalyticsIAPEventTrackData data)
        {
            dataKeyValue.Clear();
            AddString("status", data.status.ToString());
            AddString("product_id", data.product_id);
            AddString("failure_reason", data.failure_reason);

            TrackCustomEvent(AnalyticsEventType.iap_event.ToString(), dataKeyValue);
        }

        private void TrackAdjAttributionGetEvent(AnalyticsAdjAttributionGetData data)
        {
            dataKeyValue.Clear();
            AddString("time_startup", data.time_startup);
            AddString("ad_network", data.ad_network);

            TrackCustomEvent(AnalyticsEventType.adj_attribution_get.ToString(), dataKeyValue);
        }




        private void TrackCustomEvent(string eventName, Dictionary<string, object> properties)
        {
            // properties = properties ?? new Dictionary<string, object>();
            // properties["event_platform"] = Application.platform.ToString();
            // properties["product_name"] = Application.productName;
            // properties["app_version"] = Application.version;

#if MODULE_THINKINGDATA && !UNITY_EDITOR
            if (properties != null)
                TDAnalytics.Track(eventName, properties);
            else
                TDAnalytics.Track(eventName);
#endif
#if TEST_MODE
            Debug.Log($"[ThinkingDataAnalyticsModule] Track event: {eventName} with properties: {SerializeProperties(properties)}");
#endif
        }

        private static string SerializeProperties(Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
                return "{}";

            var entries = new List<string>();
            foreach (var kvp in properties)
            {
                entries.Add($"{kvp.Key}: {kvp.Value}");
            }

            return "{" + string.Join(", ", entries) + "}";
        }
    }
}

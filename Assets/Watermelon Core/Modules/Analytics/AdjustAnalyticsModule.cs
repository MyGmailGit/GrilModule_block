using System;
using System.Collections.Generic;
using UnityEngine;
using ThinkingAnalytics;
using ThinkingData.Analytics;


#if MODULE_ADJUST
using AdjustSdk;
#endif

namespace Watermelon
{
    public class AdjustAnalyticsModule : BaseAnalyticsModule
    {
        private const string AppTokenAndroid = "qfm3idlj9vr4";
        private const string AppTokenIOS = "qfm3idlj9vr4";

        public const string AdjustOrganic = "Organic";

        public static bool IsGetAdjustData { get; private set; } = false;

        private float startupTime;

        public override Dictionary<AnalyticsEventType, Action<IAnalyticsEventData>> GetHandlers()
        {
            return new Dictionary<AnalyticsEventType, Action<IAnalyticsEventData>>
            {
                { AnalyticsEventType.ad_revenue, data => TrackAdRevenueEvent((AnalyticsAdRevenueData)data) },
                { AnalyticsEventType.ads_amout, data => TrackAD_IPUxx((UserPropertyInt)data) },
                { AnalyticsEventType.iap_event, data => TrackIAP_RevenueEvent((AnalyticsIAPEventTrackData)data) },
            };
        }

        public override void OnInitialized()
        {
            IsGetAdjustData = false;

#if MODULE_ADJUST
            InitializeAdjust();
#else
            Debug.Log("[AdjustAnalyticsModule]: MODULE_ADJUST define not found. Adjust will not be initialized.");
#endif
        }

#if MODULE_ADJUST
        private void InitializeAdjust()
        {
            string appToken = Application.platform == RuntimePlatform.IPhonePlayer ? AppTokenIOS : AppTokenAndroid;
            if (string.IsNullOrEmpty(appToken))
            {
                Debug.LogWarning("[AdjustAnalyticsModule] Adjust app token is not set. Skipping Adjust initialization.");
                return;
            }
#if TEST_MODE
            var adjustConfig = new AdjustConfig(appToken, AdjustEnvironment.Sandbox)
#else
            var adjustConfig = new AdjustConfig(appToken, AdjustEnvironment.Production)
#endif
            {
                LogLevel = AdjustLogLevel.Debug,
                IsSendingInBackgroundEnabled = true,
                IsDeferredDeeplinkOpeningEnabled = true,
                AttributionChangedDelegate = OnAttributionChanged,
                EventSuccessDelegate = OnAdjustEventSuccess,
                EventFailureDelegate = OnAdjustEventFailure,
                SessionSuccessDelegate = OnAdjustSessionSuccess,
                SessionFailureDelegate = OnAdjustSessionFailure,
                ExternalDeviceId = TDAnalytics.GetDeviceId(),
            };
            Adjust.AddGlobalCallbackParameter("ta_distinct_id", TDAnalytics.GetDistinctId());
            // Adjust.AddGlobalCallbackParameter("ta_account_id", ThinkingMgr.Ins.ThinkingCount);

            Adjust.InitSdk(adjustConfig);

            Adjust.GetAdid(adid =>
            {
                // _adjustAdid = adid;
                // DataManager.AdjustId = _adjustAdid;
            });
            startupTime = Time.realtimeSinceStartup;
            Debug.Log("[AdjustAnalyticsModule]: Adjust initialized.");
        }

        private void OnAttributionChanged(AdjustAttribution attribution)
        {
            if (attribution == null)
            {
                Debug.Log("[AdjustAnalyticsModule]: Attribution callback received null data.");
                return;
            }
            PlayerPrefs.SetString(AnalyticsEventType.ad_network.ToString(), attribution.Network);
            PlayerPrefs.SetString(AnalyticsEventType.campaign_id.ToString(), attribution.Campaign);
            PlayerPrefs.SetString(AnalyticsEventType.campaign_name.ToString(), attribution.Campaign);
            PlayerPrefs.SetString(AnalyticsEventType.creative_name.ToString(), attribution.Creative);
            PlayerPrefs.SetString(AnalyticsEventType.site_id.ToString(), attribution.Adgroup);

            AnalyticsController.OnUserPerportyString(AnalyticsEventType.ad_network, attribution.Network);
            AnalyticsController.OnUserPerportyString(AnalyticsEventType.campaign_id, attribution.Campaign);
            AnalyticsController.OnUserPerportyString(AnalyticsEventType.campaign_name, attribution.Campaign);
            AnalyticsController.OnUserPerportyString(AnalyticsEventType.creative_name, attribution.Creative);
            AnalyticsController.OnUserPerportyString(AnalyticsEventType.site_id, attribution.Adgroup);

            IsGetAdjustData = true;

            AnalyticsController.OnAdjAttributionGet(Time.realtimeSinceStartup - startupTime, attribution.Network);

            Debug.Log($"[AdjustAnalyticsModule] Attribution changed: network={attribution.Network}, campaign={attribution.Campaign}, tracker={attribution.TrackerName}, adgroup={attribution.Adgroup}, creative={attribution.Creative}");
        }

        private void OnAdjustEventSuccess(AdjustEventSuccess eventSuccess)
        {
            Debug.Log($"[AdjustAnalyticsModule] Event success: token={eventSuccess.EventToken}, message={eventSuccess.Message}");
        }

        private void OnAdjustEventFailure(AdjustEventFailure eventFailure)
        {
            Debug.Log($"[AdjustAnalyticsModule] Event failure: token={eventFailure.EventToken}, message={eventFailure.Message}, willRetry={eventFailure.WillRetry}");
        }

        private void OnAdjustSessionSuccess(AdjustSessionSuccess sessionSuccess)
        {
            Debug.Log($"[AdjustAnalyticsModule] Session success: message={sessionSuccess.Message}");
        }

        private void OnAdjustSessionFailure(AdjustSessionFailure sessionFailure)
        {
            Debug.Log($"[AdjustAnalyticsModule] Session failure: message={sessionFailure.Message}, willRetry={sessionFailure.WillRetry}");
        }

        private void TrackAdRevenueEvent(AnalyticsAdRevenueData data)
        {
            AdjustAdRevenue adjustAdRevenue = new AdjustAdRevenue("admob");
            adjustAdRevenue.SetRevenue(data.revenue, "USD");
            adjustAdRevenue.AdImpressionsCount = 1;
            adjustAdRevenue.AdRevenueNetwork = data.networkName ?? string.Empty;
            adjustAdRevenue.AdRevenueUnit = data.adUnitId ?? string.Empty;
            adjustAdRevenue.AdRevenuePlacement = data.placement;

            Adjust.TrackAdRevenue(adjustAdRevenue);

            Debug.Log($"[AdjustAnalyticsModule]: AdRevenue:{data}");
        }

        private void TrackAD_IPUxx(UserPropertyInt data)
        {
            // "ipu2", "ipu3", "ipu5", "ipu10"
            // if (data.count == 2)
            // {
            //     AdjustEvent adjustEvent = new AdjustEvent("ipu2");
            //     Adjust.TrackEvent(adjustEvent);
            // }
            // else 
            if (data.count == 3)
            {
                AdjustEvent adjustEvent = new AdjustEvent("jf659u");
                Adjust.TrackEvent(adjustEvent);
            }
            else if (data.count == 5)
            {
                AdjustEvent adjustEvent = new AdjustEvent("pu53q5");
                Adjust.TrackEvent(adjustEvent);
            }
            else if (data.count == 10)
            {
                AdjustEvent adjustEvent = new AdjustEvent("8gttm5");
                Adjust.TrackEvent(adjustEvent);
            }

            Debug.Log($"[AdjustAnalyticsModule] TrackAD_IPUxx: {data.count}");
        }

        private void TrackIAP_RevenueEvent(AnalyticsIAPEventTrackData data)
        {
            if (data.status == IAPStatus.success)
            {
                IAPItem item = IAPManager.GetIAPItem(data.product_id);
                // 不上报订阅成功的收入
                if (item.ProductType == ProductType.Subscription) return;

                AdjustEvent adjustEvent = new AdjustEvent("gpLT007");
                adjustEvent.SetRevenue(data.localizedPrice, data.isoCurrencyCode);
                adjustEvent.ProductId = data.product_id;

                Adjust.TrackEvent(adjustEvent);

                //                 if (string.IsNullOrEmpty(data.token))
                //                 {
                //                     Adjust.TrackEvent(adjustEvent);
                //                 }
                //                 else
                //                 {
                //                     adjustEvent.PurchaseToken = data.token;
                // #if TEST_MODE
                //                     Debug.Log("[AdjustAnalyticsModule] TrackIAP_RevenueEvent with token: " + data.token);
                // #endif
                //                     Adjust.VerifyAndTrackPlayStorePurchase(adjustEvent, verificationResult =>
                //                     {
                // #if TEST_MODE
                //                         Debug.Log("[AdjustAnalyticsModule] VerifyAndTrackPlayStorePurchase Verification status: " + verificationResult.VerificationStatus);
                //                         Debug.Log("[AdjustAnalyticsModule] VerifyAndTrackPlayStorePurchase Code: " + verificationResult.Code);
                //                         Debug.Log("[AdjustAnalyticsModule] VerifyAndTrackPlayStorePurchase Message: " + verificationResult.Message);
                // #endif
                //                     });
                //                 }
#if TEST_MODE
                Debug.Log($"[AdjustAnalyticsModule] TrackIAP_RevenueEvent: {data}");
#endif
            }
        }

        // private void TrackAdjustCurrencyEvent(AnalyticsEventType eventType, AnalyticsCurrencyData data)
        // {
        //     if (data == null)
        //         return;

        //     TrackAdjustEvent(eventType, new Dictionary<string, string>
        //     {
        //         { "source", data.Source ?? string.Empty },
        //         { "currency_delta", SerializeCurrencyDelta(data.CurrenciesDelta) }
        //     });
        // }

        // private void TrackAdjustIAPEvent(AnalyticsEventType eventType, AnalyticsIAPData data)
        // {
        //     if (data == null)
        //         return;

        //     var partnerParameters = new Dictionary<string, string>
        //     {
        //         { "item_id", data.Item?.ID ?? string.Empty },
        //         { "product_type", data.Item?.ProductType.ToString() ?? string.Empty },
        //         { "receipt", data.Receipt ?? string.Empty },
        //         { "localized_price", data.LocalizedPrice.ToString() },
        //         { "currency_code", data.IsoCurrencyCode ?? string.Empty }
        //     };

        //     if (eventType == AnalyticsEventType.IAPPurchased && data.LocalizedPrice > 0f)
        //     {
        //         TrackAdjustEvent(eventType, partnerParameters, data.LocalizedPrice, data.IsoCurrencyCode);
        //     }
        //     else
        //     {
        //         TrackAdjustEvent(eventType, partnerParameters);
        //     }
        // }

        // private void TrackAdjustIAPFailed(AnalyticsIAPFailData data)
        // {
        //     if (data == null)
        //         return;

        //     TrackAdjustEvent(AnalyticsEventType.IAPFailed, new Dictionary<string, string>
        //     {
        //         { "item_id", data.Item?.ID ?? string.Empty },
        //         { "product_type", data.Item?.ProductType.ToString() ?? string.Empty },
        //         { "failure_reason", data.FailureReason.ToString() }
        //     });
        // }

        // private void TrackAdjustAdEvent(AnalyticsEventType eventType, IAnalyticsEventData data)
        // {
        //     var source = string.Empty;
        //     switch (data)
        //     {
        //         case AnalyticsRVData rvData:
        //             source = rvData.Source;
        //             break;
        //         case AnalyticsInterstitialData intData:
        //             source = intData.Source;
        //             break;
        //     }

        //     TrackAdjustEvent(eventType, new Dictionary<string, string>
        //     {
        //         { "source", source ?? string.Empty }
        //     });
        // }

        private void TrackAdjustEvent(AnalyticsEventType eventType, Dictionary<string, string> partnerParameters, double? revenue = null, string currency = null)
        {
            // string eventToken = GetAdjustEventToken(eventType);
            // if (string.IsNullOrEmpty(eventToken))
            // {
            //     Debug.LogWarning($"[AdjustAnalyticsModule] Missing Adjust event token for event type: {eventType}");
            //     return;
            // }

            // var adjustEvent = new AdjustEvent(eventToken);

            // if (revenue.HasValue && !string.IsNullOrEmpty(currency))
            // {
            //     adjustEvent.SetRevenue(revenue.Value, currency);
            // }

            // if (partnerParameters != null)
            // {
            //     foreach (var parameter in partnerParameters)
            //     {
            //         if (!string.IsNullOrEmpty(parameter.Key) && parameter.Value != null)
            //         {
            //             adjustEvent.AddPartnerParameter(parameter.Key, parameter.Value);
            //         }
            //     }
            // }

            // Adjust.TrackEvent(adjustEvent);
        }

        // private string GetAdjustEventToken(AnalyticsEventType eventType)
        // {
        //     EventTokens.TryGetValue(eventType, out string token);
        //     return token;
        // }

        private static string SerializeCurrencyDelta(Dictionary<CurrencyType, int> currenciesDelta)
        {
            if (currenciesDelta == null || currenciesDelta.Count == 0)
                return string.Empty;

            var entries = new List<string>();
            foreach (var kvp in currenciesDelta)
            {
                entries.Add($"{kvp.Key}:{kvp.Value}");
            }

            return string.Join(",", entries);
        }
#endif
    }
}

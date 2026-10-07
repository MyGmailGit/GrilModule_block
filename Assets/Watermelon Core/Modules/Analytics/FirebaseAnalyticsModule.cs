using GameLogic;
using Firebase.Analytics;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class FirebaseAnalyticsModule : Singleton<FirebaseAnalyticsModule>
    {
        public enum UserPropertyType
        {
            user_video_b = 0,
        }
        public enum EventType
        {
            user_B_set,
        }

        public enum EventLeveType
        {
            start,
            complete,
            fail,
        }
        protected override void OnInit()
        {
            base.OnInit();
        }

        private readonly Dictionary<string, string> dataKeyValue = new Dictionary<string, string>();
        private readonly List<Parameter> parameters = new List<Parameter>();

        public void SetUserProperty(UserPropertyType userPropertyType, string value)
        {
#if !UNITY_EDITOR
            FirebaseAnalytics.SetUserProperty(userPropertyType.ToString(), value);
#endif
        }

        public void SendUserBSetEvent()
        {
            var save = SaveController.GetSaveObject<SimpleIntSave>($"FirebaseAnalyticsModule_user_B_set");
            if (save.Value != 1)
            {
                save.Value = 1;
                SendEvent(EventType.user_B_set, null);
            }
        }

        public void SendLevelEvent(int level, EventLeveType isStart)
        {
            if (isStart == EventLeveType.start)
            { SendEvent($"level_start_{level + 1}", null); }
            else if (isStart == EventLeveType.complete)
            { SendEvent($"level_complete_{level + 1}", null); }
            else if (isStart == EventLeveType.fail)
            { SendEvent($"level_fail_{level + 1}", null); }
        }

        public void SendGallaryViewEvent(string ImgId)
        {
            SendEvent($"gallary_view_{ImgId}", null);
        }

        public void SendGallaryDownloadEvent(string ImgId)
        {
            SendEvent($"gallary_download_{ImgId}", null);
        }

        public void RateUsShow()
        {
            SendEvent("rate_us_show", null);
        }
        public void RateUsNotRateClose()
        {
            SendEvent("rate_us_closeNotRate", null);
        }

        public void SetReteUsStar(int starNum)
        {
            SendEvent($"rate_us_star_{starNum}", null);
        }

        public void OnAdRevenuePaidEvent(string countryCode, double revenue, string networkName, string adUnitId, string adFormat, string mediation, int networkfirmid, string placement, string adsource)
        {
            // 创建要发送的参数
            var impressionParameters = new[] {
                    new Firebase.Analytics.Parameter("ad_platform", mediation),
                    new Firebase.Analytics.Parameter("ad_source", networkName),
                    new Firebase.Analytics.Parameter("ad_unit_name", adUnitId),
                    new Firebase.Analytics.Parameter("ad_format", adFormat),
                    new Firebase.Analytics.Parameter("value", revenue),
                    new Firebase.Analytics.Parameter("currency", "USD"), // AppLovin 收入默认为美元
                    new Firebase.Analytics.Parameter("placement", placement),
                };

            // 记录 ad_impression 事件
            Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
        }


        private void SendEvent(EventType eventType, Dictionary<string, string> dataalue)
        {
            SendEvent(eventType.ToString(), dataalue);
        }
        private void SendEvent(string eventType, Dictionary<string, string> dataalue)
        {
            parameters.Clear();
            if (dataalue != null)
                foreach (var it in dataalue)
                {
                    parameters.Add(new Parameter(it.Key, it.Value));
                }
#if !UNITY_EDITOR
            FirebaseAnalytics.LogEvent(eventType, parameters);
#endif
            Debug.Log($"[FirebaseAnalytics] Track event: {eventType} with properties: {SerializeProperties(dataalue)}");
        }
        private static string SerializeProperties(Dictionary<string, string> properties)
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

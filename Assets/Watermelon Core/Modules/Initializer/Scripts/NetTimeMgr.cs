using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GameLogic;
using UnityEngine;

namespace GameBase
{
    /// <summary>
    /// 时间管理类
    /// </summary>
    public class NetTimeMgr : MonoBehaviour
    {
        public static NetTimeMgr Instance { get; private set; } = null;

        public Action<DateTime> onEnterNextDay;

        private DateTime m_timeOf1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        /// <summary>
        /// 服务器时间 毫秒
        /// </summary>
        private ulong m_NetUtcTime = 0;
        /// <summary>
        /// 服务器时间回来的时候的 Time.realtimeSinceStartup
        /// </summary>
        private float m_onNetTime_StartTime = 0;

        private const int CUniTaskUpdateTimeMs = 5;

        /// <summary>
        /// 本地时区时间
        /// </summary>
        private DateTime m_NetCurLocalZoneTime;

        /// <summary>
        /// 记录已经跨过的天。防止网络数据慢导致重复跨天
        /// </summary>
        private List<DateTime> m_dateList = new List<DateTime>();

        // 帧计数器，用于每10帧检测一次
        private int m_frameCounter = 0;

        private DateTime m_lastCheckTime;

        public bool HasGetNetTime { get { return m_NetUtcTime != 0; } }

        public DateTime CurUtc
        {
            get
            {
                ///如果没有服务器时间，就用本地时间
                if (m_NetUtcTime == 0)
                {
                    return DateTime.Now.ToUniversalTime();
                }
                return CurTime.ToUniversalTime();
            }
        }

        public static void Create()
        {
            if (Instance == null)
            {
                GameObject gameObj = new GameObject("[NetTimeMgr]");
                gameObj.AddComponent<NetTimeMgr>();
            }
        }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        private void Start()
        {
            m_lastCheckTime = CurTime;
            StartCoroutine(NetTimeCheck());
        }

        public DateTime GetDate()
        {
            return CurTime.Date;
        }

        public DateTime CurTime
        {
            get
            {
                ///如果没有服务器时间，就用本地时间
                if (m_NetUtcTime == 0)
                {
                    return DateTime.Now;
                }
                return m_NetCurLocalZoneTime.AddMilliseconds((Time.realtimeSinceStartup - m_onNetTime_StartTime) * 1000);
            }
        }

        /// <summary>
        /// 设置网络时间
        /// </summary>
        /// <param name="utcTime"></param>
        private void SetNetUtcTime(ulong utcTime)
        {
            if (utcTime == 0)
            {
                return;
            }

            var oldTime = CurTime;

            m_NetUtcTime = utcTime;
            m_NetCurLocalZoneTime = m_timeOf1970.AddMilliseconds(utcTime).ToLocalTime();

            m_onNetTime_StartTime = Time.realtimeSinceStartup;
            __CheckNextDay(oldTime, CurTime);
            // 服务器时间
            UnityEngine.Debug.Log("SetNetUtcTime:" + m_NetCurLocalZoneTime);
        }

        private void Update()
        {
            // 每10帧检测一次是否跨天
            m_frameCounter++;

            if (m_frameCounter >= 10)
            {
                m_frameCounter = 0;
                DateTime currentTime = CurTime;
                __CheckNextDay(m_lastCheckTime, currentTime);
                m_lastCheckTime = currentTime;
            }
        }

        IEnumerator NetTimeCheck()
        {
            // 首次启动时立即请求一次
            yield return StartCoroutine(FetchServerTime());
            yield return null;
            // 然后每隔 CUniTaskUpdateTimeMs 毫秒请求一次
            while (!(m_NetUtcTime > 0))
            {
                yield return new WaitForSecondsRealtime(CUniTaskUpdateTimeMs);
                yield return StartCoroutine(FetchServerTime());
            }
            UnityEngine.Debug.Log("已成功获取服务器时间，停止网络请求");
        }

        /// <summary>
        /// 从服务器获取时间
        /// </summary>
        private IEnumerator FetchServerTime()
        {
            string url = "https://4f74g8tyu6.execute-api.ap-southeast-2.amazonaws.com/api/time/utc";

            using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string json = request.downloadHandler.text;
                        // 解析JSON
                        var response = JsonUtility.FromJson<NetworkTimeResponse>(json);
                        if (response != null && response.unixtime > 0)
                        {
                            SetNetUtcTime((ulong)response.unixtime * 1000); // 转换为毫秒
                        }
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError($"解析服务器时间失败: {e.Message}");
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"获取服务器时间失败: {request.error}");
                }
            }
        }

        /// <summary>
        /// 检查是否是下一天
        /// </summary>
        /// <param name="oldtime"></param>
        /// <param name="newtime"></param>
        private void __CheckNextDay(DateTime oldtime, DateTime newtime)
        {
            if (oldtime.DayOfYear != newtime.DayOfYear && !__IsExistDate(newtime))
            {
                UnityEngine.Debug.Log("NextDay");
                __RecordDate(newtime);
                onEnterNextDay?.Invoke(newtime);
            }
        }

        private void __RecordDate(DateTime dateTime)
        {
            m_dateList.Add(dateTime);
        }

        private bool __IsExistDate(DateTime dateTime)
        {
            return m_dateList.Exists((item) => { return item.Year == dateTime.Year && item.Month == dateTime.Month && item.Day == dateTime.Day; });
        }

    }

    [Serializable]
    public class NetworkTimeResponse
    {
        public long unixtime;
        public string utc_datetime;
    }
}
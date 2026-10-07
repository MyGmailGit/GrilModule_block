using System;
using System.Collections.Generic;
using UnityEngine;

namespace VideoSystem
{
    public enum VideoServerType
    {
        Joy,
        Odhikltd,
        ShinyScrews,
        BaseUrl
    }

    /// <summary>
    /// 视频下载地址辅助类
    /// </summary>
    public static class VideoServerUrl
    {
        private const string PREF_USED_URL = "VIDEO_USED_URL_";
        private const string PREF_SERVER_INVALID = "VIDEO_SERVER_INVALID_";
        private const string PREF_CURRENT_SERVER_INDEX = "VIDEO_CURRENT_SERVER_INDEX";
        private const string PREF_BASE_LEVEL = "VIDEO_BASE_LEVEL";

        public static string BASE_URL { get; } = "https://d2ljjr3y54ibcz.cloudfront.net/abcd/";

        public class UrlData
        {
            public VideoServerType videoServerType;
            public string Url;
            public string netFileName;
        }
        private class UrlWithName
        {
            public string url;
            public string netfile;
        }

        /// <summary>
        /// 最终兜底地址
        /// level1.mp4
        /// level2.mp4
        /// ...
        /// </summary>
        // private const string BASE_URL =
        // "https://d2ljjr3y54ibcz.cloudfront.net/beiyong/";

        private static readonly VideoServerType[] RotateServers =
        {
            VideoServerType.Joy,
            VideoServerType.Odhikltd,
            VideoServerType.ShinyScrews
        };

        #region Joy

        // private const string JOY_BASE =
        // "https://joys3.sortchallenge.ink/resource.ictapi.xyz/PictureRelease/Videos/Girls/";

        private static readonly List<string> JoyFiles = new();

        #endregion

        #region Odhikltd

        // private const string ODHI_BASE =
        //     "https://odhikltd.com/MT_video/";

        private static readonly List<string> OdhFiles = new();

        #endregion

        #region ShinyScrews

        // private const string SHINY_BASE =
        //     "https://www.shinyscrews.com/img/1.0.0/";

        private static readonly List<string> ShinyFiles = new();

        #endregion

        static VideoServerUrl()
        {
            InitJoyFiles();
            InitOdhiFiles();
            InitShinyFiles();
        }

        #region Public

        /// <summary>
        /// 获取视频下载地址
        /// </summary>
        public static UrlData GetVideoUrl()
        {
            // 前3种轮询
            for (int i = 0; i < RotateServers.Length; i++)
            {
                VideoServerType serverType = GetNextServer();

                if (IsServerInvalid(serverType))
                    continue;

                UrlWithName url = BuildUrl(serverType);

                if (!string.IsNullOrEmpty(url.url))
                {
                    SaveUsedUrl(url.url);

                    Debug.Log($"[VideoServer] Use Server : {serverType}");
                    Debug.Log($"[VideoServer] Url : {url}");

                    return new UrlData() { Url = url.url, videoServerType = serverType, netFileName = url.netfile };
                }

                // 当前服务器资源耗尽
                SetServerInvalid(serverType, true);
            }

            // 全部失效 -> BaseUrl
            UrlWithName baseUrl = GetBaseLevelUrl();

            SaveUsedUrl(baseUrl.url);

            Debug.Log("[VideoServer] All Server Invalid -> Use BaseUrl");
            Debug.Log($"[VideoServer] Url : {baseUrl}");

            return new UrlData() { Url = baseUrl.url, videoServerType = VideoServerType.BaseUrl, netFileName = baseUrl.netfile };
        }

        /// <summary>
        /// 标记服务器失效
        /// </summary>
        public static void SetServerInvalid(VideoServerType type, bool invalid)
        {
            PlayerPrefs.SetInt(PREF_SERVER_INVALID + type, invalid ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 是否使用过
        /// </summary>
        public static bool HasUsedUrl(string url)
        {
            return PlayerPrefs.GetInt(PREF_USED_URL + url, 0) == 1;
        }

        /// <summary>
        /// 重置所有记录（调试用）
        /// </summary>
        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(PREF_CURRENT_SERVER_INDEX);
            PlayerPrefs.DeleteKey(PREF_BASE_LEVEL);

            foreach (VideoServerType type in Enum.GetValues(typeof(VideoServerType)))
            {
                PlayerPrefs.DeleteKey(PREF_SERVER_INVALID + type);
            }

            PlayerPrefs.Save();

            Debug.Log("[VideoServer] Reset All");
        }

        #endregion

        #region Core

        private static VideoServerType GetNextServer()
        {
            int index = PlayerPrefs.GetInt(PREF_CURRENT_SERVER_INDEX, 0);

            VideoServerType type = RotateServers[index];

            index++;

            if (index >= RotateServers.Length)
                index = 0;

            PlayerPrefs.SetInt(PREF_CURRENT_SERVER_INDEX, index);

            return type;
        }

        private static bool IsServerInvalid(VideoServerType type)
        {
            return PlayerPrefs.GetInt(PREF_SERVER_INVALID + type, 0) == 1;
        }

        private static void SaveUsedUrl(string url)
        {
            PlayerPrefs.SetInt(PREF_USED_URL + url, 1);
            PlayerPrefs.Save();
        }

        private static UrlWithName BuildUrl(VideoServerType type)
        {
            string JOY_BASE = "https://joys3.sortchallenge.ink/resource.ictapi.xyz/PictureRelease/Videos/Girls/";
            string ODHI_BASE = "https://odhikltd.com/MT_video/";
            string SHINY_BASE = "https://www.shinyscrews.com/img/1.0.0/";
            return type switch
            {
                VideoServerType.Joy =>
                    GetUnusedUrl(JOY_BASE, JoyFiles),

                VideoServerType.Odhikltd =>
                    GetUnusedUrl(ODHI_BASE, OdhFiles),

                VideoServerType.ShinyScrews =>
                    GetUnusedUrl(SHINY_BASE, ShinyFiles),

                VideoServerType.BaseUrl =>
                    GetBaseLevelUrl(),

                _ => null
            };
        }

        /// <summary>
        /// 获取未使用URL
        /// </summary>
        private static UrlWithName GetUnusedUrl(string baseUrl, List<string> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                string fileName = files[i] + ".mp4";
                string url = baseUrl + fileName;

                if (!HasUsedUrl(url))
                    return new UrlWithName() { url = url, netfile = fileName };
            }

            return null;
        }

        /// <summary>
        /// BaseUrl:
        /// level1.mp4
        /// level2.mp4
        /// ...
        /// </summary>
        private static UrlWithName GetBaseLevelUrl()
        {
            // string BASE_URL = "https://d2ljjr3y54ibcz.cloudfront.net/beiyong/";
            const int maxLevel = 150;
            int level = PlayerPrefs.GetInt(PREF_BASE_LEVEL, 1);
            if (level < 1)
                level = 1;

            int realLevel = ((level - 1) % maxLevel) + 1;
            string fileName = $"Block_Video_{realLevel}.mp4";
            string url = $"{BASE_URL}{fileName}";

            int nextLevel = realLevel + 1;
            if (nextLevel > maxLevel)
                nextLevel = 1;

            PlayerPrefs.SetInt(PREF_BASE_LEVEL, nextLevel);
            PlayerPrefs.Save();

            return new UrlWithName() { url = url, netfile = fileName };
        }

        #endregion

        #region Init Files

        private static void InitJoyFiles()
        {
            AddRange(JoyFiles, 6200001, 6200100);

            AddRange(JoyFiles, 650040001, 650040150);
            AddRange(JoyFiles, 650010001, 650010150);
            AddRange(JoyFiles, 650020001, 650020100);
            AddRange(JoyFiles, 650030001, 650030100);
            AddRange(JoyFiles, 650050001, 650050100);
            AddRange(JoyFiles, 650060001, 650060100);
            AddRange(JoyFiles, 650070001, 650070150);
            AddRange(JoyFiles, 650080001, 650080030);

            // AddRange(JoyFiles, 6000106001, 6000106016);
            // AddRange(JoyFiles, 6000242001, 6000242016);
            // AddRange(JoyFiles, 6000249001, 6000249016);

            // AddRange(JoyFiles, 6330303001, 6330303016);
            // AddRange(JoyFiles, 6330379001, 6330379016);
            // AddRange(JoyFiles, 6330445001, 6330445016);
            // AddRange(JoyFiles, 6330503001, 6330503016);

            AddRange(JoyFiles, 6000106001, 6000106016);
            AddRange(JoyFiles, 6000154001, 6000154016);
            AddRange(JoyFiles, 6000204001, 6000204016);
            AddRange(JoyFiles, 6000242001, 6000242016);
            AddRange(JoyFiles, 6000246001, 6000246016);
            AddRange(JoyFiles, 6000249001, 6000249016);
            AddRange(JoyFiles, 6000273001, 6000273016);
            AddRange(JoyFiles, 6330303001, 6330303016);
            AddRange(JoyFiles, 6330333001, 6330333016);
            AddRange(JoyFiles, 6330336001, 6330336016);
            AddRange(JoyFiles, 6330370001, 6330370016);
            AddRange(JoyFiles, 6330379001, 6330379016);
            AddRange(JoyFiles, 6330445001, 6330445016);
            AddRange(JoyFiles, 6330464001, 6330464016);
            AddRange(JoyFiles, 6330466001, 6330466016);
            AddRange(JoyFiles, 6330503001, 6330503016);
            AddRange(JoyFiles, 6330517001, 6330517016);
        }

        private static void InitOdhiFiles()
        {
            AddRangePrefix(OdhFiles, "Mov_", 1001, 1004);
            AddRangePrefix(OdhFiles, "Mov_", 2001, 2010);
            AddRangePrefix(OdhFiles, "Mov_", 3001, 3011);
            AddRangePrefix(OdhFiles, "Mov_", 4001, 4018);
            AddRangePrefix(OdhFiles, "Mov_", 5001, 5051);
        }

        private static void InitShinyFiles()
        {
            AddRangePrefix(ShinyFiles, "Sex_Video_", 1, 150);
        }

        #endregion

        #region Utils

        private static void AddRange(List<string> list, long start, long end)
        {
            for (long i = start; i <= end; i++)
            {
                list.Add(i.ToString());
            }
        }

        private static void AddRangePrefix(
            List<string> list,
            string prefix,
            int start,
            int end)
        {
            for (int i = start; i <= end; i++)
            {
                list.Add(prefix + i);
            }
        }

        #endregion
    }
}
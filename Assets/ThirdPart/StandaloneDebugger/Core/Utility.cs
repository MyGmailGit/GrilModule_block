using System;
using System.IO;
using UnityEngine;

namespace StandaloneDebugger
{
    /// <summary>
    /// 工具类 - 处理的简化版本
    /// </summary>
    internal static class Utility
    {
        public static class Text
        {
            public static string Format(string format, params object[] args)
            {
                return string.Format(format, args);
            }
        }

        public static class Path
        {
            public static string GetRegularPath(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return path;
                return path.Replace("\\", "/");
            }
        }

        public static class Converter
        {
            public static float GetInchesFromPixels(float pixels)
            {
                return pixels / Screen.dpi;
            }

            public static float GetCentimetersFromPixels(float pixels)
            {
                return GetInchesFromPixels(pixels) * 2.54f;
            }
        }

        public static class Marshal
        {
            private static long _cachedHGlobalSize = 0;

            public static long CachedHGlobalSize
            {
                get => _cachedHGlobalSize;
                set => _cachedHGlobalSize = value;
            }
        }
    }
}

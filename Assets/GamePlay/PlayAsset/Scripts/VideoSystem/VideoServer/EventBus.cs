using System;
using System.Collections.Generic;

namespace VideoSystem
{
    /// <summary>
    /// 全局事件总线
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> events = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// 订阅事件
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (!events.ContainsKey(type))
            {
                events[type] = new List<Delegate>();
            }

            if (!events[type].Contains(handler))
            {
                events[type].Add(handler);
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (events.ContainsKey(type))
            {
                events[type].Remove(handler);

                if (events[type].Count == 0)
                {
                    events.Remove(type);
                }
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        public static void Publish<T>(T eventData) where T : class
        {
            var type = typeof(T);
            if (events.ContainsKey(type))
            {
                // 创建副本避免在遍历时修改集合
                var handlers = new List<Delegate>(events[type]);
                foreach (var handler in handlers)
                {
                    try
                    {
                        (handler as Action<T>)?.Invoke(eventData);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"Event handler error: {e.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有事件（用于场景切换或重置）
        /// </summary>
        public static void ClearAll()
        {
            events.Clear();
        }
    }
}
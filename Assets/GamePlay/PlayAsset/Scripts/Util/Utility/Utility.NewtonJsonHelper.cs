using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Utility
{
    /// <summary>
    /// 默认 JSON 函数集辅助器。
    /// </summary>
    public static class NewtonJsonHelper
    {
        /// <summary>
        /// 将对象序列化为 JSON 字符串。
        /// </summary>
        /// <param name="obj">要序列化的对象。</param>
        /// <returns>序列化后的 JSON 字符串。</returns>
        public static string ToJson(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <typeparam name="T">对象类型。</typeparam>
        /// <param name="json">要反序列化的 JSON 字符串。</param>
        /// <returns>反序列化后的对象。</returns>
        public static T ToObject<T>(string json)
        {
            Type t = typeof(T);
            bool isDictionary = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)
                                && t.GetGenericArguments()[0] == typeof(string)
                                && t.GetGenericArguments()[1] == typeof(object);
            bool isListObject = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)
                                && t.GetGenericArguments()[0] == typeof(object);
            // 字典和列表对象是object的时候使其能够继续深度遍历解析
            if (isDictionary || isListObject)
            {
                return (T)ParseJToken(JObject.Parse(json));
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        // 解析JToken递归地将其转换为字典或列表结构
        public static object ParseJToken(JToken token)
        {
            if (token is JObject obj)
            {
                var dict = new Dictionary<string, object>();
                foreach (var property in obj.Properties())
                {
                    dict[property.Name] = ParseJToken(property.Value);
                }
                return dict;
            }
            else if (token is JArray array)
            {
                var list = new List<object>();
                foreach (var item in array)
                {
                    list.Add(ParseJToken(item));
                }
                return list;
            }
            else
            {
                // 对于原始数据类型，直接返回其值
                return ((JValue)token).Value;
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为对象。
        /// </summary>
        /// <param name="objectType">对象类型。</param>
        /// <param name="json">要反序列化的 JSON 字符串。</param>
        /// <returns>反序列化后的对象。</returns>
        public static object ToObject(System.Type objectType, string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject(json, objectType);
        }
    }
}
#if TEST_MODE
using System;
using UnityEngine;

namespace StandaloneDebugger
{
    /// <summary>
    /// 日志节点。
    /// </summary>
    public class LogNode
    {
        /// <summary>
        /// 日志类型。
        /// </summary>
        public LogType Type { get; }

        /// <summary>
        /// 日志信息。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 堆栈跟踪。
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        /// 日志时间戳（记录时刻）。
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// 初始化新实例。
        /// </summary>
        public LogNode(LogType type, string message, string stackTrace)
        {
            Type = type;
            Message = message;
            StackTrace = stackTrace;
            Timestamp = DateTime.Now;
        }
    }
}
#endif
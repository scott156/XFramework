using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger
{
    /// <summary>
    /// 日志内容
    /// </summary>
    internal class LogEntity
    {
        /// <summary>
        /// Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Logging date
        /// </summary>
        public DateTime LogDate { get; set; }

        /// <summary>
        /// Current log level
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Thread
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Log Tags
        /// </summary>
        public Dictionary<string, string> LogTags { get; set; }

        /// <summary>
        /// Transaction exception
        /// 根据当前系统的日志级别, 级别越低, 记录越详细
        /// </summary>
        public Exception Exception { get; set; }

        public string CategoryName { get; set; }
    }
}

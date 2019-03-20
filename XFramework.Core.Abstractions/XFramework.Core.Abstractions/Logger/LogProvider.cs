using XFramework.Core.Abstractions.Utility;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;

namespace XFramework.Core.Abstractions.Logger
{
    /// <summary>
    /// Central log provider
    /// </summary>
    public class LogProvider : Singleton<LogProvider>, ILoggerProvider
    {
        private ILoggerFactory loggerFactory;

        // 同步锁
        private static readonly object syncObject = new object();

        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName);
        }
        
        public void Dispose()
        {
            XLogger.Stop();
        }

        /// <summary>
        /// 存储全局的日志工厂类
        /// </summary>
        /// <param name="factory"></param>
        public void Store(ILoggerFactory factory)
        {
            this.loggerFactory = factory;
        }

        /// <summary>
        /// 增加LogTag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        public void Add(string tag, string value)
        {
            if (string.IsNullOrEmpty(tag)) {
                return;
            }

            LogScopeProvider.GetInstance().Add(tag.ToLower(), value);
        }

        public void Add(Dictionary<string, string> tags)
        {
            if (tags == null)
            {
                return;
            }

            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag.Key))
                {
                    continue;
                }

                LogScopeProvider.GetInstance().Add(tag.Key.ToLower(), tag.Value);
            }            
        }

        /// <summary>
        /// 获取日志工厂类
        /// </summary>
        /// <returns></returns>
        public ILoggerFactory LoggerFactory
        {
            get
            {
                return loggerFactory;
            }
        }
        

        /// <summary>
        /// 创建Logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static ILogger Create(string categoryName)
        {
            return LogProvider.GetInstance().LoggerFactory.CreateLogger(categoryName);
        }

        /// <summary>
        /// 创建Logger
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static ILogger Create(Type type)
        {
            return LogProvider.GetInstance().LoggerFactory.CreateLogger(type.Name);
        }
    }
}
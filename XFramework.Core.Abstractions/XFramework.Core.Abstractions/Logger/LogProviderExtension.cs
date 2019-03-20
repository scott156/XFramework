using Microsoft.Extensions.Logging;
using System;

namespace XFramework.Core.Abstractions.Logger
{
    /// <summary>
    /// XFramework日志扩展
    /// </summary>
    public static class LogProviderExtension
    {
        /// <summary>
        /// 增加X日志
        /// </summary>
        /// <param name="factory">ILoggerFactory</param>
        public static ILoggerFactory AddXLog(this ILoggerFactory factory)
        {
            LogProvider.GetInstance().Store(factory);
            factory.AddProvider(LogProvider.GetInstance());

            return factory;
        }
        
        /// <summary>
        /// 保存loggerFactory实例
        /// </summary>
        /// <param name="factory"></param>
        public static void Store(this ILoggerFactory factory)
        {
            LogProvider.GetInstance().Store(factory);
        }

        public static void Debug(this ILogger logger, string messageFormat, params object[] args)
        {
            logger.LogDebug(SilenceFormat(messageFormat, args));
        }

        public static void Info(this ILogger logger, string messageFormat, params object[] args)
        {
            logger.LogInformation(SilenceFormat(messageFormat, args));
        }

        public static void Error(this ILogger logger, string messageFormat, params object[] args)
        {
            logger.LogError(SilenceFormat(messageFormat, args));
        }

        public static void Error(this ILogger logger, Exception e, string messageFormat, params object[] args)
        {
            logger.LogError(e, SilenceFormat(messageFormat, args));
        }

        public static void Warn(this ILogger logger, string messageFormat, params object[] args)
        {
            logger.LogWarning(SilenceFormat(messageFormat, args));
        }

        private static string SilenceFormat(string messageFormat, params object[] args)
        {
            if (args == null || args.Length == 0) {
                return messageFormat;
            }
            
            try
            {
                return string.Format(messageFormat, args);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
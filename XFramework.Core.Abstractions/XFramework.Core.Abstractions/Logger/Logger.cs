using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Utility;

namespace XFramework.Core.Abstractions.Logger
{
    /// <summary>
    /// XLog实现
    /// </summary>
    public class Logger : ILogger
    {
        private string categoryName;
        public Logger(string categoryName)
        {
            this.categoryName = categoryName;
        }

        /// <summary>
        /// 连续记录日志
        /// </summary>
        /// <param name="state"></param>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            return LogScopeProvider.GetInstance().SetScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
            Exception exception, Func<TState, Exception, string> formatter)
        {
            if (state == null)
            {
                return;
            }

            var entity = new LogEntity()
            {
                LogDate = DateTime.Now,
                Level = logLevel,
                LogTags = LogScopeProvider.GetInstance().Scopes,
                Message = state.ToString(),
                ThreadId = Thread.CurrentThread.ManagedThreadId,
                Exception = exception,
                CategoryName = categoryName
            };

            XLogger.Log(entity);
        }
    }

    internal static class XLogger
    {
        private const int LOG_LEVEL_DISPLAY_LENGTH = 5;
        private const string THREAD_ID_FORMAT = "000000";
        private const string LOG_DATE_FORMAT = "[yyyy-MM-dd HH:mm:ss fff]";

        // 本地文件写入
        private static StreamWriter writer;

        public static void Log(LogEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            logQueue.Enqueue(entity);
            manualEvent.Set();
        }
        private static ManualResetEventSlim manualEvent = new ManualResetEventSlim(true);

        private static ConcurrentQueue<LogEntity> logQueue = new ConcurrentQueue<LogEntity>();
        private static long currentSize = 0;
        private static LocalLogInfo logInfo = AppSetting.GetInstance().LocalLogInfo;
        private static bool forceStopSign = false;
        private static Task loggerTask;
        private const int LoggerTaskTimeoutInMs = 3000;

        /// <summary>
        /// 初始化日志, 打开本地文件
        /// </summary>
        static XLogger()
        {
            OpenFile();

            loggerTask = Task.Factory.StartNew(() =>
            {
                Execute();
            }, TaskCreationOptions.LongRunning);
        }

        public static void Stop()
        {
            // 强制停止标记
            forceStopSign = true;
            // 设置信号，确保标记被执行
            manualEvent.Set();
            // 等待执行结束
            loggerTask.Wait(LoggerTaskTimeoutInMs);
            // 强制刷新
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }
        }

        private static void Execute()
        {
            try
            {
                while (true)
                {
                    // 等待事件触发
                    manualEvent.Wait();
                    manualEvent.Reset();

                    WriteLog();

                    if (forceStopSign == true)
                    {
                        return;
                    }
                }
            }
            catch { }
        }

        private static void WriteLog()
        {
            while (logQueue.Count > 0)
            {
                // 如果文件大小超过了预定的大小，那么打开一个新文件
                if (currentSize > logInfo.MaxLogSize)
                {
                    OpenFile();
                }

                logQueue.TryDequeue(out LogEntity entity);
                if (writer == null || entity == null) continue;

                // 组装前缀                      
                var prefix = $"{entity.LogDate.ToString(LOG_DATE_FORMAT)} " +
                             $"[{StringFormatter.SubStringOrPadding(entity.Level.ToString(), LOG_LEVEL_DISPLAY_LENGTH)}] " +
                             $"{entity.ThreadId.ToString(THREAD_ID_FORMAT)} ";


                if (logInfo.IncludeScopes) LogScopes(entity, prefix);

                var content = $"{prefix}{entity.Message}";
                Log(content);

                // Record details for exception
                LogException(entity.Exception, prefix);
            }
        }

        private static void LogException(Exception exception, string prefix)
        {
            if (exception != null)
            {
                Log(prefix + exception.Message);
                Log(prefix + exception.StackTrace.Replace(Environment.NewLine, Environment.NewLine + prefix));
            }
        }

        private static StringBuilder sb = new StringBuilder(100);
        /// <summary>
        /// 保存日志的范围
        /// </summary>
        /// <param name="currentSize"></param>
        /// <param name="entity"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private static void LogScopes(LogEntity entity, string prefix)
        {
            if (entity.LogTags != null && entity.LogTags.Count > 0)
            {
                foreach (var item in entity.LogTags)
                {
                    if (AppSetting.GetInstance().LocalLogInfo.IgnoreScopes.Contains(item.Key)) continue;
                    sb.Append($"[{item.Key}={item.Value}],");
                };
            }

            sb.Append($"[category={entity.CategoryName}]");

            Log(sb.ToString());
        }

        /// <summary>
        /// 将日志写入文件
        /// </summary>
        /// <param name="value"></param>
        /// <param name="currentSize"></param>
        private static void Log(string value)
        {
            writer.WriteLine(value);
            currentSize = writer.BaseStream.Length;
            writer.AutoFlush = true;
        }

        /// <summary>
        /// 打开一个新的日志文件
        /// </summary>
        private static void OpenFile()
        {
            var logInfo = AppSetting.GetInstance().LocalLogInfo;

            // 创建目录
            var dir = Directory.CreateDirectory(logInfo.LogPath);

            // 如果日志前缀没有定义, 使用AppId作为前缀
            var prefix = logInfo.Prefix ?? AppSetting.GetInstance().AppId;
            var filename = $"{prefix}.{DateTime.Now.ToString("yyyyMMdd.HHmmss.fff")}.log";

            writer = new StreamWriter(Path.Combine(dir.FullName, filename))
            {
                AutoFlush = true
            };
        }
    }
}

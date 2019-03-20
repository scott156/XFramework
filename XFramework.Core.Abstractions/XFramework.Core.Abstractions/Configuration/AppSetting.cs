using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using XFramework.Core.Abstractions.Utility;

namespace XFramework.Core.Abstractions.Configuration
{
    /// <summary>
    /// 应用程序配置
    /// </summary>
    public class AppSetting : Singleton<AppSetting>
    {
        /// <summary>
        /// 应用程序唯一识别符
        /// </summary>
        public string AppId { get; set; }

        public IHostingEnvironment Enviroment { get; set; }

        /// <summary>
        /// 当前应用支持的服务类型
        /// </summary>
        public List<string> ServiceHandler { get; set; }

        public List<string> SoaServiceListener { get; set; }

        public List<string> AutofacAssemblies { get; set; }

        private string _instanceId;
        public string InstanceId
        {
            get
            {
                if (string.IsNullOrEmpty(_instanceId))
                {
                    return System.Environment.GetEnvironmentVariable("InstanceId");
                }

                return _instanceId;
            }
            set
            {
                this._instanceId = value;
            }
        }

        public LocalLogInfo LocalLogInfo { get; set; } = new LocalLogInfo();

        public List<DatabaseSetInfo> DatabaseSets { get; set; }
    }

    public class DatabaseSetInfo
    {
        public string Name { get; set; }

        public DatabaseTypeEnum DatabaseType { get; set; }

        public List<ConnectionInfo> Connections { get; set; }
    }

    public class ConnectionInfo
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string ConnectionString { get; set; }
    }

    public enum DatabaseTypeEnum
    {
        MySql = 1,
        SqlServer = 2
    }

    public class LocalLogInfo
    {
        /// <summary>
        /// 日志前缀
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// 默认的日志路径
        /// </summary>
        public string LogPath { get; set; } = "logs";

        /// <summary>
        /// 是否输出Scope/Tag
        /// </summary>
        public bool IncludeScopes { get; set; } = false;

        /// <summary>
        /// 单个文件最大的大小, 默认为10M
        /// </summary>
        public long MaxLogSize { get; set; } = 1024 * 1024 * 10;

        public List<string> IgnoreScopes { get; set; } = new List<string> { "requestpath", "correlationid", "category"};

    }
}

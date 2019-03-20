using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Configuration.Server.Contract.Configuration.Query
{
    public class ConfigurationQueryRequestType : SoaRequestType
    {
        /// <summary>
        /// 目标文件
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// 本地缓存的配置版本
        /// </summary>
        public long CurrentVersion { get; set; }
    }
}

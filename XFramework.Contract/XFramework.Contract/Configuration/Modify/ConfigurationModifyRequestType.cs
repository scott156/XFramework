using System.Collections.Generic;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Contract.Configuration.Modify
{
    public class ConfigurationModifyRequestType : SoaRequestType
    {
        public string File { get; set; }

        /// <summary>
        /// 本地缓存的配置版本
        /// </summary>
        public long CurrentVersion { get; set; }

        /// <summary>
        /// 强制更新
        /// 强制更新时，无需校验版本信息
        /// </summary>
        public bool ForceOverride { get; set; }

        /// <summary>
        /// 修改和删除的键值
        /// </summary>
        public List<ModifiedCongiruationItemType> ModifiedProperties { get; set; }

        /// <summary>
        /// 新增的键值
        /// </summary>
        public List<ModifiedCongiruationItemType> AddedProperties { get; set; }
    }

    public class ModifiedCongiruationItemType
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}

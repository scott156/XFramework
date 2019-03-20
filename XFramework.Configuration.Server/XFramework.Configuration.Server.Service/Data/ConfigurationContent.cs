using System;
using System.Collections.Generic;

namespace XFramework.Configuration.Server.Service.Query.Data
{
    public class ConfigurationContent : ICloneable
    {
        /// <summary>
        /// 配置文件名称
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// 配置文件版本号
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// 配置文件属性
        /// </summary>
        public List<ConfigurationItem> Properties { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastModifyDate { get; set; }

        public object Clone()
        {
            var newConfig = new ConfigurationContent
            {
                Version = this.Version + 1,
                Filename = this.Filename,
                LastModifyDate = DateTime.Now,
                Properties = new List<ConfigurationItem>()
            };

            if (this.Properties != null && this.Properties.Count > 0)
            {
                this.Properties.ForEach(p => newConfig.Properties.Add(new ConfigurationItem()
                {
                    Key = p.Key,
                    Value = p.Value
                }));
            }

            return newConfig;
        }
    }
}

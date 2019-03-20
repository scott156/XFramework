using System;
using System.Xml.Serialization;

namespace XFramework.Configuration.Server.Service.Query.Data
{
    public class ConfigurationItem
    {
        /// <summary>
        /// 主键
        /// </summary>
        [XmlAttribute]
        public String Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [XmlAttribute]
        public String Value { get; set; }
    }
}

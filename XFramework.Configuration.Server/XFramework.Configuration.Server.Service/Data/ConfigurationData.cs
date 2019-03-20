using System.Collections.Generic;
using System.Xml.Serialization;

namespace XFramework.Configuration.Server.Service.Query.Data
{
    /// <summary>
    /// 配置数据清单
    /// </summary>
    public class ConfigurationData
    {
        public List<ConfigurationContent> Configurations { get; set; }

        [XmlIgnore]
        public Dictionary<string, ConfigurationContent> ConfigurationDictionary { get; set; } = new Dictionary<string, ConfigurationContent>();

        public void Init()
        {
            var dic = new Dictionary<string, ConfigurationContent>();
            Configurations.ForEach(p => dic.Add(p.Filename, p));

            this.ConfigurationDictionary = dic;
        }


    }
}

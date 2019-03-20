using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using XFramework.Configuration.Server.Service.Data.DataAdapter.DataAdapter;
using XFramework.Configuration.Server.Service.Query.Data;
using XFramework.Configuration.Server.Service.Error;
using XFramework.Core.Abstractions.Error;
using System.Collections.Generic;

namespace XFramework.Configuration.Server.Service.Data.DataAdapter
{
    /// <summary>
    /// 本地文件配置数据加载器，将来可以通过其他实现切换到数据库
    /// </summary>
    public class LocalConfigurationDataAdapter : IConfigurationDataAdapter
    {
        /// <summary>
        /// 本地配置文件名称
        /// </summary>
        private const string LOCAL_CONFIG_FILENAME = "ConfigurationData.xml";

        private readonly ConfigurationData Config;

        public LocalConfigurationDataAdapter()
        {
            Config = Load();
        }

        /// <summary>
        /// 获取配置文件信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ConfigurationContent GetContent(string fileName)
        {
            var config = Config;
            fileName = fileName.ToLower();

            if (config.ConfigurationDictionary.ContainsKey(fileName))
            {
                return config.ConfigurationDictionary[fileName];
            }

            return null;
        }

        private ConcurrentDictionary<string, List<ManualResetEventSlim>> registers 
            = new ConcurrentDictionary<string, List<ManualResetEventSlim>>();

        /// <summary>
        /// 注册监听事件，当版本发生变化后及时通知
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ManualResetEventSlim Register(string fileName)
        {
            var list = registers.GetOrAdd(fileName.ToLower(), new List<ManualResetEventSlim>());

            lock (list)
            {
                var e = new ManualResetEventSlim();
                list.Add(e);

                return e;
            }
        }

        private XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationData));

        private ConfigurationData Load()
        {
            // 如果文件不存在，那么新建一个配置文件
            if (!File.Exists(LOCAL_CONFIG_FILENAME))
            {
                using (var sw = new StreamWriter(LOCAL_CONFIG_FILENAME))
                {
                    var configData = new ConfigurationData();
                    serializer.Serialize(sw, configData);

                    return configData;
                }
            }
            
            using (FileStream fs = new FileStream(LOCAL_CONFIG_FILENAME, FileMode.Open))
            {
                var configData = (ConfigurationData)serializer.Deserialize(fs);
                configData.Init();

                return configData;
            }
        }

        /// <summary>
        /// 修改配置，
        /// TODO：仔细考虑多个场景，不能出现遗漏，极端情况下减少错误判断的可能性
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public void Modify(string fileName, ConfigurationContent content)
        {
            var original = Config.Configurations.Find(p => p.Filename.Equals(fileName));

            if (original == null)
            {
                throw new ConfigurationModifyException
                    (ErrorCode.InvalidConfigurationFile, $"修改配置时无法找到相应的配置文件 : {fileName}");
            }

            // 修改当前配置
            Config.Configurations.Remove(original);
            Config.Configurations.Add(content);

            // 同步配置文件
            using (var sw = new StreamWriter(LOCAL_CONFIG_FILENAME))
            {
                serializer.Serialize(sw, Config);
            }

            // 重新初始化
            Config.Init();

            // 触发监听
            registers.TryGetValue(fileName, out var eventList);
            if (eventList == null) return;

            lock (eventList)
            {
                foreach (var manualEvent in eventList)
                {
                    manualEvent.Set();
                }

                // 清空所有监听事件
                eventList.Clear();
            }
        }
    }
}

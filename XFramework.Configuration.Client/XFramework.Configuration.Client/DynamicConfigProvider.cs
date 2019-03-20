using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using XFramework.Contract.Configuration.Query;
using XFramework.Core.Abstractions.Client;
using XFramework.Core.Abstractions.Client.Serializer;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Configuration.Client
{
    public class DynamicConfigProvider : IDynamicConfigProvider
    {
        private readonly ILogger logger = LogProvider.Create(typeof(DynamicConfigProvider));

        private readonly IServiceClient client = new HttpServiceClient();

        private long version = 0;

        /// <summary>
        /// 配置文件名称
        /// </summary>
        private readonly string file;

        private ConcurrentDictionary<string, string> properties = new ConcurrentDictionary<string, string>();

        public DynamicConfigProvider(string fileName)
        {
            this.file = fileName;

            // 必须初始化一次
            Sync(fileName);

            Task.Factory.StartNew(() =>
            {
                while(true)
                {
                    try
                    {
                        Sync(fileName);
                        Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Configuration server failure");
                        Thread.Sleep(3000);
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void Sync(string file)
        {
            var request = new ConfigurationQueryRequestType()
            {
                CurrentVersion = version,
                File = file
            };

            logger.Info($"Fetch configuration, file : {version}, version : {file}");

            var response = CallService(request);

            if (response.Header.ResponseCode == (int)ErrorCode.ConfigurationNotChanged)
            {
                logger.Info("Configuration file has not been changed");
                return;
            }

            if (response.Header.ResponseCode != (int)ErrorCode.Success)
            {
                throw new SoaServiceException(ErrorCode.InvalidConfigurationFile,
                    $"Error to fetch configuration file : {response.Header.Remark}");
            }

            version = response.Version;
            logger.Info($"Fetch configuration file successfully, new version : {version}");
            if (response.Properties == null || response.Properties.Count == 0)
            {
                properties.Clear();
                return;
            }

            var dic = new ConcurrentDictionary<string, string>();
            foreach (var property in response.Properties)
            {
                dic.TryAdd(property.Key, property.Value);
            }

            properties = dic;

            // 执行事件监听的动作
            foreach (var action in listeners)
            {
                action.Invoke();
            }
        }

        private const int DEFAULT_TIMEOUT_IN_MS = 60000;

        private readonly IServiceSerializer serailzer = new JsonSerializer();
        
        private ConfigurationQueryResponseType CallService(ConfigurationQueryRequestType request)
        {
            // 使用默认的地址 :
            // http://[env].configuration.colorstudio.com.cn
            var result = client.Call<ConfigurationQueryRequestType, ConfigurationQueryResponseType>
                ($"{GetConfigurationServerUri()}/api/configuration/query", request, DEFAULT_TIMEOUT_IN_MS, serailzer);

            return result.Result;
        }

        private string GetConfigurationServerUri()
        {
            var enviroment = AppSetting.GetInstance().Enviroment;

            var name = "Development";
            if (enviroment != null && !string.IsNullOrEmpty(enviroment.EnvironmentName))
            {
                name = enviroment.EnvironmentName;
            }
            
            return $"http://{name.ToLower()}.configuration.colorstudio.com.cn";
        }

        public bool? GetBooleanProperty(string key)
        {
            if (properties.TryGetValue(key, out var value) == false)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            value = value.Trim();

            if ("T".Equals(value, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if ("True".Equals(value, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if ("Yes".Equals(value, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public int? GetIntProperty(string key)
        {
            if (properties.TryGetValue(key, out var value) == false)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value, out var result) == false)
            {
                return null;
            }

            return result;
        }

        public long? GetLongProperty(string key)
        {
            if (properties.TryGetValue(key, out var value) == false)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            if (long.TryParse(value, out var result) == false)
            {
                return null;
            }

            return result;
        }

        public string GetStringProperty(string key)
        {
            properties.TryGetValue(key, out var value);

            return value;
        }

        private readonly ConcurrentBag<Action> listeners = new ConcurrentBag<Action>();

        public void RegisterListener(Action action)
        {
            listeners.Add(action);
        }
    }
}

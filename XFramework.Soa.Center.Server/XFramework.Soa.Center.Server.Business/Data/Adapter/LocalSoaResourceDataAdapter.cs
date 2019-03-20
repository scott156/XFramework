using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using XFramework.Configuration.Client;
using XFramework.Configuration.Client.Data;
using XFramework.Contract.ResourceCenter.Query;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Center.Server.Business.Client.ResourceCenter;
using XFramework.Soa.Center.Server.Business.Error;

namespace XFramework.Soa.Center.Server.Business.Data.Adapter
{
    public class LocalSoaResourceDataAdapter : ISoaResourceDataAdapter
    {
        private readonly ILogger logger = LogProvider.Create(typeof(LocalSoaResourceDataAdapter));
        /// <summary>
        /// 本地的配置文件名称
        /// </summary>
        private const string RESOURCE_FILE = "SoaData.xml";

        private readonly ConcurrentDictionary<string, SoaInfo> resources;

        public IResourceCenterClient ResourceClient { get; set; }

        public LocalSoaResourceDataAdapter()
        {
            var serializer = new XmlSerializer(typeof(SoaResource));

            using (var sr = new StreamReader(RESOURCE_FILE))
            {
                var data = (SoaResource)serializer.Deserialize(sr);
                resources = Convert(data.SoaResources);
            }
        }

        private ConcurrentDictionary<string, SoaInfo> Convert(List<SoaInfo> list)
        {
            var dic = new ConcurrentDictionary<string, SoaInfo>();
            if (list == null || list.Count == 0) return dic;

            list.ForEach(p =>
            {
                if (string.IsNullOrWhiteSpace(p.ServiceId)) return;

                var serviceId = p.ServiceId.ToLower();
                if (dic.ContainsKey(serviceId))
                {
                    logger.Warn($"ServiceId is duplicate : {serviceId}");
                }

                dic.TryAdd(serviceId, p);
            });

            return dic;
        }

        public SoaInfo Query(string serviceId)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new SoaDataAdapterException(ErrorCode.InvalidSoaServiceId, "ServiceId is empty");
            }

            if (resources.TryGetValue(serviceId.ToLower(), out var value) == false)
            {
                throw new SoaDataAdapterException(ErrorCode.InvalidSoaServiceId, "SerivceId does not exist");
            }

            return value;
        }

        /// <summary>
        /// 申请新的Soa服务
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="appId"></param>
        /// <param name="description"></param>
        public void Apply(string serviceId, string appId, string description)
        {
            if (string.IsNullOrWhiteSpace(serviceId))
            {
                throw new SoaDataAdapterException(ErrorCode.InvalidSoaServiceId, "ServiceId is empty");
            }

            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new SoaDataAdapterException(ErrorCode.InvalidAppId, "AppId is empty");
            }

            var soaInfo = new SoaInfo()
            {
                ServiceId = serviceId.Trim().ToLower(),
                AppId = appId.Trim().ToLower(),
                Description = description
            };

            if (resources.TryAdd(soaInfo.ServiceId, soaInfo) == false)
            {
                throw new SoaDataAdapterException(ErrorCode.InvalidSoaServiceId, "SerivceId already exist");
            }

            var list = new List<SoaInfo>(resources.Values);
            if (list.Exists(p => string.Equals(p.AppId, soaInfo.AppId, StringComparison.InvariantCultureIgnoreCase)) == false)
            {
                // 新的AppId需要被监听
                Listen(soaInfo.AppId);
            }

            Save();
        }

        /// <summary>
        /// 保存Soa注册列表
        /// </summary>
        private void Save()
        {
            var res = new SoaResource
            {
                SoaResources = new List<SoaInfo>(resources.Values)
            };
            
            using (var sw = new StreamWriter(RESOURCE_FILE))
            {
                var serializer = new XmlSerializer(typeof(SoaResource));
                serializer.Serialize(sw, res);
            }
        }

        private readonly object syncObject = new object();

        public void Register(string serviceId, string instanceId)
        {
            lock (syncObject)
            {
                if (string.IsNullOrWhiteSpace(instanceId))
                {
                    throw new SoaDataAdapterException(ErrorCode.InvalidInstanceId, "InstanceId is empty");
                }

                // 检查ServiceId存在性
                var soaInfo = Query(serviceId);

                // 判断是否已经注册了
                if (soaInfo.RegisteredResources != null)
                {
                    if (soaInfo.RegisteredResources.Exists(p => instanceId.Equals(p.InstanceId, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        logger.Info($"Soa服务 : {serviceId} 对应的实例 : {instanceId} 已经在注册列表中了");
                        return;
                    }
                }

                // 检查Soa绑定的App中，是否具备当前实例
                // 如果实例不存在，说明实例是不可以运行此Soa服务的
                var resp = ResourceClient.AppInfoQuery(soaInfo.AppId);
                if (resp.Instances == null || resp.Instances.Count == 0)
                {
                    throw new SoaDataAdapterException(ErrorCode.SoaRegistionFailed,
                        $"该服务{serviceId}对应的AppId({soaInfo.AppId})中，无可用的实例");
                }

                if (resp.Instances.Exists(p => instanceId.Equals(p.InstanceId, StringComparison.InvariantCultureIgnoreCase)) == false)
                {
                    throw new SoaDataAdapterException(ErrorCode.SoaRegistionFailed,
                        $"该服务{serviceId}对应的AppId({soaInfo.AppId})中，找不到{instanceId}实例");
                }

                // 注册当前实例
                soaInfo.RegisteredResources.Add(new ResourceInfo()
                {
                    InstanceId = instanceId.Trim().ToLower()
                });

                Save();
            }
        }

        public void StartListener()
        {
            var list = new List<string>();
            foreach (var info in resources)
            {
                if (list.Exists(p => p.Equals(info.Value.AppId, StringComparison.InvariantCultureIgnoreCase)) == false)
                {
                    list.Add(info.Value.AppId);
                }
            }
            
            foreach (var app in list)
            {
                Listen(app);
            }
        }

        public IDynamicConfigModifier Modifier { get; set; }

        private void Listen(string app)
        {
            var config = DynamicConfigFactory.GetInstance($"{app}.status.properties");

            // 应用程序状态发生变化后，对应的Soa服务可用实例的清单也会发生变化
            config.RegisterListener(() =>
            {
                // 获取需要更新的服务清单
                var services = QueryServiceByAppId(app);

                // 获取AppInfo
                var appInfo = ResourceClient.AppInfoQuery(app);

                // 更新Soa在配置中心的可用实例列表
                services.ForEach(p =>
                {
                    var addressList = GetAvailableInstances(appInfo, config, p);
                    var address = addressList == null ? string.Empty : string.Join(";", addressList);

                    logger.Info($"Update soa instances list : {address}");
                    // 更新配置中心的Soa对应的实例的清单
                    // 为了提高性能，如果Soa注册中心的服务采用集群的环境，那么需要一个Controller来控制和分配每一个实例支持不同的app
                    Modifier.Update($"soa.{p.ServiceId}.properties", new List<ModifiedPropertyInfo>()
                    {
                        new ModifiedPropertyInfo()
                        {
                            Key = "ServerList",
                            Value = address
                        }
                    });
                });

            });
        }

        private List<string> GetAvailableInstances
            (AppInfoQueryResponseType appInfo, IDynamicConfigProvider provider, SoaInfo soaInfo)
        {
            // 如果Soa中没有注册任何实例，那么当前soa服务就不存在可用的实例
            if (soaInfo.RegisteredResources == null || soaInfo.RegisteredResources.Count == 0)
            {
                logger.Warn($"There's no registered instance in current service : {soaInfo.ServiceId}");
                return null;
            }

            // 如果soa服务对应的app不存在任何实例，那么soa服务也不可以被访问
            if (appInfo.Instances == null || appInfo.Instances.Count == 0)
            {
                logger.Warn($"There's no instance in current app : {appInfo.AppId}");
                return null;
            }

            List<string> list = new List<string>();
            foreach (var item in soaInfo.RegisteredResources)
            {
                // 根据在Soa中注册的实例Id，在app信息中查找实例是否存在，并且校验实例的状态
                var instance = appInfo.Instances.Find
                    (p => p.InstanceId.Equals(item.InstanceId, StringComparison.InvariantCultureIgnoreCase));

                if (instance == null)
                {
                    logger.Warn($"Althrough this instances '{item.InstanceId}' was registered in soa service '{soaInfo.ServiceId}'," +
                        $" it is no longer in the app's resource list : {appInfo.AppId}");
                    continue;
                }

                var status = provider.GetIntProperty(item.InstanceId);
                logger.Info($"Instance status : {item.InstanceId}, {status}");

                if (VerifyInstanceStatus(status) == false)
                {
                    continue;
                }
                // 获取实例的具体资源
                var address = $"{instance.ResourceId}:{instance.Port}";
                logger.Info($"Found available instance {item.InstanceId} of {appInfo.AppId} for service : " +
                    $"{soaInfo.ServiceId}, resource : {address}");

                list.Add(address);
            }

            return list;
        }

        private bool VerifyInstanceStatus(int? status)
        {
            // 如果当前应用不含有该实例，那么这个服务不应该被加入到可用实例列表中
            if (status.HasValue == false)
            {
                return false;
            }

            // 如果服务没有启动，那么这个服务不应该被加入到可用实例列表中
            if ((status.Value & (int)ServiceStatus.Up) != (int)ServiceStatus.Up)
            {
                return false;
            }

            // 如果服务没有被拉入，那么这个服务不应该被加入到可用实例列表中
            if ((status.Value & (int)ServiceStatus.PullIn) != (int)ServiceStatus.PullIn)
            {
                return false;
            }

            return true;
        }

        public enum ServiceStatus
        {
            Down = 1,
            Up = 2,
            PullOut = 4,
            PullIn = 8
        }

        private List<SoaInfo> QueryServiceByAppId(string appId)
        {
            var list = new List<SoaInfo>();
            foreach (var info in resources)
            {
                if (info.Value.AppId.Equals(appId, StringComparison.InvariantCultureIgnoreCase))
                {
                    list.Add(info.Value);
                }
            }

            return list;
        }
    }
}

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter.Bean;
using XFramework.Resource.Center.Server.Business.Error;

namespace XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter
{
    public class LocalAppResourceDataAdapter : IAppResourceDataAdapter
    {
        protected ILogger logger = LogProvider.Create(typeof(LocalAppResourceDataAdapter));
        /// <summary>
        /// 资源锁对象
        /// </summary>
        private readonly object _syncObject = new object();

        private readonly ConcurrentDictionary<string, AppInfo> applications = 
            new ConcurrentDictionary<string, AppInfo>();
        private const string ResourceFile = "AppResource.xml";

        public LocalAppResourceDataAdapter()
        {
            var xmlSerializer = new XmlSerializer(typeof(AppResource));

            using (var fs = new FileStream(ResourceFile, FileMode.Open))
            {
                var resource = (AppResource)xmlSerializer.Deserialize(fs);
                if (resource.Applications == null || resource.Applications.Count == 0) return;

                foreach (var app in resource.Applications)
                {
                    var appId = app.AppId?.Trim().ToLower();

                    if (string.IsNullOrEmpty(appId))
                    {
                        logger.Info("Application id is empty, current application will be ignored");
                        continue;
                    }
                    
                    // 检查重复性，统一采用小写的格式
                    if (applications.ContainsKey(appId))
                    {
                        logger.Info($"Duplication application id : {appId}, current application will be ignored");
                        continue;
                    }

                    applications.TryAdd(appId, app);
                }

                logger.Info($"Load {applications.Count} applications");
            }
        }

        public AppInfo Query(string appId)
        {
            if (string.IsNullOrEmpty(appId))
            {
                throw new ApplicationResourceServiceException(ErrorCode.InvalidAppId, "App Id is empty");
            }

            if (applications.TryGetValue(appId.ToLower(), out var appInfo) == false)
            {
                throw new ApplicationResourceServiceException(ErrorCode.InvalidAppId, "Invalid app Id");
            }

            return appInfo;
        }

        public int UpdateStatus(string appId, string instanceId, int status)
        {
            InstanceInfo instance = GetInstance(appId, instanceId);

            int target = CalculateStatus(status, instance.ServerStatus);

            logger.Info($"应用{appId}的实例{instance.InstanceId}状态由 {instance.ServerStatus} 更新为 {target} ");
            instance.ServerStatus = target;

            return target;
        }
        /// <summary>
        /// 根据传入的状态计算目标状态，
        /// 传入为负数代表状态取消，正数为状态设置
        /// </summary>
        /// <param name="status"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        private static int CalculateStatus(int status, int currentStatus)
        {
            if (status > 0)
                return currentStatus | status;
            else
                return currentStatus & (int.MaxValue + status);
        }

        private InstanceInfo GetInstance(string appId, string instanceId)
        {
            var appInfo = Query(appId);
            if (string.IsNullOrEmpty(instanceId))
            {
                throw new ApplicationResourceServiceException
                    (ErrorCode.InvalidInstanceId, "InstanceId is empty");
            }

            if (appInfo.AppInstances == null || appInfo.AppInstances.Count == 0)
            {
                throw new ApplicationResourceServiceException
                    (ErrorCode.InvalidInstanceId, $"No instance available in current app : {appInfo.AppId}");
            }

            // 查找应用程序对应的实例
            var instance = appInfo.AppInstances.FindLast
                (p => p.InstanceId.Equals(instanceId, StringComparison.InvariantCultureIgnoreCase));
            if (instance == null)
            {
                throw new ApplicationResourceServiceException
                    (ErrorCode.InvalidInstanceId, $"The iinstance {instanceId} is not exist in app : {appInfo.AppId}");
            }

            return instance;
        }
    }
}

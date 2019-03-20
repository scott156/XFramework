using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Core.Abstractions.Error;
using XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter;
using XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter.Bean;
using XFramework.Resource.Center.Server.Contract.Query;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Resource.Center.Server.Service.Query
{
    /// <summary>
    /// Resource.础信息查询
    /// </summary>
    [Soa("resource", "appinfoquery")]
    public class AppInfoQueryService : SoaService<AppInfoQueryRequestType, AppInfoQueryResponseType>
    {
        public override string Description => "App资源信息查询";

        public override void Verify(AppInfoQueryRequestType request)
        {
            if (string.IsNullOrEmpty(request.AppId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidAppId, "AppId不能为空");
            }
        }

        public override Dictionary<string, string> LogTag(AppInfoQueryRequestType request)
        {
            return new Dictionary<string, string> { { "appid", request.AppId } };
        }
        
        public IAppResourceDataAdapter AppResourceDataAdpater { get; set; }

        public override async Task<AppInfoQueryResponseType> Process(AppInfoQueryRequestType request)
        {
            await Task.Run(() =>
            {

            });

            var appInfo = AppResourceDataAdpater.Query(request.AppId);
            
            return new AppInfoQueryResponseType()
            {
                AppId = appInfo.AppId,
                Description = appInfo.Description,
                Owners = appInfo.Owners,
                Instances = appInfo.AppInstances?.ConvertAll(Converter)
            };
        }

        public InstanceInfoType Converter(InstanceInfo instanceInfo)
        {
            return new InstanceInfoType()
            {
                ContainerId = instanceInfo.ContainerId,
                Cpu = instanceInfo.Cpu,
                Memory = instanceInfo.Memory,
                InstanceId = instanceInfo.InstanceId,
                ResourceId = instanceInfo.ResourceId,
                Port = instanceInfo.Port,
                ServerStatus = instanceInfo.ServerStatus
            };
        }
    }
}

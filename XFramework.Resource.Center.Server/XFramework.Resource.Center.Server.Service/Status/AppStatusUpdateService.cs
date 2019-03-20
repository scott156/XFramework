using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Configuration.Client;
using XFramework.Configuration.Client.Data;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter;
using XFramework.Resource.Center.Server.Contract.StatusUpdate;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Resource.Center.Server.Service.Status
{
    /// <summary>
    /// 提供App实例状态更新的服务
    /// </summary>
    [Soa("resource", "appstatusupdate")]
    public class AppStatusUpdateService : SoaService<AppStatusUpdateRequestType, SoaResponseType>
    {
        public override string Description => "App状态更新";

        /// <summary>
        /// 日志Tag
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Dictionary<string, string> LogTag(AppStatusUpdateRequestType request)
        {
            return new Dictionary<string, string> { { "instanceid", request.InstanceId } };
        }

        /// <summary>
        /// 用于操作App资源数据的适配器
        /// </summary>
        public IAppResourceDataAdapter DataAdapter { get; set; }

        public IDynamicConfigModifier ConfigModifier { get; set; }
        
        public override async Task<SoaResponseType> Process(AppStatusUpdateRequestType request)
        {
            await Task.Run(() =>
            {

            });

            // 更新App实例的状态
            var status = DataAdapter.UpdateStatus(request.AppId, request.InstanceId,
                (int)request.Operation * (int)request.ServerStatus);

            // 需要同步通知配置中心
            ConfigModifier.Update($"{request.AppId}.status.properties", new List<ModifiedPropertyInfo>
            {
                new ModifiedPropertyInfo()
                {
                    Key = request.InstanceId.ToLower(),
                    Value = status.ToString()
                }
            });

            return new SoaResponseType();
        }

        public override void Verify(AppStatusUpdateRequestType request)
        {
            if (string.IsNullOrEmpty(request.AppId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidAppId, "AppId不能为空");
            }

            if (string.IsNullOrEmpty(request.InstanceId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidInstanceId, "App实例Id不能为空");
            }

            if (request.Operation == 0)
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidRequest, "Operation不能为空");
            }

            if (request.ServerStatus == 0)
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidRequest, "状态不能为空");
            }
        }
    }
}

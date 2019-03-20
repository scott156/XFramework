using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Contract.SoaCenter.Apply;
using XFramework.Core.Abstractions.Error;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;
using XFramework.Soa.Center.Server.Business.Data.Adapter;

namespace XFramework.Soa.Center.Server.Service.Register
{
    [Soa("soa", "apply")]
    public class SoaApplyService : SoaService<SoaApplyRequestType, SoaResponseType>
    {
        public override string Description => "Soa申请服务";

        public override Dictionary<string, string> LogTag(SoaApplyRequestType request)
        {
            return new Dictionary<string, string> { { "service", request.ServiceId }, { "appId", request.AppId } };
        }

        public ISoaResourceDataAdapter DataAdapter { get; set; }

        public override async Task<SoaResponseType> Process(SoaApplyRequestType request)
        {
            await Task.Run(() =>
            {
                // Soa服务申请
                DataAdapter.Apply(request.ServiceId, request.AppId, request.Description);
            });
            
            return new SoaResponseType();
        }

        /// <summary>
        /// 校验请求
        /// </summary>
        /// <param name="request"></param>
        public override void Verify(SoaApplyRequestType request)
        {
            if (string.IsNullOrEmpty(request.AppId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidAppId, "AppId不能为空");
            }

            if (string.IsNullOrEmpty(request.ServiceId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidSoaServiceId, "Soa服务Id不能为空");
            }
        }
    }
}

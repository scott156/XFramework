using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Contract.SoaCenter.Query;
using XFramework.Core.Abstractions.Error;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;
using XFramework.Soa.Center.Server.Business.Data;
using XFramework.Soa.Center.Server.Business.Data.Adapter;

namespace XFramework.Soa.Center.Server.Service.Query
{
    [Soa("soa", "query")]
    public class SoaQueryService : SoaService<SoaQueryRequestType, SoaQueryResponseType>
    {
        public override string Description => "Soa查询服务";

        public override Dictionary<string, string> LogTag(SoaQueryRequestType request)
        {
            return new Dictionary<string, string> { { "service", request.ServiceId } };
        }

        public ISoaResourceDataAdapter DataAdapter { get; set; }

        public override async Task<SoaQueryResponseType> Process(SoaQueryRequestType request)
        {
            await Task.Run(() =>
            {

            });

            var info = DataAdapter.Query(request.ServiceId);

            return new SoaQueryResponseType()
            {
                AppId = info.AppId,
                Description = info.Description,
                ServiceId = info.ServiceId,
                RegisteredInstances = info.RegisteredResources?.ConvertAll(Convert)
            };
        }

        private InstanceInfoType Convert(ResourceInfo info)
        {
            return new InstanceInfoType()
            {
                InstanceId = info.InstanceId
            };
        }
        
        /// <summary>
        /// 校验请求
        /// </summary>
        /// <param name="request"></param>
        public override void Verify(SoaQueryRequestType request)
        {
            if (string.IsNullOrEmpty(request.ServiceId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidSoaServiceId, "Soa服务Id不能为空");
            }
        }
    }
}

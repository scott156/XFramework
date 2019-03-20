using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Contract.SoaCenter.Register;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;
using XFramework.Soa.Center.Server.Business.Data.Adapter;

namespace XFramework.Soa.Center.Server.Service.Register
{
    [Soa("soa", "register")]
    public class SoaQueryService : SoaService<SoaRegisterRequestType, SoaResponseType>
    {
        public override string Description => "Soa自注册服务";

        public override Dictionary<string, string> LogTag(SoaRegisterRequestType request) =>
            new Dictionary<string, string> { { "service", request.ServiceId }, { "instance", request.InstanceId } };

        public ISoaResourceDataAdapter DataAdapter { get; set; }

        public override async Task<SoaResponseType> Process(SoaRegisterRequestType request)
        {
            if (IsDevelopment(request.Header.Enviroment))
            {
                logger.Info("The request source is a development region instance, ignore register this time");

                return new SoaResponseType()
                {
                    Header = new SoaResponseHeader()
                    {
                        Remark = "请求的实例为开发环境，放弃注册"
                    }
                };
            }

            await Task.Run(() =>
            {
                // 注册
                DataAdapter.Register(request.ServiceId, request.InstanceId);
            });

            return new SoaResponseType();
        }

        private bool IsDevelopment(string enviorment)
        {
            if (string.IsNullOrWhiteSpace(enviorment))
            {
                return true;
            }

            if ("Development".Equals(enviorment, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 校验请求
        /// </summary>
        /// <param name="request"></param>
        public override void Verify(SoaRegisterRequestType request)
        {
            if (string.IsNullOrEmpty(request.InstanceId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidInstanceId, "InstanceId不能为空");
            }

            if (string.IsNullOrEmpty(request.ServiceId))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidSoaServiceId, "Soa服务Id不能为空");
            }
        }
    }
}

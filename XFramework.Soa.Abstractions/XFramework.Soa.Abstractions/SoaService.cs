using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Interface;

namespace XFramework.Soa.Abstractions
{
    public abstract class SoaService<Req, Resp> : ISoaService<Req, Resp>
        where Req : SoaRequestType, new()
        where Resp : SoaResponseType, new()
    {
        /// <summary>
        /// 派生类可以使用该日志进行处理
        /// </summary>
        protected ILogger logger = LogProvider.Create(typeof(SoaService<Req, Resp>));
        
        /// <summary>
        /// 当前Soa服务的说明
        /// </summary>
        public abstract string Description { get; }

        public abstract Dictionary<string, string> LogTag(Req request);

        /// <summary>
        /// 对请求进行校验
        /// </summary>
        /// <param name="request"></param>
        public abstract void Verify(Req request);

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="request">Soa请求</param>
        /// <returns>Soa响应</returns>
        public abstract Task<Resp> Process(Req request);

        /// <summary>
        /// 组装失败响应的默认方法
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public virtual Resp BuildFailureResponse(FrameworkException exception) {
            Resp resp = new Resp();
            
            return new Resp()
            {
                Header = new SoaResponseHeader()
                {
                    ResponseCode = exception.ErrorCode,
                    Remark = exception.Message
                }
            };
        }
    }
}
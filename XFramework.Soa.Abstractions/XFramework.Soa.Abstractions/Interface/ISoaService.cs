using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Soa.Abstractions.Interface
{
    public interface ISoaService<Req, Resp> : IService
        where Req : SoaRequestType, new()
        where Resp : SoaResponseType, new()
    {
        /// <summary>
        /// 校验请求
        /// </summary>
        /// <param name="request">Soa请求</param>
        void Verify(Req request);

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="request">Soa请求</param>
        /// <returns></returns>
        Task<Resp> Process(Req request);

        /// <summary>
        /// 获取日志Tag
        /// </summary>
        /// <param name="request">请求</param>
        /// <returns>日志Tag</returns>
        Dictionary<string, string> LogTag(Req request);
    }
}

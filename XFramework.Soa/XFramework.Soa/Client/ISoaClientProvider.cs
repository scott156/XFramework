using System.Threading.Tasks;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Soa.Client
{
    /// <summary>
    /// 用于访问Soa服务, 后续增加异步调用支持
    /// </summary>
    public interface ISoaClientProvider
    {
        Task<Resp> Call<Req, Resp>(string serviceId, string operationName, Req request, int timeoutInMs = 35000)
            where Req : SoaRequestType
            where Resp : SoaResponseType;        

        Task<Resp> Call<Req, Resp>(string uri, string serviceId, string operationName, Req request, int timeoutInMs = 35000)
            where Req : SoaRequestType
            where Resp : SoaResponseType;
    }
}

using System.Threading.Tasks;
using XFramework.Core.Abstractions.Client.Serializer;

namespace XFramework.Core.Abstractions.Client
{
    /// <summary>
    /// 客户端
    /// </summary>
    public interface IServiceClient
    {
        Task<Resp> Call<Req, Resp>(string uri, Req request, int timeoutInMs, IServiceSerializer serializer);
    }
}

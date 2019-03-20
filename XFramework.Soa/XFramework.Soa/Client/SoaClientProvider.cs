using System;
using System.Text;
using System.Threading.Tasks;
using XFramework.Configuration.Client;
using XFramework.Core.Abstractions.Client;
using XFramework.Core.Abstractions.Client.Serializer;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Soa.Client
{
    public class SoaClientProvider : ISoaClientProvider
    {
        /// <summary>
        ///  默认使用Json序列化器
        /// </summary>
        private readonly IServiceSerializer serializer = new JsonSerializer();

        private readonly IServiceClient serviceClient = new HttpServiceClient();

        private const int DEFAULT_TIMEOUT_IN_MS = 35000;
        
        /// <summary>
        /// Soa自动寻址
        /// </summary>
        /// <typeparam name="Req"></typeparam>
        /// <typeparam name="Resp"></typeparam>
        /// <param name="serviceId"></param>
        /// <param name="operationName"></param>
        /// <param name="timeoutInMs"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Resp> Call<Req, Resp>(string serviceId, string operationName, Req request, int timeoutInMs = DEFAULT_TIMEOUT_IN_MS)
            where Req : SoaRequestType
            where Resp : SoaResponseType
        {
            var provider = DynamicConfigFactory.GetInstance($"soa.{serviceId}.properties");
            var address = provider.GetStringProperty("ServerList");

            if (string.IsNullOrEmpty(address))
            {
                throw new SoaClientException(ErrorCode.AppResourceUnavilable, "There's no effective instances available");
            }

            var addressList = address.Split(';');

            // TODO, 这里需要优化为Soa负载均衡策略
            var val = (new Random()).Next(addressList.Length - 1);

            return await Call<Req, Resp>(addressList[val], serviceId, operationName, request, timeoutInMs);
        }

        public async Task<Resp> Call<Req, Resp>
            (string uri, string serviceId, string operationName, Req request, int timeoutInMs = DEFAULT_TIMEOUT_IN_MS)
            where Req : SoaRequestType
            where Resp : SoaResponseType
        {
            if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(serviceId) || string.IsNullOrEmpty(operationName))
            {
                throw new FrameworkException(ErrorCode.InvalidUri,
                    $"Invalid arguments, url : {uri}, service Id : {serviceId}, " +
                    $"operation Name : {operationName}, any of these parameters is empty");
            }

            var address = BuildAddress(uri, serviceId, operationName);
            request.Header = BuildSoaHeader();

            return await serviceClient.Call<Req, Resp>(address, request, timeoutInMs, serializer);
        }

        /// <summary>
        /// 组装Soa头部信息
        /// </summary>
        /// <returns></returns>
        private SoaRequestHeaderType BuildSoaHeader()
        {
            AppSetting setting = AppSetting.GetInstance();
            // 重新设定Header
            return new SoaRequestHeaderType()
            {
                AppId = setting.AppId,
                Enviroment = setting.Enviroment?.EnvironmentName,
                TransactionId = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// 组装Http地址
        /// Http地址格式: http://ipaddress/api/[serviceId]/[operationName]
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="serviceId"></param>
        /// <param name="operationName"></param>
        /// <returns></returns>
        private string BuildAddress(string uri, string serviceId, string operationName)
        {
            var sb = new StringBuilder(uri.Length + serviceId.Length + operationName.Length + 10);

            uri = uri.Trim();
            if (!uri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                sb.Append("http://");
            }

            sb.Append(uri);

            if (!uri.EndsWith("/"))
            {
                sb.Append("/");
            }

            sb.Append($"api/{serviceId}/{operationName}");

            return sb.ToString();
        }
    }
}

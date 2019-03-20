using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using XFramework.Core.Abstractions.Client.Serializer;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;

namespace XFramework.Core.Abstractions.Client
{
    /// <summary>
    /// 基于Http的请求
    /// </summary>
    public class HttpServiceClient : IServiceClient
    {
        private readonly ILogger logger = LogProvider.Create(typeof(HttpServiceClient));
        /// <summary>
        /// 默认超时时间：30秒
        /// </summary>
        private const int DEFAULT_TIMEOUT_IN_MS = 35000;
        /// <summary>
        /// 最小的超时时间
        /// </summary>
        private const int MINIMAL_TIMEOUT_IN_MS = 100;
        

        private static int GetTimeout(int timeoutInMs)
        {
            if (timeoutInMs < 100)
            {
                timeoutInMs = MINIMAL_TIMEOUT_IN_MS;
            }

            return timeoutInMs;
        }

        public async Task<Resp> Call<Req, Resp>(string uri, Req request, int timeoutInMs, IServiceSerializer serializer)
        {
            using (var client = new HttpClient())
            {
                logger.Debug($"http request : {JsonConvert.SerializeObject(request, Formatting.Indented)}");
                var sw = new Stopwatch();
                sw.Start();

                // 设置超时时间
                var timeout = GetTimeout(timeoutInMs);
                logger.Debug($"Http client : {uri}, timeout : {timeout}");
                client.Timeout = new TimeSpan(0, 0, 0, timeout);

                ByteArrayContent content = new ByteArrayContent(serializer.Serialize<Req>(request));
                var response = await client.PostAsync(uri, content);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new FrameworkException(ErrorCode.InvalidHttpResponse, 
                        $"Access {uri} failed, responsed code : {response.StatusCode}");
                }

                var buffer = await response.Content.ReadAsByteArrayAsync();

                var resp = serializer.Deserialize<Resp>(buffer);
                logger.Debug($"Total elapsed : {sw.Elapsed.TotalSeconds.ToString("0.000")}s, " +
                    $"response : {Environment.NewLine}{JsonConvert.SerializeObject(resp, Formatting.Indented)}");

                return resp;
            }
        }
    }
}

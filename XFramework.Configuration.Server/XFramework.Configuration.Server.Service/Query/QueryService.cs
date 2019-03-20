using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Configuration.Server.Service.Data.DataAdapter.DataAdapter;
using XFramework.Configuration.Server.Service.Error;
using XFramework.Configuration.Server.Service.Query.Data;
using XFramework.Contract.Configuration.Query;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Configuration.Server.Service.Query
{
    /// <summary>
    /// 配置中心配置查询服务
    /// 采用Long Pooling的方式，默认超时时间为30秒超时
    /// </summary>
    [Soa("configuration", "query")]
    public class QueryService : SoaService<ConfigurationQueryRequestType, ConfigurationQueryResponseType>
    {
        /// <summary>
        /// 默认的长轮询超时时间
        /// </summary>
        private const int DEFAULT_LONG_POOLING_TIMEOUT_IN_MS = 30 * 1000;

        /// <summary>
        /// 对请求进行校验
        /// </summary>
        /// <param name="request">Soa请求</param>
        public override void Verify(ConfigurationQueryRequestType request)
        {
            if (string.IsNullOrEmpty(request.File))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidRequest, "配置中心的文件名不能为空");
            }
        }

        /// <summary>
        /// 配置当前请求的Tag，便于日志中心化以后，使用Tag进行查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Dictionary<string, string> LogTag(ConfigurationQueryRequestType request)
        {
            return new Dictionary<string, string>() { { "file", request.File } };
        }

        public IConfigurationDataAdapter DataAdapter { get; set; }

        public override async Task<ConfigurationQueryResponseType> Process(ConfigurationQueryRequestType request)
        {
            logger.Info($"配置信息查询 : filename {request.File}, version : {request.CurrentVersion}");

            ConfigurationContent content = null;
            await Task.Run(() =>
            {
                content = DataAdapter.GetContent(request.File);
                if (content == null)
                {
                    throw new ConfigurationQueryException
                        (ErrorCode.InvalidConfigurationFile, $"无效的配置文件名 : {request.File}");
                }
            });

            // 这里要注意并发可能导致的问题
            // TODO
            // 如果版本检查时是相同的，但是检查后发生了变化，并且已经触发了当前的所有事件，那么该注册的事件可能不会被触发，直到超时。
            if (request.CurrentVersion == content.Version)
            {
                logger.Info($"配置文件版本没有发生变化，等待中");
                // 注册监听事件
                var manualEvent = DataAdapter.Register(request.File);
                manualEvent.Wait(DEFAULT_LONG_POOLING_TIMEOUT_IN_MS);

                if (manualEvent.IsSet)
                {
                    return BuildResponse(DataAdapter.GetContent(request.File));
                }
                else
                {
                    throw new ConfigurationQueryException
                        (ErrorCode.ConfigurationNotChanged, $"配置文件没有发生变化，操作超时");
                }
            }

            return BuildResponse(content);
        }
        
        private ConfigurationQueryResponseType BuildResponse(ConfigurationContent content)
        {
            var response = new ConfigurationQueryResponseType
            {
                Version = content.Version,
                LastModifyDate = content.LastModifyDate,
                File = content.Filename
            };

            if (content.Properties == null || content.Properties.Count == 0) return response;

            response.Properties = new List<ConfigurationItemType>();
            content.Properties.ForEach(p => response.Properties.Add(new ConfigurationItemType()
            {
                Key = p.Key,
                Value = p.Value
            }));

            return response;
        }

        public override string Description => "配置中心查询服务";
    }
}

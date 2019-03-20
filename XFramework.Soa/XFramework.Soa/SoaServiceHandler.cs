using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using XFramework.Core.Abstractions;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Error;
using XFramework.Soa.Abstractions.Interface;
using XFramework.Soa.Abstractions.Data;
using System.Threading.Tasks;

namespace XFramework.Soa
{
    public class SoaServiceHandler : IServiceHandler
    {
        private ILogger logger = LogProvider.Create(typeof(SoaServiceHandler));

        public string Router => "api";

        public string Name => "XFramework.SOA Service";

        /// <summary>
        /// 对请求进行处理 http://address/api/[serviceid]/[interface]
        /// 例如: http://192.168.1.100:5000/api/configuration/query
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> Process(HttpContext context)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var response = await ProcessRequest(context);

            logger.Info("Serialize response");
            var resp = JsonConvert.SerializeObject(response);

            logger.Debug($"Response : [{resp}]");

            sw.Stop();
            logger.Info($"Request process finished, total elapsed {sw.ElapsedMilliseconds.ToString("0.00")}ms. " +
                $"Response code : {response.Header.ResponseCode}");

            return resp;
        }

        private async Task<SoaResponseType> ProcessRequest(HttpContext context)
        {
            try
            {
                var entity = SoaServiceProvider.GetInstance().GetService(context.Request.Path, Router);

                using (logger.BeginScope(new KeyValuePair<string, string>("soa.service", entity.ToString())))
                {
                    try
                    {
                        return await RequestHandler(context, entity);
                    }
                    catch (Exception e2)
                    {
                        if (e2.InnerException != null) e2 = e2.InnerException;
                        logger.Error(e2, "Soa请求处理失败");
                        return BuildFailureResponse(e2);
                    }
                }
            }
            catch (SoaServiceException e1)
            {
                logger.Error(e1, "Soa request process failure");
                return BuildFailureResponse(e1);
            }
        }

        /// <summary>
        /// 组装请求，
        /// 如果请求为空，那么新建一个该服务对应的请求的实例
        /// </summary>
        /// <param name="stream">请求数据流</param>
        /// <param name="requestType">Soa服务请求的类型</param>
        /// <returns>组装后的Soa请求</returns>
        private SoaRequestType BuildRequest(Stream stream, Type requestType)
        {
            using (var sr = new StreamReader(stream))
            {
                var content = sr.ReadToEnd();
                logger.Debug($"Request received : [{content}]");

                if (string.IsNullOrWhiteSpace(content))
                {
                    logger.Debug($"Request is empty, Initialize an empty request : {requestType}");
                    return (SoaRequestType)Activator.CreateInstance(requestType);
                }
                else
                {
                    logger.Info("Deserialize request.");
                    try
                    {
                        var request = (SoaRequestType)JsonConvert.DeserializeObject(content, requestType);
                        if (request.Header == null) request.Header = new SoaRequestHeaderType();

                        if (string.IsNullOrWhiteSpace(request.Header.TransactionId))
                        {
                            request.Header.TransactionId = Guid.NewGuid().ToString();
                        }

                        logger.Info($"Deserialize succeed, transaction id : {request.Header.TransactionId}");

                        return request;
                    }
                    catch (Exception e)
                    {
                        logger.Error("Deserialize request failed");
                        throw new SoaServiceException(ErrorCode.InvalidRequest, e);
                    }
                }
            }
        }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="entity">Soa服务信息实体</param>
        /// <returns>Soa响应</returns>
        private async Task<SoaResponseType> RequestHandler(HttpContext context, SoaServiceEntity entity)
        {
            return await Task.Run(() =>
            {
                var request = BuildRequest(context.Request.Body, entity.RequestType);

                // 通过DI获取
                IService service = (IService)context.RequestServices.GetService(entity.ServiceType);
                logger.Debug($"Create instance : {entity.ServiceType}, description : {service.Description}");

                var tags = (Dictionary<string, string>)entity.ServiceType.GetMethod("LogTag").Invoke(service, new object[] { request });
                LogProvider.GetInstance().Add(tags);

                logger.Info("Verify request");
                entity.ServiceType.GetMethod("Verify").Invoke(service, new object[] { request });

                logger.Info("Processing");

                var resp = entity.ServiceType.GetMethod("Process").Invoke(service, new object[] { request });
                dynamic task = resp;

                SoaResponseType response = task.Result;
                if (response == null) response = GetDefaultResponseEntity();

                return response;
            });
        }

        /// <summary>
        /// 组装默认的错误响应
        /// </summary>
        /// <returns></returns>
        private SoaResponseType GetDefaultResponseEntity()
        {
            return BuildFailureResponse(new FrameworkException(ErrorCode.InternalError, "内部错误"));
        }

        /// <summary>
        /// 组装失败的响应
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private SoaResponseType BuildFailureResponse(Exception exception)
        {
            var e = exception as FrameworkException;
            
            return new SoaResponseType
            {
                Header = new SoaResponseHeader()
                {
                    Remark = exception.Message,
                    ResponseCode = e == null ? (int)ErrorCode.InternalError : (int)e.ErrorCode
                }
            };
        }
       
        /// <summary>
        /// 根据配置，加载Soa服务对应的类，并获取各个服务类对应的请求类型和响应类型
        /// </summary>
        public void Init(IServiceCollection services)
        {
            SoaServiceProvider.GetInstance().ConfigService(services);
        }
    }
}
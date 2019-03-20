using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Logger;

namespace XFramework.Core.ServiceHandler
{
    /// <summary>
    /// 用于处理XFramework框架支持的请求
    /// 包括SOA, Message等
    /// </summary>
    public class ServiceHandlerMiddleware
    {
        private ILogger logger = LogProvider.Create(typeof(ServiceHandlerMiddleware));
        private readonly RequestDelegate next;
        // 获取应用配置
        private AppSetting appInfo = AppSetting.GetInstance();

        public ServiceHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// 处理请求
        /// 接受请求,映射到对应的Service
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            using (logger.BeginScope(new KeyValuePair<string, string>("middleware", "xframework.servicehandler")))
            {
                var router = GetBaseRouter(context.Request.Path);
                logger.Info("Request path : {0}, router : {1}", context.Request.Path, router);

                var service = ServiceHandlerProvider.GetInstance().GetService(router, context.RequestServices);
                if (service == null)
                {
                    // 自身无法处理的话交给下一个中间件处理, 完成后退出
                    await next(context);
                    return;
                }

                using (logger.BeginScope(new KeyValuePair<string, string>("servicehandler", service.Name)))
                {
                    // 强制使用UTF8作为返回的Encoding
                    context.Response.ContentType = "application/json; charset=UTF-8";
                    // 这里要注意是，如果能被这个Middleware处理，它则是最后一个中间件，如果不能被它处理，则放过，让404中间件进行处理
                    var response = await service.Process(context);
                    await context.Response.WriteAsync(response);
                }
            }
        }

        /// <summary>
        /// 获取基础的路由信息
        /// 路径示例： http://soa.techcenter.[uat].com/api/configurationservice/getconfiguration
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string GetBaseRouter(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var array = path.Split('/');
            if (array.Length >= 2)
            {
                return array[1].ToLower();
            }

            return null;
        }
    }
}

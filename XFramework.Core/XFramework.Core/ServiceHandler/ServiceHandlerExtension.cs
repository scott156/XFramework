using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Logger;

namespace XFramework.Core.ServiceHandler
{
    /// <summary>
    /// XServiceHandler扩展
    /// </summary>
    public static class ServiceHandlerExtension
    {
        private static readonly ILogger logger = LogProvider.Create(typeof(ServiceHandlerExtension));
        /// <summary>
        /// 提供Use方法，使用XFramework架构对请求进行处理
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseXServiceHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ServiceHandlerMiddleware>();
        }

        /// <summary>
        /// 将XFramework架构添加到服务中，便于引入Ioc
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddXServiceHandler
            (this IServiceCollection services, IConfiguration configuration, IHostingEnvironment environment)
        {
            var setting = AppSetting.GetInstance();
            // 初始化配置文件
            configuration.Bind(setting);
            setting.Enviroment = environment;

            try
            {
                // 初始化
                ServiceHandlerProvider.GetInstance().ConfigService(services);
            }
            catch (Exception e)
            {
                logger.Error(e, "Service handler start up failure");
                // 销毁LogProvider，确保日志记录完整
                LogProvider.GetInstance().Dispose();
                throw e;
            }

            return services;
        }
    }
}

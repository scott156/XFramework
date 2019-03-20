using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using XFramework.Core.Abstractions;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Core.Abstractions.Utility;

namespace XFramework.Core.ServiceHandler
{
    /// <summary>
    /// 管理和维护当前应用声明的XService
    /// </summary>
    internal class ServiceHandlerProvider : Singleton<ServiceHandlerProvider>
    {
        private ILogger logger = LogProvider.Create(typeof(ServiceHandlerProvider));

        /// <summary>
        /// 存放已经配置的XServiceHandler
        /// </summary>
        private readonly Dictionary<string, Type> handlers = new Dictionary<string, Type>();

        public IServiceHandler GetService(string router, IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(router))
            {
                logger.Debug("Request router is null, skip current process by XServiceHandler");
                return null;
            }

            router = router.ToLower().Trim();
            if (handlers == null || !handlers.ContainsKey(router))
            {
                logger.Debug($"XServiceHandler is not able to process '{router}'");
                return null;
            }

            var handler = (IServiceHandler)serviceProvider.GetService(handlers[router]);
            if (handler == null)
            {
                throw new FrameworkException(ErrorCode.InvalidXServiceHandler,
                    $"Create '{handlers[router].FullName}' handler failed, " +
                    $"IServiceHandler is not implemented or not able to create instance for that.");
            }
            
            return handler;
        }

        public void ConfigService(IServiceCollection services)
        {
            logger.Info($"AppId : {AppSetting.GetInstance().AppId}");
            logger.Info("Start XServiceHandler");
            var handlerList = AppSetting.GetInstance().ServiceHandler;

            if (handlerList == null || handlerList.Count == 0)
            {
                throw new FrameworkException(ErrorCode.InvalidXServiceHandler,
                    $"ServiceHandler node is not configured, system can not process any dynamic request");
            }

            foreach (var typeName in handlerList)
            {
                logger.Info($"Initialize service handler : {typeName}");
                var instance = GetServiceHandler(typeName);
                // 初始化XService的各项服务
                instance.Init(services);
                
                var router = instance.Router.ToLower().Trim();
                
                if (handlers.ContainsKey(router))
                {
                    logger.Warn($"Service handler '{typeName}' is duplicate, {handlers[router].GetType().FullName} is loaded, "
                    + $"They have same router infomation : {router}, current service will not started");
                    continue;
                }
                
                services.AddSingleton(instance.GetType(), instance);
                handlers.Add(instance.Router.Trim().ToLower(), instance.GetType());
                logger.Info($"Service handler {instance.Name}({instance.GetType().FullName})" +
                    $" loaded successfully, router path : {router}");
            }
        }

        private IServiceHandler GetServiceHandler(string typeName)
        {
            var type = GetHandlerType(typeName);

            // 反射出对应的服务处理类
            var handler = Activator.CreateInstance(type) as IServiceHandler;
            if (handler == null)
            {
                throw new FrameworkException(ErrorCode.InvalidXServiceHandler,
                    $"Create '{typeName}' handler failed, IServiceHandler is not implemented or not able to create instance for that.");
            }

            if (string.IsNullOrWhiteSpace(handler.Router))
            {
                throw new FrameworkException(ErrorCode.InvalidXServiceHandler,
                    $"创建'{typeName}'控制类失败, 该控制器的路由信息为空，请联系控制器提供方");
            }

            return handler;
        }

        private Type GetHandlerType(string handler)
        {
            var type = Type.GetType(handler);
            if (type == null)
            {
                throw new FrameworkException(ErrorCode.XServiceInitFailure,
                    $"XService加载失败, 无效的XServiceHandler类型 : {type}");
            }

            return type;
        }
    }
}

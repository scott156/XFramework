using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using XFramework.Configuration.Client;
using XFramework.Contract.SoaCenter.Register;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Core.Abstractions.Utility;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;
using XFramework.Soa.Abstractions.Interface;
using XFramework.Soa.Client;

namespace XFramework.Soa
{
    internal class SoaServiceProvider : Singleton<SoaServiceProvider>
    {
        private ILogger logger = LogProvider.Create(typeof(SoaServiceProvider));

        private Dictionary<string, Dictionary<string, SoaServiceEntity>> serviceList =
            new Dictionary<string, Dictionary<string, SoaServiceEntity>>();

        /// <summary>
        /// 查找请求对应的Soa服务类
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public SoaServiceEntity GetService(string path, string router)
        {
            var (service, operation) = GetServiceInfo(path, router);
            if (!serviceList.ContainsKey(service))
            {
                throw new SoaServiceException(ErrorCode.InvalidSoaServiceId, $"Map to service : {service} failed");
            }

            if (!serviceList[service].ContainsKey(operation))
            {
                throw new SoaServiceException(ErrorCode.InvalidSoaOperation,
                    $"Could not found opeartion '{operation}' in '{service}' service");
            }

            var entity = serviceList[service][operation];
            logger.Info($"Mapping {service}.{operation} to {entity.ServiceType}");

            return entity;
        }

        /// <summary>
        /// 获取服务信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private (string service, string operation) GetServiceInfo(string path, string router)
        {
            var routers = path.Split('/');
            // /api/[serviceId]/[interface]
            if (routers.Length < 4)
            {
                throw new SoaServiceException(ErrorCode.InvalidRequestUrl, 
                    $"Invalid request path, sample : '/{router}/Domain/Service'.");
            }

            return (routers[2].ToLower(), routers[3].ToLower());
        }

        /// <summary>
        /// Soa listener定义的是[serviceId],[AssemblyName]
        /// </summary>
        private const char SOA_LISTENER_SEPERATOR = ',';
        private const int SOA_LISTENER_DEF_LEN = 2;

        public void ConfigService(IServiceCollection services)
        {
            var soaServices = AppSetting.GetInstance().SoaServiceListener;
            if (soaServices == null || soaServices.Count == 0)
            {
                logger.Warn("None of soa service listener was registered in appsetting.json file, " +
                    "please check 'SoaServiceListener' node");
                return;
            }

            foreach (var service in soaServices)
            {
                if (string.IsNullOrEmpty(service))
                {
                    throw new FrameworkException(ErrorCode.InvalidSoaListener,
                        "Soa service listener is empty, please check appsetting.json file");
                }

                var value = service.Split(SOA_LISTENER_SEPERATOR);               
                if (value.Length != SOA_LISTENER_DEF_LEN)
                {
                    throw new FrameworkException(ErrorCode.InvalidSoaListener,
                        "Invalid soa service listener, the correct format is [listener],[Assembly]");
                }

                // SoaServiceListener 正确的注册格式为 : [serviceId], [AssemblyName]
                var serviceId = value[0].Trim().ToLower();
                var assemblyName = value[1].Trim();

                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                {
                    throw new FrameworkException(ErrorCode.InvalidSoaListener,
                        $"Invalid soa service listener, unable to load assembly {value[1].Trim()}");
                }

                foreach (var type in assembly.GetExportedTypes())
                {
                    var entity = GetSoaServiceEntity(type);

                    if (entity == null) continue;

                    // 配置中的ServiceId信息和实际代码中的信息不一致, 则忽略当前的Soa实现.
                    if (!serviceId.Equals(entity.Service, StringComparison.InvariantCultureIgnoreCase))
                    {
                        logger.Warn($"Service Id is not matched, found other soa service " +
                            $"{entity.Service}.{entity.Operation} defined in assembly {assemblyName}." +
                            $"Current service Id is : {serviceId}, This service will be ignored");
                        continue;
                    }

                    // 保存Soa服务的信息
                    SaveSoaService(entity, services);
                }
                
                // Soa self regisition
                RegisterService(serviceId, AppSetting.GetInstance().InstanceId);
            }
        }

        private readonly ISoaClientProvider client = new SoaClientProvider();

        /// <summary>
        /// Soa服务自注册
        /// </summary>
        /// <param name="serviceId"></param>
        /// <param name="instanceId"></param>
        private void RegisterService(string serviceId, string instanceId)
        {
            logger.Info($"Soa self registration : {serviceId}-{instanceId}");
            
            if (string.IsNullOrEmpty(instanceId))
            {
                logger.Warn("Instance id is empty, soa self registration will be ignored");
                return;
            }

            var result = client.Call<SoaRegisterRequestType, SoaResponseType>
                (DynamicConfigFactory.GetInstance("global.properties").GetStringProperty("SoaCenter"), 
                "soa", "register", new SoaRegisterRequestType()
            {
                ServiceId = serviceId,
                InstanceId = instanceId
            });

            var resp = result.Result;

            if (resp.Header.ResponseCode != (int)ErrorCode.Success)
            {
                throw new SoaServiceException(ErrorCode.SoaRegistionFailed, 
                    $"Soa self registion failure, error code : {resp.Header.ResponseCode}, reason : {resp.Header.ResponseCode}");
            }
        }
        
        /// <summary>
        /// 将Soa服务保存到Soa服务列表中
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="services"></param>
        private void SaveSoaService(SoaServiceEntity entity, IServiceCollection services)
        {
            if (serviceList.ContainsKey(entity.Service))
            {
                if (serviceList[entity.Service].ContainsKey(entity.Operation))
                {
                    logger.Warn($"Soa service found : {entity.Service}.{entity.Operation}, " +
                        $"fullname : {entity.ServiceType.FullName}, " +
                        $"but it was registered already. current service will not effected");
                    return;
                }

                serviceList[entity.Service].Add(entity.Operation, entity);
            }
            else
            {
                serviceList.Add(entity.Service, new Dictionary<string, SoaServiceEntity> { { entity.Operation, entity } });
            }

            
            logger.Info($"Soa service found : {entity.Service}.{entity.Operation}, fullname : " +
                $"{entity.ServiceType.FullName}, description : {entity.Description}");
            // 增加到Ioc容器中
            services.AddSingleton(entity.ServiceType);
        }

        /// <summary>
        /// Verify, load and register soa service
        /// </summary>
        /// <param name="type"></param>
        private SoaServiceEntity GetSoaServiceEntity(Type type)
        {
            if (!IsSoaTypeValid(type)) return null;

            // Create instance from soa service base on specific type
            IService instance = (IService)Activator.CreateInstance(type);

            SoaAttribute attr = (SoaAttribute)type.GetCustomAttributes(typeof(SoaAttribute), false)[0];

            var service = attr.Service.ToLower();
            var operation = attr.Operation.ToLower();
            
            return BuildServiceInfo(service, operation, type, instance.Description);
        }

        /// <summary>
        /// 判断类型是否符合Soa服务规范，包括
        /// 1. 不能是抽象类
        /// 2. 实现了ISoaService接口
        /// 3. 定义了SoaService属性
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool IsSoaTypeValid(Type type)
        {
            // 必须是引用类型，不能为抽象类
            if (!type.IsClass || type.IsAbstract)
                return false;

            if (!Array.Exists(type.GetInterfaces(), p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(ISoaService<,>)))

            // 必须实现SoaServiceAttribute属性，而且不能继承与父类
            if (!type.IsDefined(typeof(SoaAttribute), false))
                return false;

            // 获取SoaService特性定义
            SoaAttribute[] attrs = (SoaAttribute[])type.GetCustomAttributes(typeof(SoaAttribute), false);

            if (attrs == null || attrs.Length == 0) return false;

            // 获取第一个SoaServiceAttribute
            var attr = attrs[0];

            // 必须定义Service和Operation
            if (string.IsNullOrWhiteSpace(attr.Service) || string.IsNullOrWhiteSpace(attr.Operation))
            {
                logger.Warn($"Found soa service class : {type.FullName}, but either service({attr.Service}) " +
                    $"or operation({attr.Operation}) attribute is not defined.");
                return false;
            }

            if (!AppSetting.GetInstance().SoaServiceListener.Exists
                (p => p.StartsWith(attr.Service + SOA_LISTENER_SEPERATOR, StringComparison.InvariantCultureIgnoreCase)))
            {
                logger.Debug($"Found soa service : {attr.Service}, but this service listener is not registered.");
                return false;
            }

            return true;
        }

        private SoaServiceEntity BuildServiceInfo(string service, string operation, Type type, string description)
        {
            return new SoaServiceEntity()
            {
                Service = service,
                Operation = operation,
                ServiceType = type,
                RequestType = type.BaseType.GetGenericArguments()[0],
                Description = description,
                ResponseType = type.BaseType.GetGenericArguments()[1]
            };
        }
    }
}

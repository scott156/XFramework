using System.Collections.Generic;
using System.Threading.Tasks;
using XFramework.Configuration.Server.Service.Data.DataAdapter.DataAdapter;
using XFramework.Configuration.Server.Service.Error;
using XFramework.Configuration.Server.Service.Query.Data;
using XFramework.Contract.Configuration.Modify;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Data;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Configuration.Server.Service.Modify
{
    /// <summary>
    /// 配置中心配置查询服务
    /// 采用Long Pooling的方式，默认超时时间为30秒超时
    /// </summary>
    [Soa("configuration", "modify")]
    public class ModifyService : SoaService<ConfigurationModifyRequestType, SoaResponseType>
    {
        /// <summary>
        /// 对请求进行校验
        /// </summary>
        /// <param name="request">Soa请求</param>
        public override void Verify(ConfigurationModifyRequestType request)
        {
            if (string.IsNullOrEmpty(request.File))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidRequest, "配置中心的文件名不能为空");
            }

            if ((request.AddedProperties == null || request.AddedProperties.Count == 0)
                && (request.ModifiedProperties == null || request.ModifiedProperties.Count == 0))
            {
                throw new SoaServiceVerifyException(ErrorCode.InvalidRequest, "配置的修改和新增节点数值不能都为空");
            }
        }

        /// <summary>
        /// 配置当前请求的Tag，便于日志中心化以后，使用Tag进行查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override Dictionary<string, string> LogTag(ConfigurationModifyRequestType request)
        {
            return new Dictionary<string, string>() { { "file", request.File } };
        }

        public IConfigurationDataAdapter DataAdapter { get; set; }

        /// <summary>
        /// 修改配置信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<SoaResponseType> Process(ConfigurationModifyRequestType request)
        {
            await Task.Run(() =>
            {
                var config = DataAdapter.GetContent(request.File);
                if (config == null)
                {
                    throw new ConfigurationModifyException
                        (ErrorCode.InvalidConfigurationFile, $"无效的配置文件名 : {request.File}");
                }

                if (request.ForceOverride == false && config.Version != request.CurrentVersion)
                {
                    throw new ConfigurationModifyException
                        (ErrorCode.InvalidConfigurationVersion, $"配置文件版本信息校验错误，{request.File}.{request.CurrentVersion}, " +
                        $"当前版本 : {config.Version}, 可能是在您修改前，其他用户对该配置进行了更新");
                }

                var newConfig = (ConfigurationContent)config.Clone();
                // 更新配置信息
                UpdateAddedProperties(request.AddedProperties, newConfig);
                UpdateModifiedProperties(request.ModifiedProperties, newConfig);

                DataAdapter.Modify(request.File, newConfig);
            });

            return new SoaResponseType();
        }

        private void UpdateModifiedProperties(List<ModifiedCongiruationItemType> list, ConfigurationContent newConfig)
        {
            if (list == null || list.Count == 0) return;

            list.ForEach(p =>
            {
                if (string.IsNullOrWhiteSpace(p.Key))
                {
                    throw new ConfigurationModifyException
                    (ErrorCode.InvalidConfigurationItem, $"配置项的Key不能为空");
                }

                var item = newConfig.Properties.Find(c => c.Key.Equals(p.Key));
                if (item == null)
                {
                    throw new ConfigurationModifyException(ErrorCode.InvalidConfigurationItem, $"需要修改的配置项不存在 : {p.Key}");
                }
                
                // 空代表删除当前配置项
                if (p.Value == null)
                {
                    logger.Info($"删除配置 : {p.Key}, 原始值 : {item.Value}");
                    newConfig.Properties.Remove(item);
                }
                else
                {
                    logger.Info($"修改配置 : {p.Key}, 修改值 : {item.Value} -> {p.Value}");
                    item.Value = p.Value;
                }
            });
        }

        private void UpdateAddedProperties(List<ModifiedCongiruationItemType> list, ConfigurationContent newConfig)
        {
            if (list == null || list.Count == 0) return;

            list.ForEach(p =>
            {
                if (string.IsNullOrWhiteSpace(p.Key))
                {
                    throw new ConfigurationModifyException
                    (ErrorCode.InvalidConfigurationItem, $"配置项的Key不能为空");
                }

                if (newConfig.Properties.Exists(c => c.Key.Equals(p.Key)))
                {
                    throw new ConfigurationModifyException
                    (ErrorCode.InvalidConfigurationItem, $"新增配置项的Key重复 : {p.Key}");
                }

                newConfig.Properties.Add(new ConfigurationItem()
                {
                    Key = p.Key,
                    Value = p.Value
                });

                logger.Info($"新增配置 : {p.Key}, {p.Value}");
            });
        }

        public override string Description => "配置中心修改服务";
    }
}

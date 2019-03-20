using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using XFramework.Configuration.Client.Data;
using XFramework.Contract.Configuration.Modify;
using XFramework.Core.Abstractions.Client;
using XFramework.Core.Abstractions.Client.Serializer;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Error;
using XFramework.Core.Abstractions.Logger;
using XFramework.Soa.Abstractions.Contract;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Configuration.Client
{
    public class DynamicConfigModifier : IDynamicConfigModifier
    {
        private readonly ILogger logger = LogProvider.Create(typeof(DynamicConfigModifier));

        private readonly IServiceClient client = new HttpServiceClient();

        private readonly IServiceSerializer serailzer = new JsonSerializer();

        private const int DEFAULT_TIMEOUT_IN_MS = 60000;
        /// <summary>
        /// 更新配置信息
        /// </summary>
        /// <param name="file">配置文件名称</param>
        /// <param name="propertyInfo">配置信息</param>
        /// <returns></returns>
        public void Update(string file, List<ModifiedPropertyInfo> propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.Count == 0)
            {
                throw new FrameworkException(ErrorCode.InvalidParameter, "参数无效, 配置更新列表为空");
            }

            var request = new ConfigurationModifyRequestType()
            {
                File = file,
                AddedProperties = Convert(propertyInfo.FindAll(p => p.Modify == false)),
                ModifiedProperties = Convert(propertyInfo.FindAll(p => p.Modify == true)),
                ForceOverride = true
            };
            
            var result = client.Call<ConfigurationModifyRequestType, SoaResponseType>
                ($"{GetConfigurationServerUri()}/api/configuration/modify", request, DEFAULT_TIMEOUT_IN_MS, serailzer);

            var response = result.Result;
            if (response.Header.ResponseCode != (int)ErrorCode.Success)
            {
                logger.Error("Synchronize configuration system failure");
                throw new SoaClientException(ErrorCode.ConfiurationModifyFailure,
                    $"Configuration Update Failure, error code : {response.Header.ResponseCode}, remark : {response.Header.Remark}");
            }
        }

        private string GetConfigurationServerUri()
        {
            return $"http://{AppSetting.GetInstance().Enviroment.EnvironmentName.ToLower()}.configuration.colorstudio.com.cn";
        }

        private List<ModifiedCongiruationItemType> Convert(List<ModifiedPropertyInfo> propertyList)
        {
            if (propertyList == null || propertyList.Count == 0) return null;

            var list = new List<ModifiedCongiruationItemType>();
            foreach (var item in propertyList)
            {
                list.Add(new ModifiedCongiruationItemType()
                {
                    Key = item.Key,
                    Value = item.Value
                });
            }

            return list;
        }
    }
}

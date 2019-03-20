using XFramework.Core.Abstractions.Error;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Configuration.Server.Service.Error
{
    /// <summary>
    /// 配置中心查询异常
    /// </summary>
    public class ConfigurationModifyException : SoaServiceException
    {
        public ConfigurationModifyException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}

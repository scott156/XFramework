using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Resource.Center.Server.Contract.Query
{
    /// <summary>
    /// 查询Resource.基础信息
    /// </summary>
    public class AppInfoQueryRequestType : SoaRequestType
    {
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 实例Id, 可选
        /// </summary>
        public string InstanceId { get; set; }
    }
}

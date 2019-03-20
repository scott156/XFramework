using System.Collections.Generic;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Resource.Center.Server.Contract.Query
{
    public class AppInfoQueryResponseType : SoaResponseType
    {
        /// <summary>
        /// 应用程序Id
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 应用程序描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 应用程序所有者
        /// </summary>
        public List<string> Owners { get; set; }
        /// <summary>
        /// AppId下的实例清单
        /// </summary>
        public List<InstanceInfoType> Instances { get; set; }
    }

    public class InstanceInfoType
    {
        /// <summary>
        /// 实例Id
        /// </summary>
        public string InstanceId { get; set; }

        /// <summary>
        /// 资源Id, 资源所处的服务器
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Cpu数量
        /// </summary>
        public int Cpu { get; set; }

        /// <summary>
        /// 内存数量
        /// </summary>
        public long Memory { get; set; }

        /// <summary>
        /// 对外暴露的端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Docker容器Id
        /// </summary>
        public string ContainerId { get; set; }

        /// <summary>
        /// 服务器状态
        /// </summary>
        public int ServerStatus { get; set; }

    }
}

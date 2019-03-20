using System;
using System.Collections.Generic;
using System.Text;

namespace XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter.Bean
{
    /// <summary>
    /// 实例信息, 一个App（应用程序站点）可以分配多个实例
    /// </summary>
    public class InstanceInfo
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

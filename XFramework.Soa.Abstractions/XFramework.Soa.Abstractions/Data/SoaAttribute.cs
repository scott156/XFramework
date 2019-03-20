using System;

namespace XFramework.Soa.Abstractions.Data
{
    public sealed class SoaAttribute : Attribute {

        /// <summary>
        /// 服务ID
        /// </summary>
        /// <returns></returns>
        public string Service { get; set; }

        /// <summary>
        /// 操作ID
        /// </summary>
        /// <returns></returns>
        public string Operation { get; set; }

        public SoaAttribute(string service, string operation) {    
            this.Service = service;
            this.Operation = operation;
        }
    }
}
using System;

namespace XFramework.Soa.Abstractions.Data
{
    public sealed class SoaServiceEntity
    {
        /// <summary>
        /// 服务域
        /// </summary>
        /// <returns></returns>
        public string Service { get; set; }

        /// <summary>
        /// 接口名称
        /// </summary>
        /// <returns></returns>
        public string Operation { get; set; }

        public Type ServiceType { get; set; }

        public Type RequestType { get; set; }

        public Type ResponseType { get; set; }

        public override string ToString() =>  $"{Service}.{Operation}";

        public string Description { get; set; }
    }
}
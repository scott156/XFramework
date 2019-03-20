using System.Collections.Generic;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Soa.Center.Server.Contract.Query
{
    public class SoaQueryResponseType : SoaResponseType
    {
        public string ServiceId { get; set; }

        public string AppId { get; set; }

        public string Description { get; set; }

        public List<InstanceInfoType> RegisteredInstances { get; set; }
    }

    public class InstanceInfoType
    {
        public string InstanceId { get; set; }
    }
}

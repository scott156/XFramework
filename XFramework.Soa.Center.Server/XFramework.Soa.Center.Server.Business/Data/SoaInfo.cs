using System;
using System.Collections.Generic;
using System.Text;

namespace XFramework.Soa.Center.Server.Business.Data
{
    public class SoaInfo
    {
        public string ServiceId { get; set; }

        public string AppId { get; set; }

        public string Description { get; set; }

        public List<ResourceInfo> RegisteredResources { get; set; }
    }
}

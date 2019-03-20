using System;
using System.Collections.Generic;
using System.Text;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Soa.Center.Server.Contract.Apply
{
    public class SoaApplyRequestType : SoaRequestType
    {
        public string ServiceId { get; set; }

        public string AppId { get; set; }

        public string Description { get; set; }
    }
}

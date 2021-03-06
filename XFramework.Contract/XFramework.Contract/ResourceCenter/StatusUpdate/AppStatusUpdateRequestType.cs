﻿using System;
using System.Collections.Generic;
using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Contract.ResourceCenter.StatusUpdate
{
    public class AppStatusUpdateRequestType : SoaRequestType
    {
        public string InstanceId { get; set; }

        public string AppId { get; set; }

        public ServiceStatusType ServerStatus { get; set; }

        public Operator Operation { get; set; }
    }
}

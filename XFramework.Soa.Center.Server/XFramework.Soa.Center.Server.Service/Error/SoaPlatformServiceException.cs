using System;
using XFramework.Core.Abstractions.Error;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Soa.Center.Server.Service.Error
{
    public class SoaPlatformServiceException : SoaServiceException
    {
        public SoaPlatformServiceException(ErrorCode errorCode, Exception innerException) : base(errorCode, innerException)
        {
        }
    }
}

using XFramework.Core.Abstractions.Error;
using XFramework.Soa.Abstractions.Error;

namespace XFramework.Resource.Center.Server.Business.Error
{
    public class ApplicationResourceServiceException : SoaServiceException
    {
        public ApplicationResourceServiceException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}

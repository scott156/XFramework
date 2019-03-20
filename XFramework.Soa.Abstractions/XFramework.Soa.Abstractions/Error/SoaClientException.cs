using System;
using XFramework.Core.Abstractions.Error;

namespace XFramework.Soa.Abstractions.Error
{
    /// <summary>
    /// Soa请求校验异常
    /// </summary>
    public class SoaClientException : SoaServiceException
    {
        public SoaClientException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }

        public SoaClientException(int errorCode, string message) : base(errorCode, message)
        {
        }
    }
}

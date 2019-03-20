using System;
using XFramework.Core.Abstractions.Error;

namespace XFramework.Soa.Abstractions.Error
{
    /// <summary>
    /// Soa请求校验异常
    /// </summary>
    public class SoaServiceVerifyException : SoaServiceException
    {
        public SoaServiceVerifyException(int errorCode, string message) 
            : base(errorCode, message)
        {
        }

        public SoaServiceVerifyException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }

        public SoaServiceVerifyException(int errorCode, string message, Exception innerException) 
            : base(errorCode, message, innerException)
        {
        }
    }
}

using System;
using XFramework.Core.Abstractions.Error;

namespace XFramework.Soa.Abstractions.Error
{
    /// <summary>
    /// Soa处理产生的异常
    /// </summary>
    public class SoaServiceException : FrameworkException
    {
        public SoaServiceException(int errorCode, string message) 
            : base(errorCode, message)
        {
        }

        public SoaServiceException(ErrorCode errorCode, string message) 
            : base(errorCode, message)
        {
        }

        public SoaServiceException(ErrorCode errorCode, Exception innerException) 
            : base(errorCode, innerException)
        {
        }

        public SoaServiceException(int errorCode, string message, Exception innerException) 
            : base(errorCode, message, innerException)
        {
        }
    }
}

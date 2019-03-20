using System;

namespace XFramework.Core.Abstractions.Error
{
    /// <summary>
    /// 框架基础异常
    /// </summary>
    public class FrameworkException : Exception
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorCode { get; }

        public FrameworkException(ErrorCode errorCode, string message)
            : base(message)
        {
            this.ErrorCode = (int)errorCode;
        }

        public FrameworkException(ErrorCode errorCode, Exception innerException)
            : base(innerException.Message, innerException)
        {
            this.ErrorCode = (int)errorCode;
        }

        public FrameworkException(int errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public FrameworkException(int errorCode, string message, Exception innerException) 
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using XFramework.Core.Abstractions.Error;

namespace XFramework.Dal.Error
{
    public class DalException : FrameworkException
    {
        public DalException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}

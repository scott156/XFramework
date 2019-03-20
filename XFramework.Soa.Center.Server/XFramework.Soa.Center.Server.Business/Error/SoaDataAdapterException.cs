using XFramework.Core.Abstractions.Error;

namespace XFramework.Soa.Center.Server.Business.Error
{
    public class SoaDataAdapterException : FrameworkException
    {
        public SoaDataAdapterException(ErrorCode errorCode, string message) : base(errorCode, message)
        {
        }
    }
}

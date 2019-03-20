using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Contract.SoaCenter.Register
{
    /// <summary>
    /// Soa自注册
    /// </summary>
    public class SoaRegisterRequestType : SoaRequestType
    {
        public string ServiceId { get; set; }
        public string InstanceId { get; set; }
    }
}

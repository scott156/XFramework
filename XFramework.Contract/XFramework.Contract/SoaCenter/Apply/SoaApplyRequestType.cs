using XFramework.Soa.Abstractions.Contract;

namespace XFramework.Contract.SoaCenter.Apply
{
    public class SoaApplyRequestType : SoaRequestType
    {
        public string ServiceId { get; set; }

        public string AppId { get; set; }

        public string Description { get; set; }
    }
}

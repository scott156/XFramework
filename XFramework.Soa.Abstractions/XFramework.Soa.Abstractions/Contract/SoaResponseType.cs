using XFramework.Core.Abstractions.Error;

namespace XFramework.Soa.Abstractions.Contract
{
    public class SoaResponseType
    {
        public SoaResponseHeader Header { get; set; } = new SoaResponseHeader();
    }

    public class SoaResponseHeader
    {
        /// <summary>
        /// 错误代号
        /// </summary>
        /// <returns></returns>
        public int ResponseCode { get; set; } = (int)ErrorCode.Success;

        /// <summary>
        /// 备注信息
        /// </summary>
        /// <returns></returns>
        public string Remark { get; set; }
    }
}
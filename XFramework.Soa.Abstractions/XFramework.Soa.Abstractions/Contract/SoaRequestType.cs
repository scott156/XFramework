namespace XFramework.Soa.Abstractions.Contract
{
    /// <summary>
    /// 基础的Soa请求类型
    /// </summary>
    public class SoaRequestType
    {
        public SoaRequestHeaderType Header { get; set; } = new SoaRequestHeaderType();
    }

    public class SoaRequestHeaderType
    {
        public string AppId { get; set; }
        
        public string TransactionId { get; set; }

        public string Enviroment { get; set; }

        public string SubEnviroment { get; set; }
    }
}
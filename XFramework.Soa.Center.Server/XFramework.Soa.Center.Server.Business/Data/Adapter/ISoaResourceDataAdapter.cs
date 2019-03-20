namespace XFramework.Soa.Center.Server.Business.Data.Adapter
{
    public interface ISoaResourceDataAdapter
    {
        /// <summary>
        /// 根据ServiceId查找Soa注册信息
        /// </summary>
        /// <param name="serviceId"></param>
        /// <returns></returns>
        SoaInfo Query(string serviceId);

        /// <summary>
        /// 申请新的Soa服务
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="appId">AppId</param>
        /// <param name="description">服务描述</param>
        void Apply(string serviceId, string appId, string description);
        
        /// <summary>
        /// Soa服务自注册
        /// </summary>
        /// <param name="serviceId">服务Id</param>
        /// <param name="instanceId">实例Id</param>
        void Register(string serviceId, string instanceId);

        void StartListener();
    }
}

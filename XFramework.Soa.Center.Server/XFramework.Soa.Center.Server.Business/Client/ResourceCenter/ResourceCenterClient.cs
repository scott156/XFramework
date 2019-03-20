using XFramework.Configuration.Client;
using XFramework.Contract.ResourceCenter.Query;
using XFramework.Soa.Client;

namespace XFramework.Soa.Center.Server.Business.Client.ResourceCenter
{
    public class ResourceCenterClient : IResourceCenterClient
    {
        public ISoaClientProvider Client { get; set; }

        private readonly IDynamicConfigProvider configProvider = DynamicConfigFactory.GetInstance("global.properties");

        public AppInfoQueryResponseType AppInfoQuery(string appId)
        {
            var request = new AppInfoQueryRequestType()
            {
                AppId = appId
            };
            
            return Client.Call<AppInfoQueryRequestType, AppInfoQueryResponseType>
                (configProvider.GetStringProperty("ResourceCenter"), "resource", "appinfoquery", request).Result;
        }
    }
}

using XFramework.Contract.ResourceCenter.Query;

namespace XFramework.Soa.Center.Server.Business.Client.ResourceCenter
{
    public interface IResourceCenterClient
    {
        AppInfoQueryResponseType AppInfoQuery(string appId);
    }
}

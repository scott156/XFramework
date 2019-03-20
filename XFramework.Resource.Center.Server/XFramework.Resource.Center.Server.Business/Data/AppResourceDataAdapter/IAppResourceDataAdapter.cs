using System;
using System.Collections.Generic;
using XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter.Bean;

namespace XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter
{
    public interface IAppResourceDataAdapter
    {
        /// <summary>
        /// 根据AppId查找App的基本信息
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        AppInfo Query(string appId);

        /// <summary>
        /// 更新App实例的状态
        /// </summary>
        /// <param name="appId">AppId</param>
        /// <param name="instanceId">实例Id</param>
        /// <param name="status">大于0代表设置状态，小于0代表取消状态</param>
        int UpdateStatus(string appId, string instanceId, int status);
    }
}

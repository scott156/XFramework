using System.Collections.Generic;
using System.Xml.Serialization;

namespace XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter.Bean
{
    /// <summary>
    /// 应用程序资源信息
    /// </summary>
    public class AppResource
    {
        /// <summary>
        /// 所有的应用程序清单
        /// </summary>
        public List<AppInfo> Applications { get; set; }
    }
}

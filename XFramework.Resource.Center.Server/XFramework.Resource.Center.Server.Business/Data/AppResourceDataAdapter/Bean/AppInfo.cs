using System.Collections.Generic;

namespace XFramework.Resource.Center.Server.Business.Data.AppResourceDataAdapter.Bean
{
    public class AppInfo
    {
        public string AppId { get; set; }

        public string Description { get; set; }

        public List<string> Owners { get; set; }

        public List<InstanceInfo> AppInstances { get; set; }
    }
}

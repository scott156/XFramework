using System.Collections.Generic;
using XFramework.Configuration.Client.Data;

namespace XFramework.Configuration.Client
{
    public interface IDynamicConfigModifier
    {
        /// <summary>
        /// 配置信息修改
        /// </summary>
        /// <param name="file">配置文件名称</param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        void Update(string file, List<ModifiedPropertyInfo> propertyInfo);
    }
}

using System.Threading;
using XFramework.Configuration.Server.Service.Query.Data;

namespace XFramework.Configuration.Server.Service.Data.DataAdapter.DataAdapter
{
    /// <summary>
    /// 配置数据加载器
    /// </summary>
    public interface IConfigurationDataAdapter
    {
        /// <summary>
        /// 获取配置数据
        /// </summary>
        /// <returns></returns>
        ConfigurationContent GetContent(string fileName);

        /// <summary>
        /// 注册监听
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        ManualResetEventSlim Register(string fileName);

        /// <summary>
        /// 修改配置
        /// </summary>
        /// <param name="fileName"></param>
        void Modify(string fileName, ConfigurationContent content);
    }
}

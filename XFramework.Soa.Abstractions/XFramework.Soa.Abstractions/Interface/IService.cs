using System.Collections.Generic;

namespace XFramework.Soa.Abstractions.Interface
{
    /// <summary>
    /// 所有的Soa服务必须实现该接口
    /// </summary>
    public interface IService
    {
        string Description { get; }
    }
}

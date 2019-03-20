using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger.Formatter
{
    /// <summary>
    /// 将原类型转换为字典，用于保存在日志的Scope中
    /// </summary>
    public interface ILogScopeFormatter<T>
    {
        Dictionary<string, string> Format(T source);
    }
}
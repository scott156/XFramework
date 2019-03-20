using System;
using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger.Formatter
{
    /// <summary>
    /// 将原类型转换为字典，用于保存在日志的Scope中
    /// 从 IEnumerable<KeyValuePair<string, object>> 转换为 Dictionary<string, string>
    /// </summary>
    public abstract class AbstractScopeFormatter<T> : ILogScopeFormatter<T>, IScopeMatcher
    {
        public abstract Dictionary<string, string> Format(T source);

        public virtual bool IsMatch(Type target)
        {
            if (typeof(T) == target || typeof(T).IsAssignableFrom(target))
            {
                return true;
            }

            return false;
        }
    }
}
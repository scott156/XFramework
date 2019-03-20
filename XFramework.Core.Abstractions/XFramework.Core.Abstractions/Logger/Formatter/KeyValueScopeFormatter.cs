using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger.Formatter
{
    /// <summary>
    /// 将原类型转换为字典，用于保存在日志的Scope中
    /// 从 <KeyValuePair<string, string>> 转换为 Dictionary<string, string>
    /// </summary>
    public class KeyValueScopeFormatter<T> : AbstractScopeFormatter<KeyValuePair<string, T>>
    {
        public override Dictionary<string, string> Format(KeyValuePair<string, T> source)
        {
            if (string.IsNullOrWhiteSpace(source.Key))
            {
                return null;
            }

            string value = null;
            if (source.Value != null)
            {
                value = source.Value.ToString();
            }

            return new Dictionary<string, string>() { { source.Key.ToLower(), value } };
        }
    }
}
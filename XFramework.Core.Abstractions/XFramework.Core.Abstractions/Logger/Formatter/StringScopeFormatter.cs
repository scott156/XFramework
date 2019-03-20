using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger.Formatter
{
    /// <summary>
    /// 将原类型转换为字典，用于保存在日志的Scope中
    /// 从 string 转换为 Dictionary<string, string>
    /// </summary>
    public class StringScopeFormatter : AbstractScopeFormatter<string>
    {
        public override Dictionary<string, string> Format(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            return new Dictionary<string, string>() { { source.ToString().ToLower(), source.ToString() } };
        }
    }
}
using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger.Formatter
{
    /// <summary>
    /// 将原类型转换为字典，用于保存在日志的Scope中
    /// 从 IEnumerable<KeyValuePair<string, object>> 转换为 Dictionary<string, string>
    /// </summary>
    public class EnumerableScopeFormatter<T> : AbstractScopeFormatter<IEnumerable<KeyValuePair<string, T>>>
    {
        public override Dictionary<string, string> Format(IEnumerable<KeyValuePair<string, T>> source)
        {
            var dictionary = ConvertToDictionary(source);

            if (dictionary == null || dictionary.Count == 0) return null;

            return dictionary;
        }

        /// <summary>
        /// 转换为字典
        /// </summary>
        /// <param name="dic"></param>
        private static Dictionary<string, string> ConvertToDictionary(IEnumerable<KeyValuePair<string, T>> dic)
        {
            if (dic == null) 
            {
                return null;
            }
            
            var dictionary = new Dictionary<string, string>();

            foreach (var item in dic)
            {
                if (string.IsNullOrWhiteSpace(item.Key)) continue;
                // 加入到Scope里面的Key统一为小写
                if (dictionary.ContainsKey(item.Key.ToLower())) continue;

                string value = null;
                if (item.Value != null)
                {
                    value = item.Value.ToString();
                }

                dictionary.Add(item.Key.ToLower(), value);
            }

            return dictionary.Count == 0 ? null : dictionary;
        }
    }
}
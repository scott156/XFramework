using System;
using System.Collections.Generic;

namespace XFramework.Core.Abstractions.Logger.Formatter
{
    public interface IScopeMatcher
    {
        /// <summary>
        /// 判断该类型是否可以使用当前Formatter
        /// </summary>
        /// <param name="target"></param>
        /// <returns>可以用来匹配的类型</returns>
        bool IsMatch(Type target);
    }
}
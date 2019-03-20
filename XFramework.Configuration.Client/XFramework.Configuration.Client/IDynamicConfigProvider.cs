using System;
using System.Collections.Generic;

namespace XFramework.Configuration.Client
{
    public interface IDynamicConfigProvider
    {
        int? GetIntProperty(string key);

        bool? GetBooleanProperty(string key);

        long? GetLongProperty(string key);

        string GetStringProperty(string key);
        
        /// <summary>
        /// 注册监听事件，当内容发生变化后，执行事件
        /// </summary>
        /// <param name="action"></param>
        void RegisterListener(Action action);
    }
}

using XFramework.Core.Abstractions.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using XFramework.Core.Abstractions.Logger.Formatter;

namespace XFramework.Core.Abstractions.Logger
{
    /// <summary>
    /// 通过Scope链，实现日志的范围
    /// </summary>
    public class LogScopeProvider : Singleton<LogScopeProvider>
    {
        // 基于异步线程的线程本地变量，存储Scope链
        private AsyncLocal<Scope> currentScope = new AsyncLocal<Scope>();

        // 存放用于将TState转为Scope的Formatter
        private static List<IScopeMatcher> matchers;

        static LogScopeProvider()
        {
            matchers = new List<IScopeMatcher>
            {
                new StringScopeFormatter(),
                new KeyValueScopeFormatter<string>(),
                new KeyValueScopeFormatter<object>(),
                new EnumerableScopeFormatter<string>(),
                new EnumerableScopeFormatter<object>()
            };
        }

        /// <summary>
        /// 找到合适的格式化工具，将TState转换为Scope, 如果无法匹配，则返回空
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        private object FindFormatter<TState>()
        {
            foreach (var matcher in matchers)
            {
                // 为了避免每次都去轮训IsMatch方法，提高效率，将数据加入到字典中
                if (matcher.IsMatch(typeof(TState)))
                {
                    return matcher;
                }
            }

            return null;
        }

        /// <summary>
        /// 在最近一次的Using范围内, 增加日志标签
        /// </summary>
        /// /// <param name="tag">日志标签</param>
        /// <param name="value">标签内容</param>
        public void Add(string tag, string value)
        {
            if (currentScope.Value == null)
            {
                currentScope.Value = new Scope(this, new Dictionary<string, string>() { { tag.ToLower(), value } }, null);
            }
            else
            {
                currentScope.Value.Tags.Add(tag.ToLower(), value);
            }
        }

        private ConcurrentDictionary<Type, Func<object, Dictionary<string, string>>> formatters =
            new ConcurrentDictionary<Type, Func<object, Dictionary<string, string>>>();

        private Dictionary<string, string> GetScope<TState>(TState state)
        {
            // 判断缓存中是否存在
            if (formatters.TryGetValue(typeof(TState), out var target) == true)
            {
                if (target != null)
                {
                    return target.Invoke(state);
                }

                return null;
            }
            else
            {
                // 重新加载数据后，再次调用获取Scope
                LoadFormatter<TState>();
                return GetScope(state);
            }
        }

        /// <summary>
        /// 将Formatter加载到缓存
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        private void LoadFormatter<TState>()
        {
            // 如果缓存中不存在，遍历所有的格式化器，找出可用的格式化工具
            var formatter = FindFormatter<TState>();

            if (formatter == null)
            {
                formatters.TryAdd(typeof(TState), null);
                return;
            }

            // 不论是否找到formatter都加入到字典单重
            formatters.TryAdd(typeof(TState), ((s) =>
            {
                var method = formatter.GetType().GetMethod("Format");

                return (Dictionary<string, string>)method.Invoke(formatter, new object[] { s });
            }));
        }

        private Dictionary<string, string> BuildScope<TState>(TState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("日志Scope不能为空");
            }

            return GetScope(state);
        }

        public IDisposable SetScope<TState>(TState state)
        {
            // 组装State
            var tags = BuildScope(state);
            if (tags == null || tags.Count == 0)
            {
                return null;
            }

            // 第一个Value为空
            var parent = currentScope.Value;
            var newScope = new Scope(this, tags, parent);
            currentScope.Value = newScope;

            // 返回实现了IDisposable的Scope的目的是为了在Using结束的时候，及时释放Scope
            return newScope;
        }

        public Dictionary<string, string> Scopes
        {
            get
            {
                if (currentScope.Value == null)
                {
                    return null;
                }

                var scope = currentScope.Value;
                var dictionary = new Dictionary<string, string>();
                while (scope != null)
                {
                    if (scope.Tags != null && scope.Tags.Count >= 0)
                    {
                        foreach (var item in scope.Tags)
                        {
                            // 去掉重复，保留最近的Key
                            if (dictionary.ContainsKey(item.Key)) continue;
                            dictionary.Add(item.Key, item.Value);
                        }
                    }

                    scope = scope.Parent;
                }

                return dictionary;
            }
        }

        /// <summary>
        /// 存储BeginScope实现的范围
        /// </summary>
        private class Scope : IDisposable
        {
            private readonly LogScopeProvider provider;
            public Dictionary<string, string> Tags { get; }
            public Scope Parent { get; }

            internal Scope(LogScopeProvider provider, Dictionary<string, string> tags, Scope parent)
            {
                this.provider = provider;
                this.Tags = tags;
                this.Parent = parent;
            }

            private bool isDisposed;

            public void Dispose()
            {
                if (!isDisposed)
                {
                    provider.currentScope.Value = Parent;
                    isDisposed = true;
                }
            }
        }
    }
}

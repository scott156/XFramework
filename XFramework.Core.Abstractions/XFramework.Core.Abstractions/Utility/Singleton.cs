namespace XFramework.Core.Abstractions.Utility
{
    /// <summary>
    /// 单例实现，继承该类实现统一的GetInstance方法
    /// </summary>
    /// <typeparam name="T">需要单例的类型</typeparam>
    public class Singleton<T> where T : class, new()
    {
        private static readonly T instance = new T();

        /// <summary>
        /// 获取应用的实例
        /// </summary>
        /// <returns>应用的实例</returns>
        public static T GetInstance()
        {
            return instance;
        }
    }
}

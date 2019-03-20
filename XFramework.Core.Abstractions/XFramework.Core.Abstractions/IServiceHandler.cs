using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace XFramework.Core.Abstractions
{
    public interface IServiceHandler
    {
        /// <summary>
        /// 处理类的名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 路由地址 http://[Address]:[Port]/[Router]
        /// </summary>
        string Router { get; }

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<string> Process(HttpContext context);

        /// <summary>
        /// 根据配置初始化XService
        /// </summary>
        /// <param name="services"></param>
        void Init(IServiceCollection services);
    }
}
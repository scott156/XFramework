using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using XFramework.Configuration.Server.Service.Modify;
using XFramework.Configuration.Server.Service.Query;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Logger;
using XFramework.Core.ServiceHandler;

namespace XFramework.Configuration.Server
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _environment = env;

            loggerFactory.AddXLog();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Initialize services
            services.AddXServiceHandler(_configuration, _environment);

            // 推荐引入Autofac，也可以不使用Autofac
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(QueryService)))
                .PublicOnly()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance()
                .PropertiesAutowired();           

            builder.Populate(services);
            builder.RegisterType<QueryService>().AsSelf().SingleInstance().PropertiesAutowired();
            builder.RegisterType<ModifyService>().AsSelf().SingleInstance().PropertiesAutowired();

            return new AutofacServiceProvider(builder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // 显示服务器处理状态
            app.UseStatusCodePages();

            // 处理基于XFramework框架的请求
            app.UseXServiceHandler();
        }
    }
}

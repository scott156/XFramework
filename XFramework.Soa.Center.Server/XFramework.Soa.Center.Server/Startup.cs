using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using XFramework.Core.Abstractions.Configuration;
using XFramework.Core.Abstractions.Logger;
using XFramework.Core.ServiceHandler;
using XFramework.Soa.Center.Server.Business.Data.Adapter;

namespace XFramework.Soa.Center.Server
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

            // Using Autofac
            var builder = new ContainerBuilder();
            builder.Populate(services);
            RegisterAssembly(builder);
            
            return new AutofacServiceProvider(builder.Build());
        }

        private void RegisterAssembly(ContainerBuilder builder)
        {
            var assemblies = AppSetting.GetInstance().AutofacAssemblies;
            if (assemblies == null || assemblies.Count == 0) return;

            foreach (var assemblyName in assemblies)
            {
                builder.RegisterAssemblyTypes(Assembly.Load(assemblyName))
                    .Where(p => p.FullName.StartsWith("XFramework"))
                    .PublicOnly()
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .SingleInstance()
                    .PropertiesAutowired();
            }
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

            // 启动的时候注册监听器
            var adapter = app.ApplicationServices.GetService<ISoaResourceDataAdapter>();
            adapter.StartListener();
        }
    }
}

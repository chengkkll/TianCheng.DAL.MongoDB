using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// DAL 针对WebApi的Startup中ConfigureServices的处理
    /// </summary>
    static public class TianChengDALConfigureServices
    {
        static private bool IsInit = false;
        /// <summary>
        /// 增加业务的Service
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void TianChengMongoDBInit(this IServiceCollection services, IConfiguration configuration)
        {
            if (IsInit) return;

            // ServiceLoader 中存入，方便后续获取服务
            TianCheng.Model.ServiceLoader.Services = services;
            TianCheng.Model.ServiceLoader.Configuration = configuration;
            // 根据IServiceRegister 接口来注册能找到的所有服务
            services.AddBusinessServices();
            // 设置对象自动映射
            TianCheng.Model.AutoMapperExtension.InitializeMappers();

            // 注册配置信息
            services.AddOptions();
            // 注册数据库模块配置信息
            services.TianChengDALInit(configuration);

            IsInit = true;
            TianCheng.Model.CommonLog.Logger.Information("ConfigureServices - TianCheng.DAL init complete.");
        }
    }
}

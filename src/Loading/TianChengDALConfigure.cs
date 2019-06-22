using Microsoft.Extensions.Configuration;
using TianCheng.DAL;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// DAL 针对WebApi的Startup中Configure的处理
    /// </summary>
    static public class TianChengDALConfigure
    {
        /// <summary>
        /// Configure 的初始化操作
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configuration"></param>
        static public void TianChengMongoDBInit(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.TianChengDALInit(configuration);
        }
    }
}

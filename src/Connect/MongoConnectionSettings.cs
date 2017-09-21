using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 数据库连接配置信息
    /// </summary>
    public class MongoConnectionSettings
    {
        #region 属性
        /// <summary>
        /// 连接的库名
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 连接的服务器名
        /// </summary>
        public string ServerAddress { get; set; }
        #endregion

        /// <summary>
        /// 连接数据库的字符串
        /// </summary>
        public string ConnectionString
        {
            get
            {
                if(String.IsNullOrWhiteSpace(ServerAddress))
                {
                    ServerAddress = MongoConnectionSettings.Default.ServerAddress;
                    Database = MongoConnectionSettings.Default.Database;
                }
                if(String.IsNullOrWhiteSpace(Database))
                {
                    return ServerAddress;
                }
                return ServerAddress + "/" + Database;
            }
        }
        /// <summary>
        /// 默认的Mongo数据库链接
        /// </summary>
        static public MongoConnectionSettings Default
        {
            get
            {
                return new MongoConnectionSettings() { Database = "demo", ServerAddress = "mongodb://localhost" };
            }
        }
    }
}

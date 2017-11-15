using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL
{
    /// <summary>
    /// 数据库链接配置
    /// </summary>
    public class DBConnectionOptions
    {
        /// <summary>
        /// 配置名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 服务器链接地址
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// 数据库名
        /// </summary>
        public string Database { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        public string Password { get; set; }
    }

}

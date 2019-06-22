using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// Mongo数据加载
    /// </summary>
    public class MongoLoader : ILoadDB
    {
        /// <summary>
        /// 实现接口方便统一初始化数据库操作
        /// </summary>
        public void Init()
        {
            // 设置数据库连接信息
            MongoConnection.SetConnection();
            // 为MongoDB操作注册全局信息  注册UTC时间转换
            Register.Init();
        }
    }
}

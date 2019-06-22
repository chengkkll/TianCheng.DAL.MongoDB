using MongoDB.Bson.Serialization;
using System;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 为MongoDB操作注册全局信息
    /// </summary>
    public class Register
    {
        static private bool IsRegister = false;
        /// <summary>
        /// 注册UTC时间转换
        /// </summary>
        static public void Init()
        {
            if (IsRegister) return;
            try
            {
                MongoLog.Logger.Debug("注册MongoDB的UTC时间转换操作");
                BsonSerializer.RegisterSerializer(typeof(DateTime), new MongoDateTimeSerializer());
                IsRegister = true;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Error(ex, "注册MongoDB的UTC时间转换时出错");
            }
        }
    }
}

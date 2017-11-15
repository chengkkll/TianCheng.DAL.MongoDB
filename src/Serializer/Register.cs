using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL.MongoDB
{
    public class Register
    {
        static public void Init()
        {
            try
            {
                DBLog.Logger.LogTrace("注册MongoDB的UTC时间转换操作");
                BsonSerializer.RegisterSerializer(typeof(DateTime), new MongoDateTimeSerializer());
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogError(ex, "注册MongoDB的UTC时间转换时出错");
            }
        }
    }
}

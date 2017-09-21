using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianCheng.DAL.MongoDB
{
    //public class MongoRegister
    //{
    //    public static bool Initializationed { get; private set; }

    //    public MongoRegister()
    //    {
    //        if (!Initializationed)
    //        {
    //            BsonSerializer.RegisterSerializer(typeof(DateTime), new MongoDateTimeSerializer());
    //            //GetConnectionStringFromConfig();
    //            Initializationed = true;
    //        }
    //    }

    //    //private void GetConnectionStringFromConfig()
    //    //{
    //    //    MongoContext.ConnectionString = "mongodb://127.0.0.1";
    //    //    //MongoContext.ConnectionString = "mongodb://123.206.26.29";
    //    //    MongoContext.Database = "storeroom";
    //    //}
    //}
}

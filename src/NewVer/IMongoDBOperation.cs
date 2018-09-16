using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TianCheng.Model;

namespace TianCheng.DAL
{
    /// <summary>
    /// 数据库操作接口
    /// </summary>
    public interface IMongoDBOperation<T> : IDBOperation<T, ObjectId> where T: MongoIdModel
    {

    }
}

using MongoDB.Bson;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 数据库操作接口
    /// </summary>
    public interface IMongoDBOperation<T> : IDBOperation<T, ObjectId> where T : MongoIdModel
    {

    }
}

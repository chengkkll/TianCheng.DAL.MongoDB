using MongoDB.Driver;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// Mongo数据库的服务缓存操作
    /// </summary>
    public class MongoProvider
    {
        /// <summary>
        /// 持久对象类型名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mongo连接终端
        /// </summary>
        public IMongoClient Client { get; internal set; }
        /// <summary>
        /// Mongo数据库
        /// </summary>
        public IMongoDatabase Database { get; internal set; }
        /// <summary>
        /// Mongo数据集合操作
        /// </summary>
        public dynamic Collection { get; internal set; }
        /// <summary>
        /// 操作的数据库集合名称
        /// </summary>
        public string CollectionName { get; internal set; }
        /// <summary>
        /// 数据库连接信息
        /// </summary>
        public DBConnectionOptions Connection { get; internal set; }       
    }
}
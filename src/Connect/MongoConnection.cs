using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 数据对象的数据操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoConnection<T> : IDisposable where T : TianCheng.Model.MongoIdModel
    {
        #region 构造
        protected IMongoClient _mongoClient { get; private set; }
        protected IMongoDatabase _mongoDatabase { get; private set; }
        protected IMongoCollection<T> _mongoCollection { get; private set; }

        public MongoConnection(string connectionString, string datebaseName)
        {
            _mongoClient = new MongoClient(connectionString);

            _mongoDatabase = _mongoClient.GetDatabase(datebaseName);
            //_mongoCollection = _mongoDatabase.GetCollection<T>(typeof(T).Name);
            string collectionName = MongoCollectionDict.Collection[typeof(T).Name];
            _mongoCollection = _mongoDatabase.GetCollection<T>(collectionName);
        }

        public MongoConnection()
        {
            MongoConnectionSettings setting = MongoContext.Settings;
            _mongoClient = new MongoClient(setting.ConnectionString);

            _mongoDatabase = _mongoClient.GetDatabase(setting.Database);
            string collectionName = MongoCollectionDict.Collection[typeof(T).Name];
            _mongoCollection = _mongoDatabase.GetCollection<T>(collectionName);

            //var setting = new MongoClientSettings();
            //setting.MinConnectionPoolSize = 1;
            //setting.MaxConnectionPoolSize = 25;
            //setting.ConnectTimeout = new TimeSpan(0, 0, 10);
            //setting.SocketTimeout = new TimeSpan(0, 0, 10);
            //setting.WaitQueueTimeout = new TimeSpan(0, 0, 10);
            //setting.ServerSelectionTimeout = new TimeSpan(0, 0, 10);
            ////setting.Server = new MongoServerAddress(MongoContext.Server);

            //_mongoClient = new MongoClient(setting);
            //_mongoDatabase = _mongoClient.GetDatabase(MongoContext.Database);
            //_mongoCollection = _mongoDatabase.GetCollection<T>(typeof(T).Name);
        }

        /// <summary>
        /// 当前版本驱动没有关闭连接方法
        /// </summary>
        public void Dispose()
        {
        }
        #endregion

        #region 数据查询
        /// <summary>
        /// 查询整个集合
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Search()
        {
            return _mongoCollection.AsQueryable();
        }

        /// <summary>
        /// 按照id来查询
        /// </summary>
        /// <returns></returns>
        public T Search(string id)
        {
            ObjectId objId;
            ObjectId.TryParse(id, out objId);

            return _mongoCollection.Find(new BsonDocument("_id", objId)).FirstOrDefault();
        }

        /// <summary>
        /// 按照id来查询
        /// </summary>
        /// <returns></returns>
        public T Search(ObjectId id)
        {
            return _mongoCollection.Find(new BsonDocument("_id", id)).FirstOrDefault();
        }

        /// <summary>
        /// 按照实体来查询
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Search(T entity)
        {
            var query = MongoSerializer.SerializeQueryModel(entity);
            return _mongoCollection.Find(query).ToEnumerable();
        }
        #endregion

        #region 数据的插入
        /// <summary>
        /// 创建单条新数据
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(T entity)
        {
            _mongoCollection.InsertOne(entity);
        }

        /// <summary>
        /// 创建多条新数据
        /// </summary>
        /// <param name="entities"></param>
        public void Insert(IEnumerable<T> entities)
        {
            _mongoCollection.InsertMany(entities);
        }
        #endregion

        #region 数据更新
        /// <summary>
        /// 更新单条数据
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            _mongoCollection.ReplaceOne(new BsonDocument("_id", entity.Id), entity);
        }

        /// <summary>
        /// 更新多条数据
        /// </summary>
        /// <param name="entities"></param>
        public void Update(IEnumerable<T> entities)
        {
            foreach (var item in entities)
            {
                Update(item);
            }
        }
        #endregion

        #region 数据删除
        /// <summary>
        /// 逻辑删除单条数据
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(T entity)
        {
            var query = MongoSerializer.SerializeQueryModel(entity);
            _mongoCollection.DeleteMany(query);
        }

        /// <summary>
        /// 逻辑删除单条数据
        /// </summary>
        /// <param name="entities"></param>
        public void Delete(IEnumerable<T> entities)
        {
            foreach (var item in entities)
            {
                Delete(item);
            }
        }


        /// <summary>
        /// 按照id来删除
        /// </summary>
        /// <returns></returns>
        public T Delete(string id)
        {
            ObjectId objId;
            ObjectId.TryParse(id, out objId);

            return _mongoCollection.FindOneAndDelete(new BsonDocument("_id", objId));
        }
        #endregion

        #region 删除表内所有数据
        /// <summary>
        /// 删除表内所有数据
        /// </summary>
        public void Drop()
        {
            string collectionName = MongoCollectionDict.Collection[typeof(T).Name];
            _mongoDatabase.DropCollection(collectionName);
        }

        /// <summary>
        /// 异步删除表内所有数据
        /// </summary>
        /// <returns></returns>
        public System.Threading.Tasks.Task DropAsync()
        {
            string collectionName = MongoCollectionDict.Collection[typeof(T).Name];
            return _mongoDatabase.DropCollectionAsync(collectionName);
        }
        #endregion



        /// <summary>
        /// Aggregate统计查询
        /// </summary>
        public List<R> Aggregate<R>(PipelineDefinition<T, R> pipeline)
        {
            //var group = new BsonDocument();
            //group.AddRange(new BsonDocument("_id", "$SalerId"));
            //group.AddRange(new BsonDocument("total", new BsonDocument( "$sum",1)));

            //PipelineDefinition<T, R> pipeline = new BsonDocument[]
            //{
            //    new BsonDocument { { "$group", group } }
            //};

            var cursor = _mongoCollection.Aggregate(pipeline);

            var result = cursor.ToList();
            return result;
        }
    }

}

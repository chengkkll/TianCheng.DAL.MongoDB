using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;



namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 异步的数据库操作请求
    /// </summary>
    public class MongoOperationAsync<T> : IDisposable where T : TianCheng.Model.MongoIdModel
    {
        #region 构造
        protected IMongoClient _mongoClient { get; private set; }
        protected IMongoDatabase _mongoDatabase { get; private set; }
        protected IMongoCollection<T> _mongoCollection { get; private set; }

        public MongoOperationAsync()
        {
            MongoConnectionSettings setting = MongoContext.Settings;
            _mongoClient = new MongoClient(setting.ServerAddress);
            _mongoDatabase = _mongoClient.GetDatabase(setting.Database);
            string collectionName = MongoCollectionDict.Collection[typeof(T).Name];
            _mongoCollection = _mongoDatabase.GetCollection<T>(collectionName);
        }

        /// <summary>
        /// 当前版本驱动没有关闭连接方法
        /// </summary>
        public void Dispose()
        {
        }
        #endregion

        //#region 数据查询
        ///// <summary>
        ///// 查询整个集合
        ///// </summary>
        ///// <returns></returns>
        //public IQueryable<T> Search()
        //{
        //    return _mongoCollection.AsQueryable();
        //}

        ///// <summary>
        ///// 按照id来查询
        ///// </summary>
        ///// <returns></returns>
        //public T Search(string id)
        //{
        //    ObjectId objId;
        //    ObjectId.TryParse(id, out objId);

        //    return _mongoCollection.Find(new BsonDocument("_id", objId)).FirstOrDefault();
        //}

        ///// <summary>
        ///// 按照id来查询
        ///// </summary>
        ///// <returns></returns>
        //public T Search(ObjectId id)
        //{
        //    return _mongoCollection.Find(new BsonDocument("_id", id)).FirstOrDefault();
        //}

        ///// <summary>
        ///// 按照实体来查询
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerable<T> Search(T entity)
        //{
        //    var query = MongoSerializer.SerializeQueryModel(entity);
        //    return _mongoCollection.Find(query).ToEnumerable();
        //}
        //#endregion

        //#region 数据的插入
        ///// <summary>
        ///// 创建单条新数据
        ///// </summary>
        ///// <param name="entity"></param>
        //public void Insert(T entity)
        //{
        //    _mongoCollection.InsertOne(entity);
        //}

        ///// <summary>
        ///// 创建多条新数据
        ///// </summary>
        ///// <param name="entities"></param>
        //public void Insert(IEnumerable<T> entities)
        //{
        //    _mongoCollection.InsertMany(entities);
        //}
        //#endregion

        #region 数据更新
        /// <summary>
        /// 更新单条数据 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Update(T entity)
        {
            var result = UpdateAsync(entity);
            result.Wait();
            return result.Result;
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

        /// <summary>
        /// 更新单条数据  异步
        /// </summary>
        /// <param name="entity"></param>
        public async Task<bool> UpdateAsync(T entity)
        {
            ReplaceOneResult result = await _mongoCollection.ReplaceOneAsync(new BsonDocument("_id", entity.Id), entity);
            if(result.ModifiedCount == 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 更新多条数据
        /// </summary>
        /// <param name="entities"></param>
        public async Task<bool> UpdateAsync(IEnumerable<T> entities)
        {
            bool flag = true;
            foreach (var item in entities)
            {
                if(!await UpdateAsync(item))
                {
                    flag = false;
                }
            }
            return flag;
        }
        #endregion
    }
}

using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianCheng.Model;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Logging;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// MongoDB 数据库操作处理
    /// </summary>
    public class MongoOperation<T> : IMongoDBOperation<T>, IDisposable where T : TianCheng.Model.MongoIdModel
    {
        #region 构造
        /// <summary>
        /// 链接mongo的客户端
        /// </summary>
        protected IMongoClient _mongoClient { get; private set; }
        /// <summary>
        /// mongo数据库
        /// </summary>
        protected IMongoDatabase _mongoDatabase { get; private set; }
        /// <summary>
        /// 表（集合）
        /// </summary>
        protected IMongoCollection<T> _mongoCollection { get; private set; }

        private DBMappingAttribute options = null;
        /// <summary>
        /// 数据库配置信息
        /// </summary>
        protected DBMappingAttribute _options
        {
            get
            {
                if (options == null)
                {
                    options = GetOptions();
                }
                return options;
            }
        }

        #region 获取数据库链接配置
        private DBMappingAttribute GetOptions()
        {
            // 从缓存中获取数据库配置信息
            ConnectionCaching caching = new ConnectionCaching();
            var dict = caching.GetCache();
            string typeName = this.GetType().FullName;
            if (!dict.ContainsKey(typeName))
            {
                DBLog.Logger.LogError($"{typeName}需要设置数据库链接信息。\r\n请在{typeName}中增加特性[DBMapping]\r\n格式：[DBMapping(集合名称,数据库链接名称)]\r\n示例：[DBMapping(\"demo_collection\", \"debug\")]");
                throw ApiException.BadRequest(typeName + "需要设置数据库链接信息");
            }
            return dict[typeName];
        }
        #endregion

        /// <summary>
        /// 构造方法处理
        /// </summary>
        public MongoOperation()
        {
            try
            {
                // 获取数据链接客户端
                _mongoClient = new MongoClient(_options.ConnectionOptions.ConnectionString());
                // 获取数据库
                _mongoDatabase = _mongoClient.GetDatabase(_options.ConnectionOptions.Database);
                // 获取一个表（集合）操作
                _mongoCollection = _mongoDatabase.GetCollection<T>(_options.CollectionName);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("DBMapping"))
                {
                    throw;
                }
                DBLog.Logger.LogError(ex, "连接数据库错误");
                throw new Exception("连接数据库错误");
            }

        }

        /// <summary>
        /// 当前版本驱动不需要关闭处理
        /// </summary>
        public void Dispose()
        {

        }
        #endregion

        #region 数据查询

        /// <summary>
        /// 根据id来查询
        /// </summary>
        /// <returns></returns>
        public T SearchById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                // 记录错误信息
                DBLog.Logger.LogError($"按ID查询时参数错误，无法转换成ObjectId的Id值为：[{id}]查询对象类型为：[{typeof(T).FullName}]");
                throw new ArgumentException("查询参数无效");
            }
            return SearchByTypeId(objId);
        }

        /// <summary>
        /// 根据id来查询
        /// </summary>
        /// <returns></returns>
        public T SearchByTypeId(ObjectId id)
        {
            try
            {
                var result = _mongoCollection.FindAsync(new BsonDocument("_id", id)).Result;
                List<T> objList = result.ToList();
                if (objList.Count == 0)
                {
                    DBLog.Logger.LogWarning($"按ObjectId查询时,无法找到对象。ObjectId：[{id}]  查询对象类型：[{typeof(T).FullName}]");
                }
                return objList.FirstOrDefault();
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw ApiException.BadRequest("连接数据库超时，请稍后再试");
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is TimeoutException)
                    {
                        DBLog.Logger.LogWarning(x, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                        throw ApiException.BadRequest("连接数据库超时，请稍后再试");
                    }
                    return false;
                });
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        /// <summary>
        /// 根据ID列表获取对象集合
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<T> SearchByIds(IEnumerable<string> ids)
        {
            try
            {
                IEnumerable<string> oids = ids.Select(e => $"ObjectId('{e}')");
                var query = new BsonDocument("_id", BsonSerializer.Deserialize<BsonDocument>("{'$in':[" + String.Join(',', oids) + "]}"));
                var result = _mongoCollection.FindAsync(query).Result;
                List<T> objList = result.ToList();
                if (objList.Count == 0)
                {
                    DBLog.Logger.LogWarning($"按ObjectId查询时,无法找到对象。ObjectId：[{ids}]  查询对象类型：[{typeof(T).FullName}]");
                }
                return objList;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 获取当前集合的查询链式接口
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Queryable()
        {
            return _mongoCollection.AsQueryable();
        }

        /// <summary>
        /// 根据Mongodb的查询条件查询
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public List<T> Search(FilterDefinition<T> filter, SortDefinition<T> sort)
        {
            //FieldDefinition<T> field = "Name";

            //var s = Builders<T>.Sort.Ascending(field);
            try
            {
                _mongoCollection.Find(filter).Sort(sort).ToListAsync();

                return _mongoCollection.FindAsync(filter).Result.ToList();
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 根据Mongodb的查询条件查询 ( 分页 )
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="queryInfo"></param>
        /// <returns></returns>
        public List<T> Search(FilterDefinition<T> filter, QueryInfo queryInfo)
        {
            SortDefinition<T> sort = "";
            FieldDefinition<T> field = queryInfo.Sort.Property;
            sort = Builders<T>.Sort.Combine(sort, queryInfo.Sort.IsAsc ? Builders<T>.Sort.Ascending(field) : Builders<T>.Sort.Descending(field));

            try
            {
                return _mongoCollection.Find(filter).Sort(sort).ToListAsync().Result;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 根据实体来查询
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Search(T entity)
        {
            try
            {
                var query = MongoSerializer.SerializeQueryModel(entity);
                return _mongoCollection.FindAsync(query).Result.ToEnumerable();
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        #endregion

        #region Save
        /// <summary>
        /// 保存对象，根据ID是否为空来判断是新增还是修改操作
        /// </summary>
        /// <param name="entity"></param>
        public void Save(T entity)
        {
            if (entity.IsEmpty)
            {
                InsertObject(entity);
            }
            else
            {
                UpdateObject(entity);
            }
        }
        #endregion

        #region 数据的插入
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <param name="entity"></param>
        public void InsertObject(T entity)
        {
            DBLog.Logger.LogTrace($"插入数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                _mongoCollection.InsertOne(entity);
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <param name="entities"></param>
        public void InsertRange(IEnumerable<T> entities)
        {
            DBLog.Logger.LogTrace($"插入多条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entities.ToJson()}] ");
            try
            {
                _mongoCollection.InsertMany(entities);
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        #region 异步插入操作
        /// <summary>
        /// 插入单条数据  异步   （据说异步写入数据比同步的慢，有待测试）
        /// </summary>
        /// <param name="entity"></param>
        public async void InsertAsync(T entity)
        {
            DBLog.Logger.LogTrace($"异步插入数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                await _mongoCollection.InsertOneAsync(entity);
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 插入多条数据 异步   （据说异步写入数据比同步的慢，有待测试）
        /// </summary>
        /// <param name="entities"></param>
        public async void InsertAsync(IEnumerable<T> entities)
        {
            DBLog.Logger.LogTrace($"异步插入多条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entities.ToJson()}] ");
            try
            {
                await _mongoCollection.InsertManyAsync(entities);
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        #endregion

        #endregion

        #region 数据更新
        /// <summary>
        /// 更新单条数据 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool UpdateObject(T entity)
        {
            DBLog.Logger.LogTrace($"更新单条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                ReplaceOneResult result = _mongoCollection.ReplaceOne(o => o.Id == entity.Id, entity);
                if (result.ModifiedCount == 1)
                {
                    return true;
                }
                if (result.ModifiedCount == 0 && result.MatchedCount == 1 && result.IsModifiedCountAvailable && result.IsAcknowledged)
                {
                    DBLog.Logger.LogDebug($"更新单条数据时，新数据与元数据相同，数据没有发生改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                    return true;
                }
                DBLog.Logger.LogError($"更新单条数据时，无数据改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                return false;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        /// <summary>
        /// 更新多条数据
        /// </summary>
        /// <param name="entities"></param>
        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var item in entities)
            {
                UpdateObject(item);
            }
        }

        #region 异步更新操作
        /// <summary>
        /// 更新单条数据  异步   （据说异步写入数据比同步的慢，有待测试）
        /// </summary>
        /// <param name="entity"></param>
        public async Task<bool> UpdateAsync(T entity)
        {
            DBLog.Logger.LogTrace($"异步更新单条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                ReplaceOneResult result = await _mongoCollection.ReplaceOneAsync(new BsonDocument("_id", entity.Id), entity);
                if (result.ModifiedCount == 1)
                {
                    return true;
                }
                if (result.ModifiedCount == 0 && result.MatchedCount == 1 && result.IsModifiedCountAvailable && result.IsAcknowledged)
                {
                    DBLog.Logger.LogDebug($"异步更新单条数据时，新数据与元数据相同，数据没有发生改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                    return true;
                }
                DBLog.Logger.LogError($"异步更新单条数据时，无数据改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                return false;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 更新多条数据 异步   （据说异步写入数据比同步的慢，有待测试）
        /// </summary>
        /// <param name="entities"></param>
        public async Task<bool> UpdateAsync(IEnumerable<T> entities)
        {
            bool flag = true;
            foreach (var item in entities)
            {
                if (!await UpdateAsync(item))
                {
                    flag = false;
                }
            }
            return flag;
        }
        #endregion

        #region 更新属性值
        /// <summary>
        /// 根据ID更新一个属性
        /// </summary>
        /// <param name="id"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public bool UpdatePropertyById(string id, string propertyName, object propertyValue)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                // 做记录，更新的ID不存在
                DBLog.Logger.LogError($"按ID更新单属性时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
            }
            return UpdatePropertyById(objId, propertyName, propertyValue);
        }
        /// <summary>
        /// 根据ID更新一个属性
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public bool UpdatePropertyById(ObjectId objId, string propertyName, object propertyValue)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", objId);
            UpdateDefinition<T> ud = Builders<T>.Update.Set(propertyName, propertyValue);
            try
            {
                UpdateResult result = _mongoCollection.UpdateOneAsync(filter, ud).Result;
                if (result.ModifiedCount == 0 && result.MatchedCount == 1)
                {
                    DBLog.Logger.LogWarning($"按ID更新单属性操作取消，更新的属性与原属性相同。\r\nId值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性为：[{propertyName}]\r\n更新后的值应为：[{propertyValue}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    DBLog.Logger.LogError($"按ID更新单属性操作取消，无数据被更新。\r\nId值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性为：[{propertyName}]\r\n更新后的值应为：[{propertyValue}]\r\n操作结果为：[{result.ToJson()}]");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 按ID更新多个属性
        /// </summary>
        /// <param name="id"></param>
        /// <param name="upProperty"></param>
        /// <returns></returns>
        public bool UpdatePropertyById(string id, UpdateDefinition<T> upProperty)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                // 做记录，更新的ID不存在
                DBLog.Logger.LogError($"按ID更新多属性时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
            }
            return UpdatePropertyById(objId, upProperty);
        }
        /// <summary>
        /// 按ID更新多个属性
        /// </summary>
        /// <param name="objId"></param>
        /// <param name="upProperty"></param>
        /// <returns></returns>
        public bool UpdatePropertyById(ObjectId objId, UpdateDefinition<T> upProperty)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", objId);
            try
            {
                UpdateResult result = _mongoCollection.UpdateOneAsync(filter, upProperty).Result;
                if (result.ModifiedCount == 0 && result.MatchedCount == 1)
                {
                    DBLog.Logger.LogWarning($"按ID更新多属性操作取消，更新的属性与原属性相同。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性信息为：[{upProperty.ToJson()}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    DBLog.Logger.LogError($"按ID更新多属性操作取消，无数据被更新。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性信息为：[{upProperty.ToJson()}]\r\n操作结果为：[{result.ToJson()}]");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        #endregion
        #endregion

        #region 物理删除
        /// <summary>
        ///  将对象内容作为查询条件来物理删除数据 
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveSearch(T entity)
        {
            DBLog.Logger.LogTrace($"物理删除数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                var query = MongoSerializer.SerializeQueryModel(entity);
                DeleteResult result = _mongoCollection.DeleteManyAsync(query).Result;
                if (result.DeletedCount == 0)
                {
                    DBLog.Logger.LogDebug($"将对象内容作为查询条件来物理删除数据 ==> 无数据被删除。\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{entity.ToJson()}] ");
                    return;
                }
                DBLog.Logger.LogDebug($"将对象内容作为查询条件来物理删除数据 ==> 已删除数据：{result.DeletedCount}条\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{entity.ToJson()}] ");
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        ///  物理删除对象 
        /// </summary>
        /// <param name="entity"></param>
        public T RemoveObject(T entity)
        {
            return RemoveByTypeId(entity.Id);
        }
        /// <summary>
        /// 根据ID列表 物理删除一组数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool RemoveByIdList(IEnumerable<string> ids)
        {
            List<ObjectId> objIdList = new List<ObjectId>();
            foreach (string id in ids)
            {
                if (!ObjectId.TryParse(id, out ObjectId objId))
                {
                    // 做记录，物理删除的ID不存在
                    DBLog.Logger.LogError($"按ID物理删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\nID列表为：[{ids.ToJson()}]\r\n类型为：[{typeof(T).FullName}]");
                    continue;
                }
                objIdList.Add(objId);
            }

            return RemoveByTypeIdList(objIdList);
        }
        /// <summary>
        /// 根据ID列表 物理删除一组数据
        /// </summary>
        /// <returns></returns>
        public bool RemoveByTypeIdList(IEnumerable<ObjectId> ids)
        {
            var filter = Builders<T>.Filter.AnyIn("_id", ids);
            try
            {
                DeleteResult result = _mongoCollection.DeleteManyAsync(filter).Result;
                if (result.DeletedCount != ids.Count())
                {
                    // 没有完全删除，做日志记录
                    DBLog.Logger.LogWarning($"根据ID列表 物理删除一组数据 ==> 已删除数据：{result.DeletedCount}条,按条件应删除{ids.Count()}条\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{ids.ToJson()}] ");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        /// <summary>
        /// 根据ID 物理删除数据
        /// </summary>
        /// <param name="id">删除的ID</param>
        /// <returns>返回已删除的对象信息</returns>
        public T RemoveById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                DBLog.Logger.LogError($"按ID物理删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
                ApiException.ThrowBadRequest("物理删除的ID值错误");
            }

            return RemoveByTypeId(objId);
        }

        /// <summary>
        /// 根据ID 物理删除数据
        /// </summary>
        /// <param name="id">删除的ID</param>
        /// <returns>返回已删除的对象信息</returns>
        public T RemoveByTypeId(ObjectId id)
        {
            try
            {
                FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", id);
                T result = _mongoCollection.FindOneAndDeleteAsync(filter).Result;
                return result;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogError(ex, $"按ID物理删除时错误。Id值为：[{id.ToString()}]\r\n类型为：[{typeof(T).FullName}]");
                throw;
            }
        }
        #endregion

        #region 逻辑删除数据
        /// <summary>
        /// 根据ID 逻辑删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                // 做记录，删除的ID不存在
                DBLog.Logger.LogError($"按ID逻辑删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
            }
            return DeleteByTypeId(objId);
        }
        /// <summary>
        /// 根据ID 逻辑删除数据
        /// </summary>
        /// <returns></returns>
        public bool DeleteByTypeId(ObjectId objId)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", objId);
            UpdateDefinition<T> ud = Builders<T>.Update.Set("IsDelete", true);

            try
            {
                UpdateResult result = _mongoCollection.UpdateOneAsync(filter, ud).Result;
                if (result.ModifiedCount == 0 && result.MatchedCount == 1)
                {
                    DBLog.Logger.LogWarning($"按ID逻辑删除操作取消，数据已被逻辑删除，无需再次逻辑删除。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    DBLog.Logger.LogError($"按ID逻辑删除时失败，无数据被更新。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        /// <summary>
        /// 根据ID列表 逻辑删除一组数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByIdList(IEnumerable<string> ids)
        {
            List<ObjectId> objIdList = new List<ObjectId>();
            foreach (string id in ids)
            {
                if (!ObjectId.TryParse(id, out ObjectId objId))
                {
                    // 做记录，删除的ID不存在
                    DBLog.Logger.LogError($"按ID逻辑删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\nID列表为：[{ids.ToJson()}]\r\n类型为：[{typeof(T).FullName}]");
                    continue;
                }
                objIdList.Add(objId);
            }

            return DeleteByTypeIdList(objIdList);
        }
        /// <summary>
        /// 根据ID列表 逻辑删除一组数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool DeleteByTypeIdList(IEnumerable<ObjectId> ids)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.AnyIn("_id", ids);
            UpdateDefinition<T> ud = Builders<T>.Update.Set("IsDelete", true);

            try
            {
                UpdateResult result = _mongoCollection.UpdateManyAsync(filter, ud).Result;
                if (result.ModifiedCount != ids.Count() && result.MatchedCount == ids.Count())
                {
                    DBLog.Logger.LogWarning($"按ID逻辑删除操作完成，其中部分数据在操作前已被逻辑删除。Id列表为：[{ids.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (ids.Count() != result.ModifiedCount)
                {
                    // 不完全更新，记录日志
                    DBLog.Logger.LogWarning($"根据ID列表 逻辑删除一组数据 ==> 已删除数据：{result.MatchedCount}条,按条件应删除{ids.Count()}条\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{ids.ToJson()}] ");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        #endregion

        #region 取消逻辑删除数据
        /// <summary>
        /// 根据ID 还原被逻辑删除的数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool UndeleteById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                // 记录日志：逻辑删除的ID错误
                DBLog.Logger.LogError($"按ID取消逻辑删除时参数错误，无法转换成ObjectId。\r\nId值为：[{id}]\t类型为：[{typeof(T).FullName}]");
            }
            return UndeleteByTypeId(objId);
        }
        /// <summary>
        /// 根据ID 还原被逻辑删除的数据
        /// </summary>
        /// <returns></returns>
        public bool UndeleteByTypeId(ObjectId objId)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("_id", objId);
            UpdateDefinition<T> ud = Builders<T>.Update.Set("IsDelete", false);
            try
            {
                UpdateResult result = _mongoCollection.UpdateOneAsync(filter, ud).Result;
                if (result.ModifiedCount != 1 && result.MatchedCount == 1)
                {
                    DBLog.Logger.LogWarning($"按ID逻辑删除操作完成，数据在操作前已被逻辑删除。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    DBLog.Logger.LogError($"按ID取消逻辑删除时失败，无数据被更新。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        /// <summary>
        /// 根据ID列表 还原被逻辑删除的一组数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool UndeleteByIdList(IEnumerable<string> ids)
        {
            List<ObjectId> objIdList = new List<ObjectId>();
            foreach (string id in ids)
            {
                if (!ObjectId.TryParse(id, out ObjectId objId))
                {
                    // 做记录，删除的ID不存在
                    DBLog.Logger.LogError($"按ID取消逻辑删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\nID列表为：[{ids.ToJson()}]\r\n类型为：[{typeof(T).FullName}]");
                    continue;
                }
                objIdList.Add(objId);
            }

            return UndeleteByTypeIdList(objIdList);
        }
        /// <summary>
        /// 根据ID列表 还原被逻辑删除的一组数据
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public bool UndeleteByTypeIdList(IEnumerable<ObjectId> ids)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.AnyIn("_id", ids);          // 设置还原的过滤条件
            UpdateDefinition<T> ud = Builders<T>.Update.Set("IsDelete", false);         // 设置还原逻辑删除

            try
            {
                UpdateResult result = _mongoCollection.UpdateManyAsync(filter, ud).Result;  // 执行还原逻辑删除操作
                if (result.ModifiedCount != ids.Count() && result.MatchedCount == ids.Count())
                {
                    DBLog.Logger.LogWarning($"按ID还原删除操作完成，其中部分数据在操作前是非删除状态。Id列表为：[{ids.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (ids.Count() != result.ModifiedCount)
                {
                    // 还原不完全，记录日志
                    DBLog.Logger.LogWarning($"根据ID列表 取消逻辑删除一组数据 ==> 已删除数据：{result.MatchedCount}条,按条件应删除{ids.Count()}条。\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{ids.ToJson()}] ");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        #endregion

        #region 删除表（集合）
        /// <summary>
        /// 删除表（集合）
        /// </summary>
        public void Drop()
        {
            DBLog.Logger.LogWarning($"删除集合 准备删除==> 集合名称：[{_options.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
            try
            {
                _mongoDatabase.DropCollection(_options.CollectionName);
                DBLog.Logger.LogWarning($"删除集合 已删除  ==> 集合名称：[{_options.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 删除表（集合） 异步
        /// </summary>
        /// <returns></returns>
        public async void DropAsync()
        {
            DBLog.Logger.LogWarning($"异步删除集合 准备删除==> 集合名称：[{_options.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
            try
            {
                await _mongoDatabase.DropCollectionAsync(_options.CollectionName);
                DBLog.Logger.LogWarning($"异步删除集合 已删除  ==> 集合名称：[{_options.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
            }
            catch (System.TimeoutException te)
            {
                DBLog.Logger.LogWarning(te, "数据库链接超时。链接字符串：" + _options.ConnectionOptions.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                DBLog.Logger.LogWarning(ex, "操作异常终止。");
                throw;
            }
        }
        #endregion

        #region 数据统计
        /// <summary>
        /// 通过Aggregate统计查询
        /// </summary>
        public List<R> Aggregate<R>(PipelineDefinition<T, R> pipeline)
        {
            var cursor = _mongoCollection.AggregateAsync(pipeline).Result;
            var result = cursor.ToList();
            return result;
        }
        #endregion
    }
}

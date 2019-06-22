using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// MongoDB 持久化操作
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class MongoOperation<T>
    {
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

        #region 数据插入
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <param name="entity"></param>
        public void InsertObject(T entity)
        {
            MongoLog.Logger.Debug($"插入数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                MongoCollection.InsertOne(entity);
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 插入多条新数据
        /// </summary>
        /// <param name="entities"></param>
        public void InsertRange(IEnumerable<T> entities)
        {
            MongoLog.Logger.Debug($"插入多条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entities.ToJson()}] ");
            try
            {
                MongoCollection.InsertMany(entities);
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
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
            MongoLog.Logger.Debug($"异步插入数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                await MongoCollection.InsertOneAsync(entity);
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
                throw;
            }
        }

        /// <summary>
        /// 插入多条数据 异步   （据说异步写入数据比同步的慢，有待测试）
        /// </summary>
        /// <param name="entities"></param>
        public async void InsertAsync(IEnumerable<T> entities)
        {
            MongoLog.Logger.Debug($"异步插入多条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entities.ToJson()}] ");
            try
            {
                await MongoCollection.InsertManyAsync(entities);
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
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
            MongoLog.Logger.Debug($"更新单条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                ReplaceOneResult result = MongoCollection.ReplaceOne(o => o.Id == entity.Id, entity);
                if (result.ModifiedCount == 1)
                {
                    return true;
                }
                if (result.ModifiedCount == 0 && result.MatchedCount == 1 && result.IsModifiedCountAvailable && result.IsAcknowledged)
                {
                    MongoLog.Logger.Debug($"更新单条数据时，新数据与元数据相同，数据没有发生改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                    return true;
                }
                MongoLog.Logger.Error($"更新单条数据时，无数据改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                return false;
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
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
            MongoLog.Logger.Debug($"异步更新单条数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                ReplaceOneResult result = await MongoCollection.ReplaceOneAsync(new BsonDocument("_id", entity.Id), entity);
                if (result.ModifiedCount == 1)
                {
                    return true;
                }
                if (result.ModifiedCount == 0 && result.MatchedCount == 1 && result.IsModifiedCountAvailable && result.IsAcknowledged)
                {
                    MongoLog.Logger.Debug($"异步更新单条数据时，新数据与元数据相同，数据没有发生改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                    return true;
                }
                MongoLog.Logger.Error($"异步更新单条数据时，无数据改变。 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] \r\n更新结果为：[{result.ToJson()}]");
                return false;
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
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
                MongoLog.Logger.Error($"按ID更新单属性时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
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
                UpdateResult result = MongoCollection.UpdateOneAsync(filter, ud).Result;
                if (result.ModifiedCount == 0 && result.MatchedCount == 1)
                {
                    MongoLog.Logger.Warning($"按ID更新单属性操作取消，更新的属性与原属性相同。\r\nId值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性为：[{propertyName}]\r\n更新后的值应为：[{propertyValue}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    MongoLog.Logger.Error($"按ID更新单属性操作取消，无数据被更新。\r\nId值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性为：[{propertyName}]\r\n更新后的值应为：[{propertyValue}]\r\n操作结果为：[{result.ToJson()}]");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
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
                MongoLog.Logger.Error($"按ID更新多属性时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
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
                UpdateResult result = MongoCollection.UpdateOneAsync(filter, upProperty).Result;
                if (result.ModifiedCount == 0 && result.MatchedCount == 1)
                {
                    MongoLog.Logger.Warning($"按ID更新多属性操作取消，更新的属性与原属性相同。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性信息为：[{upProperty.ToJson()}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    MongoLog.Logger.Error($"按ID更新多属性操作取消，无数据被更新。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n更新属性信息为：[{upProperty.ToJson()}]\r\n操作结果为：[{result.ToJson()}]");
                    return false;
                }
                return true;
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
                throw;
            }
        }
        #endregion
        #endregion
    }
}

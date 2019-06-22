using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// MongoDB 数据库操作处理
    /// </summary>
    public partial class MongoOperation<T> : IMongoDBOperation<T> where T : TianCheng.Model.MongoIdModel
    {
        #region 物理删除
        /// <summary>
        ///  将对象内容作为查询条件来物理删除数据 
        /// </summary>
        /// <param name="entity"></param>
        public void RemoveSearch(T entity)
        {
            MongoLog.Logger.Debug($"物理删除数据 ==> 类型：[{typeof(T).FullName}]\r\n数据信息为：[{entity.ToJson()}] ");
            try
            {
                var query = MongoSerializer.SerializeQueryModel(entity);
                DeleteResult result = MongoCollection.DeleteManyAsync(query).Result;
                if (result.DeletedCount == 0)
                {
                    MongoLog.Logger.Debug($"将对象内容作为查询条件来物理删除数据 ==> 无数据被删除。\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{entity.ToJson()}] ");
                    return;
                }
                MongoLog.Logger.Debug($"将对象内容作为查询条件来物理删除数据 ==> 已删除数据：{result.DeletedCount}条\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{entity.ToJson()}] ");
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
                    MongoLog.Logger.Error($"按ID物理删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\nID列表为：[{ids.ToJson()}]\r\n类型为：[{typeof(T).FullName}]");
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
                DeleteResult result = MongoCollection.DeleteManyAsync(filter).Result;
                if (result.DeletedCount != ids.Count())
                {
                    // 没有完全删除，做日志记录
                    MongoLog.Logger.Warning($"根据ID列表 物理删除一组数据 ==> 已删除数据：{result.DeletedCount}条,按条件应删除{ids.Count()}条\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{ids.ToJson()}] ");
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
        /// 根据ID 物理删除数据
        /// </summary>
        /// <param name="id">删除的ID</param>
        /// <returns>返回已删除的对象信息</returns>
        public T RemoveById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                MongoLog.Logger.Error($"按ID物理删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
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
                T result = MongoCollection.FindOneAndDeleteAsync(filter).Result;
                return result;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Error(ex, $"按ID物理删除时错误。Id值为：[{id.ToString()}]\r\n类型为：[{typeof(T).FullName}]");
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
                MongoLog.Logger.Error($"按ID逻辑删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\n类型为：[{typeof(T).FullName}]");
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
                UpdateResult result = MongoCollection.UpdateOneAsync(filter, ud).Result;
                if (result.ModifiedCount == 0 && result.MatchedCount == 1)
                {
                    MongoLog.Logger.Warning($"按ID逻辑删除操作取消，数据已被逻辑删除，无需再次逻辑删除。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    MongoLog.Logger.Error($"按ID逻辑删除时失败，无数据被更新。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
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
                    MongoLog.Logger.Error($"按ID逻辑删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\nID列表为：[{ids.ToJson()}]\r\n类型为：[{typeof(T).FullName}]");
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
                UpdateResult result = MongoCollection.UpdateManyAsync(filter, ud).Result;
                if (result.ModifiedCount != ids.Count() && result.MatchedCount == ids.Count())
                {
                    MongoLog.Logger.Warning($"按ID逻辑删除操作完成，其中部分数据在操作前已被逻辑删除。Id列表为：[{ids.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (ids.Count() != result.ModifiedCount)
                {
                    // 不完全更新，记录日志
                    MongoLog.Logger.Warning($"根据ID列表 逻辑删除一组数据 ==> 已删除数据：{result.MatchedCount}条,按条件应删除{ids.Count()}条\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{ids.ToJson()}] ");
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
                MongoLog.Logger.Error($"按ID取消逻辑删除时参数错误，无法转换成ObjectId。\r\nId值为：[{id}]\t类型为：[{typeof(T).FullName}]");
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
                UpdateResult result = MongoCollection.UpdateOneAsync(filter, ud).Result;
                if (result.ModifiedCount != 1 && result.MatchedCount == 1)
                {
                    MongoLog.Logger.Warning($"按ID逻辑删除操作完成，数据在操作前已被逻辑删除。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (1 != result.ModifiedCount)
                {
                    // 更新失败，记录日志
                    MongoLog.Logger.Error($"按ID取消逻辑删除时失败，无数据被更新。Id值为：[{objId.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
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
                    MongoLog.Logger.Error($"按ID取消逻辑删除时参数错误，无法转换成ObjectId的Id值为：[{id}]\r\nID列表为：[{ids.ToJson()}]\r\n类型为：[{typeof(T).FullName}]");
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
                UpdateResult result = MongoCollection.UpdateManyAsync(filter, ud).Result;  // 执行还原逻辑删除操作
                if (result.ModifiedCount != ids.Count() && result.MatchedCount == ids.Count())
                {
                    MongoLog.Logger.Warning($"按ID还原删除操作完成，其中部分数据在操作前是非删除状态。Id列表为：[{ids.ToString()}]\r\n类型为：[{typeof(T).FullName}]\r\n操作结果为：[{result.ToJson()}]");
                    return true;
                }
                if (ids.Count() != result.ModifiedCount)
                {
                    // 还原不完全，记录日志
                    MongoLog.Logger.Warning($"根据ID列表 取消逻辑删除一组数据 ==> 已删除数据：{result.MatchedCount}条,按条件应删除{ids.Count()}条。\r\n 类型：[{typeof(T).FullName}]\r\n查询条件为：[{ids.ToJson()}] ");
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

        #region 删除表（集合）
        /// <summary>
        /// 删除表（集合）
        /// </summary>
        public void Drop()
        {
            MongoLog.Logger.Warning($"删除集合 准备删除==> 集合名称：[{Provider.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
            try
            {
                Provider.Database.DropCollection(Provider.CollectionName);
                MongoLog.Logger.Warning($"删除集合 已删除  ==> 集合名称：[{Provider.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
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
        /// 删除表（集合） 异步
        /// </summary>
        /// <returns></returns>
        public async void DropAsync()
        {
            MongoLog.Logger.Warning($"异步删除集合 准备删除==> 集合名称：[{Provider.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
            try
            {
                await Provider.Database.DropCollectionAsync(Provider.CollectionName);
                MongoLog.Logger.Warning($"异步删除集合 已删除  ==> 集合名称：[{Provider.CollectionName}]\t对应类型：[{typeof(T).FullName}] ");
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
    }
}

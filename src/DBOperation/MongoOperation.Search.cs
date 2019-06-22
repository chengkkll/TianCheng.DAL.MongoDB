using MongoDB.Bson;
using MongoDB.Bson.Serialization;
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
        #region 数据查询
        /// <summary>
        /// 根据id查询对象
        /// </summary>
        /// <returns></returns>
        public T SearchById(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objId))
            {
                // 记录错误信息
                MongoLog.Logger.Error($"按ID查询时参数错误，无法转换成ObjectId的Id值为：[{id}]查询对象类型为：[{typeof(T).FullName}]");
                throw new ArgumentException("查询参数无效");
            }
            return SearchByTypeId(objId);
        }

        /// <summary>
        /// 根据id查询对象
        /// </summary>
        /// <returns></returns>
        public T SearchByTypeId(ObjectId id)
        {
            try
            {
                var result = MongoCollection.FindAsync(new BsonDocument("_id", id)).Result;
                List<T> objList = result.ToList();
                if (objList.Count == 0)
                {
                    MongoLog.Logger.Warning($"按ObjectId查询时,无法找到对象。ObjectId：[{id}]  查询对象类型：[{typeof(T).FullName}]");
                }
                return objList.FirstOrDefault();
            }
            catch (System.TimeoutException te)
            {
                MongoLog.Logger.Warning(te, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                throw ApiException.BadRequest("连接数据库超时，请稍后再试");
            }
            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is TimeoutException)
                    {
                        MongoLog.Logger.Warning(x, "数据库链接超时。链接字符串：" + Provider.Connection.ConnectionString());
                        throw ApiException.BadRequest("连接数据库超时，请稍后再试");
                    }
                    return false;
                });
                throw;
            }
            catch (Exception ex)
            {
                MongoLog.Logger.Warning(ex, "操作异常终止。");
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
                var query = new BsonDocument("_id", BsonSerializer.Deserialize<BsonDocument>("{'$in':[" + String.Join(",", oids) + "]}"));
                var result = MongoCollection.FindAsync(query).Result;
                List<T> objList = result.ToList();
                if (objList.Count == 0)
                {
                    MongoLog.Logger.Warning($"按ObjectId查询时,无法找到对象。ObjectId：[{ids}]  查询对象类型：[{typeof(T).FullName}]");
                }
                return objList;
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
        /// 获取当前集合的查询链式接口
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> Queryable()
        {
            return MongoCollection.AsQueryable();
        }

        /// <summary>
        /// 查看指定属性值在表中是否有重复项
        /// </summary>
        /// <param name="objectId">当前对象ID</param>
        /// <param name="prop">属性名</param>
        /// <param name="val">属性值</param>
        /// <param name="ignoreDelete">是否忽略逻辑删除数据</param>
        /// <returns></returns>
        public bool HasRepeat(ObjectId objectId, string prop, string val, bool ignoreDelete = true)
        {
            try
            {
                var builder = Builders<T>.Filter;
                FilterDefinition<T> filter = builder.Eq(prop, val);
                if (ignoreDelete)
                {
                    filter &= builder.Eq("IsDelete", false);
                }
                if (!(objectId == null || String.IsNullOrEmpty(objectId.ToString()) || objectId.Timestamp == 0 || objectId.Machine == 0 || objectId.Increment == 0))
                {
                    filter &= builder.Ne("_id", objectId);
                }
                return MongoCollection.CountDocuments(filter) > 0;
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
                MongoCollection.Find(filter).Sort(sort).ToListAsync();

                return MongoCollection.FindAsync(filter).Result.ToList();
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
                return MongoCollection.Find(filter).Sort(sort).ToListAsync().Result;
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
        /// 根据实体来查询
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Search(T entity)
        {
            try
            {
                var query = MongoSerializer.SerializeQueryModel(entity);
                return MongoCollection.FindAsync(query).Result.ToEnumerable();
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

        #region 数据统计
        /// <summary>
        /// 通过Aggregate统计查询
        /// </summary>
        public List<R> Aggregate<R>(PipelineDefinition<T, R> pipeline)
        {
            var cursor = MongoCollection.AggregateAsync(pipeline).Result;
            var result = cursor.ToList();
            return result;
        }
        #endregion
    }
}

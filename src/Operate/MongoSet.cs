using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    public class MongoSet<T> where T : MongoIdModel
    {
        public int Count
        {
            get
            {
                using (var connection = new MongoConnection<T>())
                {
                    return connection.Search().Count();
                }
            }
        }

        public MongoSet()
        {

        }

        /// <summary>
        /// 获取查询条件
        /// </summary>
        /// <returns></returns>
        public IQueryable<T> SearchQueryable()
        {
            using (var connection = new MongoConnection<T>())
            {
                try
                {
                    var entities = connection.Search();
                    if (entities == null)
                    {
                        return null;// new IQueryable<T>();
                    }

                    return entities;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 获取查询结果
        /// </summary>
        /// <returns></returns>
        public List<T> Search()
        {
            return SearchQueryable().ToList();
        }
        /// <summary>
        /// 根据id从表中查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T SearchById(string id)
        {
            using (var connection = new MongoConnection<T>())
            {
                return connection.Search(id);
            }
        }

        /// <summary>
        /// 根据id从表中查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T SearchById(ObjectId id)
        {
            using (var connection = new MongoConnection<T>())
            {
                return connection.Search(id);
            }
        }

        /// <summary>
        /// 根据对象从表中查询
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<T> Search(T entity)
        {
            using (var connection = new MongoConnection<T>())
            {
                var entities = connection.Search(entity);
                if (entities == null)
                {
                    return new List<T>();
                }

                return entities;
            }
        }

        /// <summary>
        /// 将对象保存到数据库
        /// </summary>
        /// <param name="entity"></param>
        public void Save(T entity)
        {
            using (var connection = new MongoConnection<T>())
            {
                if (entity.Id == ObjectId.Empty)
                {
                    connection.Insert(entity);
                    return;
                }
                connection.Update(entity);
            }
        }

        /// <summary>
        /// 将对象列表保存到数据库
        /// </summary>
        /// <param name="entities"></param>
        public void Save(List<T> entities)
        {
            using (var connection = new MongoConnection<T>())
            {
                foreach (var entity in entities)
                {
                    if (entity.Id == ObjectId.Empty)
                    {
                        connection.Insert(entity);
                        continue;
                    }
                    connection.Update(entity);
                }
            }
        }

        /// <summary>
        /// 按照对象删除表中的数据
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(T entity)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Delete(entity);
            }
        }


        /// <summary>
        /// 按照id删除表中的数据
        /// </summary>
        /// <param name="id"></param>
        public void Remove(string id)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Delete(id);
            }
        }
        /// <summary>
        /// 按照对象列表删除表中的数据
        /// </summary>
        /// <param name="entities"></param>
        public void Remove(List<T> entities)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Delete(entities);
            }
        }

        /// <summary>
        /// 将新数据插入到数据库
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(T entity)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Insert(entity);
            }
        }

        /// <summary>
        /// 将新数据列表插入到数据库
        /// </summary>
        /// <param name="entities"></param>
        public void Insert(List<T> entities)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Insert(entities);
            }
        }

        /// <summary>
        /// 将数据更新到数据库
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Update(entity);
            }
        }

        /// <summary>
        /// 将数据列表更新到数据库
        /// </summary>
        /// <param name="entities"></param>
        public void Update(List<T> entities)
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Update(entities);
            }
        }

        /// <summary>
        /// 删除表内所有数据
        /// </summary>
        public void Drop()
        {
            using (var connection = new MongoConnection<T>())
            {
                connection.Drop();
            }
        }
    }
}

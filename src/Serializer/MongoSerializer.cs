using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
//using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    public class MongoSerializer
    {
        /// <summary>
        /// 将BsonDocument反序列化成对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <returns></returns>
        static public T Deserialize<T>(BsonDocument document) where T : MongoIdModel
        {
            return BsonSerializer.Deserialize<T>(document);
        }

        /// <summary>
        /// 将对象序列化成BsonDocument
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        static public BsonDocument Serialize<T>(T entity) where T : MongoIdModel
        {
            return entity.ToBsonDocument();
        }

        /// <summary>
        /// 序列化查询对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        static public BsonDocument SerializeQueryModel<T>(T entity) where T : MongoIdModel
        {

            var query = new BsonDocument();

            if (entity.Id != ObjectId.Empty)
            {
                query.Add("_id", entity.Id);
                return query;
            }

            foreach (var property in typeof(T).GetProperties())
            {
                //忽略ObjectId
                if (property.PropertyType.ToString() == "MongoDB.Bson.ObjectId")
                {
                    continue;
                }

                //DateTime特殊处理
                if (property.PropertyType.ToString() == "System.DateTime")
                {
                    DateTime dateTimeValue;
                    DateTime.TryParse(property.GetValue(entity, null).ToString(), out dateTimeValue);

                    var value = property.GetValue(entity, null);

                    //忽略DateTime空值
                    if (dateTimeValue == DateTime.MinValue)
                    {
                        continue;
                    }

                    var dateTimeValueStrong = (DateTime)value;
                    var utcTime = new DateTime(dateTimeValueStrong.Ticks, DateTimeKind.Utc);
                    query.Add(property.Name, BsonDateTime.Create(utcTime));

                    continue;
                }

                //忽略空值
                if (property.GetValue(entity, null) == null)
                {
                    continue;
                }

                query.Add(property.Name, property.GetValue(entity, null).ToString());
            }

            return query;
        }
    }
}

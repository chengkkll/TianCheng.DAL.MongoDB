using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// Mongo的数据连接
    /// </summary>
    public class MongoConnection
    {
        #region 属性
        /// <summary>
        /// 获取数据库连接信息对象
        /// </summary>
        private static DBConnection Connection;

        /// <summary>
        /// 所有数据库连接服务字典
        /// </summary>
        static private Dictionary<string, MongoProvider> ProviderDict { get; set; } = new Dictionary<string, MongoProvider>();
        /// <summary>
        /// 日志使用的默认数据库连接
        /// </summary>
        public static DBConnectionOptions LoggerConnect;
        #endregion

        #region 设置数据库连接以及数据库操作对象缓存
        /// <summary>
        /// 初始化数据库的连接
        /// </summary>
        public static void SetConnection()
        {
            // 创建数据库连接信息
            if (Connection == null)
            {
                Connection = new DBConnection(DBType.MongoDB, OnChange);
            }
            if (Connection.ConnectionList.Count == 0)
            {
                ApiException.ThrowBadRequest("请填写数据库的连接配置");
            }

            // 为每个数据库连接创建数据库服务
            Dictionary<string, MongoProvider> ConnDict = new Dictionary<string, MongoProvider>();
            foreach (var conn in Connection.ConnectionList)
            {
                conn.Name = conn.Name.ToLower();
                LoggerConnect = conn;
                if (string.IsNullOrWhiteSpace(conn.Name))
                {
                    ApiException.ThrowBadRequest("请为连接数据库的字符串指定名称");
                }
                if (ConnDict.ContainsKey(conn.Name))
                {
                    continue;
                }
                try
                {
                    // 获取数据链接客户端
                    IMongoClient MongoClient = new MongoClient(conn.ConnectionString());
                    // 获取数据库
                    IMongoDatabase MongoDatabase = MongoClient.GetDatabase(conn.Database);
                    ConnDict.Add(conn.Name, new MongoProvider { Client = MongoClient, Database = MongoDatabase, Connection = conn });
                }
                catch (Exception ex)
                {
                    // 由于数据库连接字符串容易被外界改变，所以只针对连接字符串出错时记录日志并发送邮件。其它错误应该在测试阶段发现。
                    DBLog.Logger.Error(ex, "连接MongoDB数据库失败", Connection.ConnectionList);
                    throw new Exception("连接数据库失败，请检查数据库连接字符串");
                }
            }

            // 根据持久化操作对象的特性创建数据库操作服务
            ProviderDict.Clear();
            foreach (var map in LoadMapping())
            {
                if (ProviderDict.ContainsKey(map.DALTypeName))
                {
                    continue;
                }

                if (!ConnDict.ContainsKey(map.ConnectionName))
                {
                    ApiException.ThrowBadRequest($"Mongo数据库{map.ConnectionName}连接信息不存在，请检查配置项");
                }
                var db = ConnDict[map.ConnectionName];
                ProviderDict.Add(map.DALTypeName, new MongoProvider
                {
                    Name = map.DALTypeName,
                    Client = db.Client,
                    Database = db.Database,
                    Connection = db.Connection,
                    CollectionName = map.CollectionName,
                    Collection = db.Database.GetType().GetMethod("GetCollection").MakeGenericMethod(map.ModelType).Invoke(db.Database, new object[] { map.CollectionName, null })
                });
            }
        }
        #endregion

        #region 根据特性获取所有数据库操作对象
        /// <summary>
        /// 获取程序集中数据访问操作的所有特性信息
        /// </summary>
        private static IEnumerable<DBMappingAttribute> LoadMapping()
        {
            foreach (var assembly in AssemblyHelper.GetAssemblyList())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // 忽略接口
                    if (type.IsInterface) continue;
                    // 忽略带泛型的定义 泛型定级均为基类
                    if (type.Name.Contains("`")) continue;
                    // 类型必须继承 IDBOperation 接口
                    if (type.GetInterfaces().Where(i => i.ToString().Contains("IDBOperation")).Count() == 0) continue;

                    // 获取数据持久化操作的特性
                    DBMappingAttribute attribute = type.GetCustomAttribute<DBMappingAttribute>(false);
                    // 数据操作必须有特性，严格检查避免代码书写错误
                    if (attribute == null)
                    {
                        ApiException.ThrowBadRequest($"数据持久化操作对象：{type.FullName} 必须设置数据映射特性：DBMapping");
                    }
                    // 完善特性信息
                    attribute.DALTypeName = type.FullName;

                    string baseName = type.BaseType.FullName;
                    int start1 = baseName.IndexOf("`1[[") + 4;
                    string ModelTypeName = baseName.Substring(start1, baseName.IndexOf(",") - start1);
                    int start2 = baseName.IndexOf(",") + 1;
                    string ModelAssemblyName = baseName.Substring(start2, baseName.IndexOf(",", start2) - start2);

                    attribute.ModelType = AssemblyHelper.GetTypeByName(ModelAssemblyName, ModelTypeName);

                    attribute.DBType = DBType.MongoDB;
                    if (string.IsNullOrWhiteSpace(attribute.ConnectionName))
                    {
                        attribute.ConnectionOptions = Connection.Default;
                        attribute.ConnectionName = Connection.Default.Name.ToLower();
                    }
                    else
                    {
                        attribute.ConnectionName = attribute.ConnectionName.ToLower();
                        attribute.ConnectionOptions = Connection.ByName(attribute.ConnectionName);
                    }
                    // 返回特性信息
                    yield return attribute;
                }
            }
        }
        #endregion

        #region 获取数据库操作对象实例
        /// <summary>
        /// 根据持久化对象名称获取数据库操作对象
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static MongoProvider GetProvider(string typeName)
        {
            if (ProviderDict.Count == 0)
            {
                new MongoLoader().Init();
            }

            if (ProviderDict.ContainsKey(typeName))
            {
                return ProviderDict[typeName];
            }
            throw ApiException.BadRequest($"指定的对象{typeName}没有对应的数据库操作服务，请检查DBMapping特性和数据库配置");
        }
        #endregion

        #region 数据库连接修改
        /// <summary>
        /// 数据库连接修改事件
        /// </summary>
        static public Action ConnectionChange;
        /// <summary>
        /// 数据库连接串修改事件处理
        /// </summary>
        /// <param name="connectionList"></param>
        private static void OnChange(List<DBConnectionOptions> connectionList)
        {
            // 重新连接所有的数据库
            SetConnection();
            // 传递修改事件
            ConnectionChange?.Invoke();
        }
        #endregion
    }
}
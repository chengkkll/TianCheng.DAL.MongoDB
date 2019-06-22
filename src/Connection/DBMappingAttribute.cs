using System;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 链接数据库集合特性信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DBMappingAttribute : Attribute
    {
        /// <summary>
        /// 集合/表 名称
        /// </summary>
        public string CollectionName { get; set; }
        /// <summary>
        /// 对应数据库链接配置名
        /// </summary>
        public string ConnectionName { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DBType DBType { get; set; } = DBType.MongoDB;

        /// <summary>
        /// 链接数据库的配置信息
        /// </summary>
        public DBConnectionOptions ConnectionOptions { get; set; } = new DBConnectionOptions();

        /// <summary>
        /// 数据库操作对象类名
        /// </summary>
        public string DALTypeName { get; set; }
        /// <summary>
        /// 实体对象类型
        /// </summary>
        public Type ModelType { get; set; }

        #region 构造方法
        /// <summary>
        /// 构造方法
        /// </summary>
        public DBMappingAttribute()
        {

        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="collectionName">集合/表 名称</param>
        /// <param name="optionsName">数据库链接名</param>
        public DBMappingAttribute(string collectionName, string optionsName = DBConnection.DefaultOptionName)
        {
            CollectionName = collectionName;
            ConnectionName = optionsName;
        }
        #endregion
    }
}

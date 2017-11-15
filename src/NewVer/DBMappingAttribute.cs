using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 链接数据库集合特性信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class DBMappingAttribute : Attribute
    {
        /// <summary>
        /// 集合名称
        /// </summary>
        public string CollectionName { get; set; }
        /// <summary>
        /// 对应数据库链接配置名
        /// </summary>
        public string OptionsName { get; set; }

        /// <summary>
        /// 链接数据库的配置信息
        /// </summary>
        internal DBConnectionOptions ConnectionOptions { get; set; } = new DBConnectionOptions();

        /// <summary>
        /// 
        /// </summary>
        internal string TypeName { get; set; }

        #region 构造方法
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="collectionName">集合/表 名称</param>
        public DBMappingAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="collectionName">集合/表 名称</param>
        /// <param name="optionsName">数据库链接名</param>
        public DBMappingAttribute(string collectionName, string optionsName)
        {
            CollectionName = collectionName;
            OptionsName = optionsName;
        }
        #endregion
    }
}

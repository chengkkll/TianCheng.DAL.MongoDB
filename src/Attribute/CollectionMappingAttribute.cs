using System;
using System.Collections.Generic;
using System.Text;

namespace TianCheng.DAL.MongoDB
{

    /// <summary>
    /// 设置数据集合信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class CollectionMappingAttribute : Attribute
    {
        /// <summary>
        /// 集合名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name"></param>
        public CollectionMappingAttribute(string name)
        {
            Name = name;
        }
    }
}

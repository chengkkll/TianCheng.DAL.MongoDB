using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TianCheng.DAL
{
    /// <summary>
    /// 数据库操作接口
    /// </summary>
    public interface IDBOperation<T>
    {
        #region 查询处理

        /// <summary>
        /// 根据id来查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T SearchById(string id);

        /// <summary>
        /// 获取当前集合的查询链式接口
        /// </summary>
        /// <returns></returns>
        IQueryable<T> Queryable();
        #endregion

        #region 数据的插入
        /// <summary>
        /// 插入单条新数据
        /// </summary>
        /// <param name="entity"></param>
        void Insert(T entity);

        ///// <summary>
        ///// 插入多条新数据
        ///// </summary>
        ///// <param name="entities"></param>
        //void Insert(IEnumerable<T> entities);
        #endregion

        #region 数据更新
        /// <summary>
        /// 更新单条数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        bool Update(T entity);
        ///// <summary>
        ///// 更新多条数据
        ///// </summary>
        ///// <param name="entities"></param>
        //void Update(IEnumerable<T> entities);

        #endregion

        #region 物理删除数据
        /// <summary>
        /// 根据ID列表 物理删除一组数据
        /// </summary>
        /// <param name="ids"></param>
        void Remove(IEnumerable<string> ids);
        /// <summary>
        /// 根据ID 物理删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T Remove(string id);
        #endregion

    }
}

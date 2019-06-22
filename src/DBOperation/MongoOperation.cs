using MongoDB.Driver;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// MongoDB 数据库操作处理
    /// </summary>
    public partial class MongoOperation<T> : IMongoDBOperation<T> where T : TianCheng.Model.MongoIdModel
    {
        #region 构造
        /// <summary>
        /// 表（集合）
        /// </summary>
        protected IMongoCollection<T> MongoCollection { get; private set; }
        /// <summary>
        /// 数据库操作服务
        /// </summary>
        protected MongoProvider Provider { get; private set; }

        /// <summary>
        /// 构造方法处理
        /// </summary>
        public MongoOperation()
        {
            // 数据库连接修改事件
            MongoConnection.ConnectionChange = OnConnectionChange;
            // 获取操作当前数据集合的服务
            Provider = MongoConnection.GetProvider(this.GetType().FullName);
            // 获取一个表（集合）操作
            MongoCollection = Provider.Collection;
        }
        /// <summary>
        /// 数据库连接修改事件
        /// </summary>
        public void OnConnectionChange()
        {
            // 重新获取操作当前数据集合的服务
            Provider = MongoConnection.GetProvider(this.GetType().FullName);
        }
        #endregion
    }
}

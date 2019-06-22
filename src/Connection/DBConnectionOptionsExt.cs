namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 数据库链接配置
    /// </summary>
    static public class DBConnectionOptionsExt
    {
        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        static public string ConnectionString(this DBConnectionOptions options)
        {
            if (options.ServerAddress.EndsWith("/") || options.Database.StartsWith("/"))
            {
                return options.ServerAddress + options.Database;
            }
            return options.ServerAddress + "/" + options.Database;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianCheng.DAL.MongoDB
{
    static public class Configs
    {

        static public DatabaseInfo GetDatabaseConfig(DatabaseType type, ConfigMode mode, string application)
        {
            switch (type)
            {
                case DatabaseType.MongoDB:
                    return new XMLConfig(type).Find(application);
                default:
                    return new DatabaseInfo();
            }
        }

        /// <summary>
        /// 获取数据库的连接信息
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        static public DatabaseInfo GetDatabaseConfig(string application)
        {
            try
            {
                return new XMLConfig(DatabaseType.MongoDB).Find(application);

            }
            catch (Exception)
            {
                // Log
                return null;
            }
        }

        /// <summary>
        /// 获取数据库配置文件路径
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static public string GetDatabasePath(DatabaseType type)
        {
            switch (type)
            {
                case DatabaseType.MongoDB:
                    return ConfigPath.MongoDB;
                default:
                    return ConfigPath.MongoDB;
            }
        }
    }

}

using MongoDB.Bson.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 数据库连接信息对象
    /// </summary>
    static public class MongoContext
    {
        #region 属性
        private static MongoConnectionSettings _Setting = null;
        //标识是否已经初始化
        private static bool _Initializationed = false;
        //初始化锁，避免多次初始化
        private static object _lock = new object();
        /// <summary>
        /// 获取Mongo数据库连接配置
        /// </summary>
        public static MongoConnectionSettings Settings
        {
            get
            {
                if (_Setting == null)
                {
                    //初始化MongoDB类
                    lock (_lock)
                    {
                        if (!_Initializationed)
                        {
                            //注册DateTimeSerializer
                            try
                            {
                                BsonSerializer.RegisterSerializer(typeof(DateTime), new MongoDateTimeSerializer());
                            }
                            catch
                            {

                            }
                            //获取连接字符串
                            LoadSettingsByFile();
                            _Initializationed = true;
                        }
                    }
                }
                return _Setting;
            }
        }

        #endregion

        #region 获取连接的字符串信息
        /// <summary>
        /// 配置文件所在位置
        /// </summary>
        private const string SettingsFileName = "appSettings.json";
        /// <summary>
        /// 数据库配置节点名
        /// </summary>
        private const string SettingsNodeName = "DatabaseInfo";

        /// <summary>
        /// 从配置文件获取数据库信息
        /// </summary>
        static private void LoadSettingsByFile()
        {
            try
            {
                var jSettings = JObject.Parse(File.ReadAllText(SettingsFileName))[SettingsNodeName];
                var settings = JsonConvert.DeserializeObject<MongoConnectionSettings>(jSettings.ToString());
                if (settings == null || String.IsNullOrWhiteSpace( settings.Database) || String.IsNullOrWhiteSpace(settings.ServerAddress))
                {
                    settings = MongoConnectionSettings.Default;
                }
                _Setting = settings;
            }
            catch (Exception)
            {
                _Setting = MongoConnectionSettings.Default;
            }
        }
        #endregion
    }
}

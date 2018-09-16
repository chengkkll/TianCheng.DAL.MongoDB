using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TianCheng.Model;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// 链接数据库的缓存处理
    /// </summary>
    public class ConnectionCaching : CachingFromFile<Dictionary<string, DBMappingAttribute>>
    {
        protected override string CacheKey { get { return "AppsettingCaching_DatabaseInfo"; } }

        protected override string DependentFile { get { return "appsettings.json"; } }

        // private IHostingEnvironment _env;
        public ConnectionCaching()
        {
            //_env = env;
        }
        /// <summary>
        /// 配置文件更新后处理
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        protected override Dictionary<string, DBMappingAttribute> ReadFile(string fileContent)
        {
            //DBLog.UpdateLogSetting(fileContent);
            LoadOptions(fileContent);
            return LoadAttribute();
        }

        /// <summary>
        /// 获取所有的特性信息
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, DBMappingAttribute> LoadAttribute()
        {
            Dictionary<string, DBMappingAttribute> result = new Dictionary<string, DBMappingAttribute>();
            foreach (var assembly in TianCheng.Model.AssemblyHelper.GetAssemblyList())
            {
                foreach (Type type in assembly.GetTypes())
                {

                    if (type.GetInterfaces().Where(i => i.ToString().Contains("IDBOperation")).Count() == 0)
                    {
                        continue;
                    }
                    if (type.IsInterface) continue;
                    //if (type.GetInterface("IDBOperation`") == null)
                    //{
                    //    continue;
                    //}
                    if (type.Name.Contains("`"))    // 自动忽略带泛型的定义
                    {
                        continue;
                    }
                    DBMappingAttribute attribute = type.GetCustomAttribute<DBMappingAttribute>(false);
                    if (attribute != null)
                    {
                        if (!result.ContainsKey(type.FullName))
                        {
                            attribute.DALTypeName = type.FullName;
                            if (String.IsNullOrWhiteSpace(attribute.ConnectionName))
                            {
                                attribute.ConnectionOptions = DefaultOptions();
                            }
                            else
                            {
                                attribute.ConnectionName = attribute.ConnectionName.ToLower();
                                attribute.ConnectionOptions = Options(attribute.ConnectionName);
                            }
                            result.Add(attribute.DALTypeName, attribute);
                        }
                    }
                    else
                    {
                        throw new Exception(type.FullName + "必须设置DBMapping的特性");
                    }
                }
            }
            return result;
        }


        #region 设置缓存处理
        /// <summary>
        /// 根据文件依赖设置缓存信息
        /// </summary>
        public override Dictionary<string, DBMappingAttribute> SetCache()
        {
            Dictionary<string, DBMappingAttribute> val = ReadFile("");
            SetCache(CacheKey, val);
            return val;
        }
        #endregion

        private List<DBConnectionOptions> OptionsList = new List<DBConnectionOptions>();
        /// <summary>
        /// 获取配置信息中的数据库连接配置
        /// </summary>
        /// <param name="settingContent"></param>
        /// <returns></returns>
        private void LoadOptions(string settingContent)
        {
            var configuration = TianCheng.Model.AppConfig.Configuration;
            OptionsList.Clear();
            for (int i = 0; true; i++)
            {
                string serverAddress = configuration.GetSection($"DBConnection:{i}:ServerAddress").Value;
                if (String.IsNullOrWhiteSpace(serverAddress))
                {
                    break;
                }
                string type = configuration.GetSection($"DBConnection:{i}:Type").Value;
                if (type.ToLower() != "mongo")
                {
                    continue;
                }
                string name = configuration.GetSection($"DBConnection:{i}:Name").Value;
                string database = configuration.GetSection($"DBConnection:{i}:Database").Value;

                OptionsList.Add(new DBConnectionOptions
                {
                    Name = name,
                    ServerAddress = serverAddress,
                    Database = database,
                    Type = type
                });
            }
            if (OptionsList.Count == 0)
            {
                throw new Exception("请填写数据库的链接配置");
            }

            //// 获取链接字符串的节点
            //var jSettings = JObject.Parse(settingContent)["DBConnection"];
            //if (jSettings == null)
            //{
            //    throw new Exception("请填写数据库的链接配置");
            //}
            //// 获取所有的数据库链接配置信息
            //OptionsList = JsonConvert.DeserializeObject<List<DBConnectionOptions>>(jSettings.ToString());
            //OptionsList = OptionsList.Where(e => e.Type != null && e.Type.ToLower() == "mongo").ToList();
            //foreach (var item in OptionsList)
            //{
            //    item.Name = item.Name.ToLower();
            //}
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DBConnectionOptions DefaultOptions()
        {
            if (OptionsList.Count == 0)
            {
                throw new Exception("请填写数据库的链接配置");
            }

            if (OptionsList.Count == 1)
            {
                return OptionsList.FirstOrDefault();
            }

            //if (!_env.IsDevelopment())
            //{
            //    var option = OptionsList.Where(e => e.Name == "release" || e.Name == "issue" || e.Name == "publish").FirstOrDefault();
            //    if (option != null)
            //    {
            //        return option;
            //    }
            //}
            return OptionsList.FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private DBConnectionOptions Options(string name)
        {
            return OptionsList.Where(e => e.Name.Equals(name.ToLower())).FirstOrDefault();
        }
    }
}

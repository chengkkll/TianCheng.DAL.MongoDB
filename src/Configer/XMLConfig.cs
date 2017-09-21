using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace TianCheng.DAL.MongoDB
{
    public class XMLConfig
    {
        private string _configPath;

        public XMLConfig(DatabaseType type)
        {
            this._configPath = Configs.GetDatabasePath(type);
        }


        /// <summary>
        /// 根据数据库类型保存模块的数据库信息
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public void Create(DatabaseInfo db)
        {
            var root = new XDocument(new XElement("applications", BuildXMLContext(db)));
            root.Declaration = new XDeclaration("1.0", "utf-8", "yes");
            FileStream fs = new FileStream(_configPath, FileMode.CreateNew);
            root.Save(fs);
            fs.Dispose();
            //root.Save(this._configPath);
        }

        /// <summary>
        /// 增加配置信息
        /// </summary>
        /// <param name="db"></param>
        public void Add(DatabaseInfo db)
        {
            var doc = XDocument.Load(this._configPath);
            doc.Element("applications").Add(BuildXMLContext(db));

            FileStream fs = new FileStream(_configPath, FileMode.OpenOrCreate);
            doc.Save(fs);
            fs.Dispose();
            //doc.Save(this._configPath);
        }

        /// <summary>
        /// 修改配置信息
        /// </summary>
        /// <param name="dbInfo"></param>
        public void Update(DatabaseInfo dbInfo)
        {
            var root = XDocument.Load(this._configPath);

            foreach (var app in root.Element("applications").Elements())
            {
                if (app.Attribute("app_name").Value.ToString() == dbInfo.Application)
                {
                    app.SetElementValue("data_base_name", dbInfo.DatabaseName);
                    app.SetElementValue("data_base_server", dbInfo.Server);
                    app.SetElementValue("connection_string", dbInfo.ConnectionString);
                }
            }

            FileStream fs = new FileStream(_configPath, FileMode.Open);
            root.Save(fs);
            fs.Dispose();
            //root.Save(this._configPath);
        }

        /// <summary>
        /// 是否存在重复节点
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <returns></returns>
        private bool HasSameNode(DatabaseInfo dbInfo)
        {
            if (!File.Exists(this._configPath))
            {
                return false;
            }

            foreach (var app in XDocument.Load(this._configPath).Element("applications").Elements())
            {
                if (app.Attribute("app_name").Value.ToString() == dbInfo.Application)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 构建XML上下文
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private XElement BuildXMLContext(DatabaseInfo db)
        {
            return new XElement("application",
                    new XAttribute("app_name", db.Application),
                    new XElement("data_base_name", db.DatabaseName),
                    new XElement("data_base_server", db.Server),
                    new XElement("connection_string", db.ConnectionString)
                );
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="dbInfo"></param>
        /// <param name="type"></param>
        public void Save(DatabaseInfo dbInfo, DatabaseType type)
        {
            if (File.Exists(this._configPath))
            {
                if (HasSameNode(dbInfo))
                {
                    Update(dbInfo);
                }
                else
                {
                    Add(dbInfo);
                }
            }
            else
            {
                Create(dbInfo);
            }
        }

        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="db"></param>
        public void Save(DatabaseInfo db)
        {
            Save(db, DatabaseType.MongoDB);
        }

        /// <summary>
        /// 获取模块的数据库信息
        /// </summary>
        /// <param name="application"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public DatabaseInfo Find(string application, DatabaseType type)
        {
            if (File.Exists(this._configPath))
            {
                var applications = XDocument.Load(this._configPath).Element("applications").Elements();

                return applications
                     .Where(e => e.Attribute("app_name").Value.ToString() == application)
                     .Select(e => new DatabaseInfo
                     {
                         Application = e.Attribute("app_name").Value,
                         DatabaseName = e.Element("data_base_name").Value,
                         Server = e.Element("data_base_server").Value,
                         ConnectionString = e.Element("connection_string").Value
                     })
                     .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// 获取模块的数据库信息
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public DatabaseInfo Find(string application)
        {
            return Find(application, DatabaseType.MongoDB);
        }

    }

}

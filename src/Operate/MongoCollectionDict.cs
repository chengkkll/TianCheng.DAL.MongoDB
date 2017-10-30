using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// MongoDB集合的字典
    /// </summary>
    public class MongoCollectionDict
    {
        static private Dictionary<string, string> collectionDict = null;

        private static readonly object obj = new object();

        ///// <summary>
        ///// 获取有效的程序集
        ///// </summary>
        ///// <returns></returns>
        //static private List<Assembly> GetAssemblyList()
        //{
        //    //获取根目录            
        //    string runPath = AppContext.BaseDirectory;
        //    //获取可能是程序集的文件列表
        //    string[] fileArray = Directory.GetFiles(runPath, "*.dll", SearchOption.AllDirectories);
        //    //获取程序集名称
        //    List<string> fileList = new List<string>();
        //    foreach (string file in fileArray)
        //    {
        //        int start = file.LastIndexOf("\\");
        //        string assemblyFile = "";
        //        if (start > 0 && start + 1 < file.Length)
        //        {
        //            assemblyFile = file.Substring(start + 1);
        //        }
        //        assemblyFile = assemblyFile.Replace(".dll", "");
        //        if (!String.IsNullOrEmpty(assemblyFile))
        //        {
        //            fileList.Add(assemblyFile);
        //        }
        //    }
        //    //获取有效的程序集
        //    List<Assembly> assemblyList = new List<Assembly>();
        //    foreach (string file in fileList)
        //    {
        //        AssemblyName an = new AssemblyName(file);
        //        if (an != null)
        //        {
        //            Assembly assembly = Assembly.Load(an);
        //            assemblyList.Add(assembly);
        //        }
        //    }
        //    //返回有效的程序集
        //    return assemblyList;
        //}

        /// <summary>
        /// 初始化集合字典
        /// </summary>
        static public void Init()
        {
            lock (obj)
            {
                if(collectionDict != null)
                {
                    return;
                }
                else
                {
                    collectionDict = new Dictionary<string, string>();
                }
                foreach (Assembly assembly in TianCheng.Model.AssemblyHelper.GetAssemblyList())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        CollectionMappingAttribute attribute = type.GetTypeInfo().GetCustomAttribute<CollectionMappingAttribute>(false);    //false 不获取基类中的特性
                        string typeName = type.Name;
                        string collectionName = typeName;
                        if (attribute != null)
                        {
                            collectionName = attribute.Name;
                        }
                        if (collectionDict.ContainsKey(typeName))
                        {
                            continue;
                            //throw ApiException.ConnectionDB("指定对象存储的表有重复项，请检查：" + typeName + "。");
                        }
                        collectionDict.Add(typeName, collectionName);
                    }
                }
            }
        }
        /// <summary>
        /// 获取字典信息
        /// </summary>
        static public Dictionary<string, string> Collection
        {
            get
            {
                if (collectionDict == null)
                {
                    Init();
                }
                return collectionDict;
            }
        }
    }
}

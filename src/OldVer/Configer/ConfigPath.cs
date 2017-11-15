using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace TianCheng.DAL.MongoDB
{
    public class ConfigPath
    {
        static public string MongoDB
        {
            get
            {
                return AppContext.BaseDirectory + @"MongoDB.config.xml";
            }
        }

    }
}

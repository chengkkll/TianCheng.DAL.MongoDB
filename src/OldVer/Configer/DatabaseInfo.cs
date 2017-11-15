using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianCheng.DAL.MongoDB
{
    public class DatabaseInfo
    {
        public string ConnectionString;
        public string Server;
        public string DatabaseName;
        public string Application;
        public string MinConnectionPoolSize;
        public string MaxConnectionPoolSize;
        public string ConnectTimeout;
        public string SocketTimeout;
        public string WaitQueueTimeout;
        public string ServerSelectionTimeout;
    }
}

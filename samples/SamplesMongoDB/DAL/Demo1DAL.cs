using Microsoft.AspNetCore.Hosting;
using SamplesMongoDB.Model;
using System;
using System.Collections.Generic;
using System.Text;
using TianCheng.DAL.MongoDB;

namespace SamplesMongoDB.DAL
{
    /// <summary>
    /// 
    /// </summary>
    [DBMapping("test_demo")]
    public class DemoDAL : MongoOperation<DemoInfo>
    {
        public DemoDAL()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Test()
        {
            //var result = Undelete(new List<string>() { "5a02b873306ad0550c48e305", "5a02b9cf03e96c56c8c93f17" });
            //result = Delete(new List<string>() { "5a02b873306ad0550c48e305", "5a02b9cf03e96c56c8c93f17" });

            var t = SearchById("5a02b873306ad0550c48e305");
            // var result = Undelete("5a02b873306ad0550c48e305");
            //Insert(new DemoInfo() { Name = Guid.NewGuid().ToString() });
        }
    }
}

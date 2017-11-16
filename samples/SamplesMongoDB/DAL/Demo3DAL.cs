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
    [DBMapping("test_demo", "Hangfire")]
    class Demo3DAL : MongoOperation<DemoInfo>
    {
        public Demo3DAL()
        {
        }
    }
}

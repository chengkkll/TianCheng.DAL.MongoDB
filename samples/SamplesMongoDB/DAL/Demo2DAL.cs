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
    [DBMapping("test_demo", "debug")]
    class Demo2DAL : MongoOperation<DemoInfo>
    {
        public Demo2DAL()
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DBMapping("test_demo")]
    class Demo3DAL : MongoOperation<DemoInfo>
    {
        public Demo3DAL()
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using TianCheng.DAL.MongoDB;
using TianCheng.Model;

namespace SamplesMongoDB.Model
{
    [CollectionMapping("demo_d1")]
    public class DemoInfo : BusinessMongoModel
    {
        public string Name { get; set; }

        public DateTime Date { get; set; }
    }
}

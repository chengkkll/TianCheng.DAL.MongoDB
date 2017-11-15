using MongoDB.Bson.Serialization.Attributes;
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
        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("test_date")]
        public DateTime Date { get; set; }
    }
}

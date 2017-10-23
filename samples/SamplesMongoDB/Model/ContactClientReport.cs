using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using TianCheng.Model;

namespace SamplesMongoDB.Model
{
    public class ContactClientReport : MongoIdModel
    {
        ///// <summary>
        ///// 业务员所在部门ID
        ///// </summary>
        //[BsonElement("DepartmentId")]
        //public string DepartmentId { get; set; }

        [BsonElement("total")]
        public int Total { get; set; }
    }
}

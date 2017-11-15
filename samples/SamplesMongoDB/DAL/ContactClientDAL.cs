using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SamplesMongoDB.Model;
using System;
using System.Collections.Generic;
using System.Text;
using TianCheng.DAL.MongoDB;

namespace SamplesMongoDB.DAL
{
    public class ContactClientDAL : DALCommon<ContactClientInfo>
    {
        public List<ContactClientReport> Report()
        {
            //var group = new BsonDocument();
            //group.AddRange(new BsonDocument("_id",  { "CreaterDepartmentId", "$CreaterDepartmentId" }});
            //group.AddRange(new BsonDocument("total", new BsonDocument("$sum", 1)));



            string json = "$group : { '_id': { 'CreaterDepartmentId' : '$CreaterDepartmentId','CreaterDepartmentName':'$CreaterDepartmentName'} ,'count4':{ $sum :{ '$cond' : [ { '$eq' : [ '$OrderStatus' , 64] } , 1 , 0 ] } }, }";

            BsonDocument groupBson = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(json);


            IList<IPipelineStageDefinition> stages = new List<IPipelineStageDefinition>();
            PipelineStageDefinition<BsonDocument, BsonDocument> stage1 = new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(json);



            PipelineDefinition<ContactClientInfo, ContactClientReport> pipeline = new BsonDocument[]
            {
                groupBson
            };


            return Aggregate(pipeline);


            /*
            接口一：
            输入条件：
	            开始日期，结束日期
            输出条件：
	            团队名称，团队ID，前端话务员成单数量，录音员成单数量，后端话务员成单数量， 开始日期，结束日期
            */

            //db.RT_OrderInfo.aggregate([
            //                        { $match:
            //                                {
            //                                    "CreaterDepartmentId":"59eab9651c89161750c4156c",
            //                                    "CreateDate" : {$gte: ISODate("2017-10-22")},
            //                                    "CreateDate" : {$lte: ISODate("2017-10-27")}
            //                                }
            //                        },
            //                        {
            //                            $group:
            //                                {
            //                                    "_id": { "CreaterDepartmentId" : "$CreaterDepartmentId","CreaterDepartmentName":"$CreaterDepartmentName"} ,
            //                                        "count1":{ $sum: { "$cond" : [ { "$eq" : [ "$OrderStatus" , 2]  } , 1 , 0 ] } },
            //                                        "count2":{ $sum :{ "$cond" : [ { "$eq" : [ "$OrderStatus" , 4]  } , 1 , 0 ] } },
            //                                        "count3":{ $sum :{ "$cond" : [ { "$eq" : [ "$OrderStatus" , 32] } , 1 , 0 ] } },
            //                                        "count4":{ $sum :{ "$cond" : [ { "$eq" : [ "$OrderStatus" , 64] } , 1 , 0 ] } },
            //                                 }
            //                        }
            //                    ])




        }

        public List<ContactClientInfo> TestFind()
        {


            //FilterDefinition<ContactClientInfo> filter = Builders<ContactClientInfo>.Filter.Eq("Client.Name", "哈尔滨业勤服装服饰有限公司");

            var builder = Builders<ContactClientInfo>.Filter;
            var filter = builder.Empty;

            filter = filter & builder.Eq("Client.Name", "哈尔滨业勤服装服饰有限公司");

            filter = filter & builder.Ne("Client.Linkman", "纪1军");





            return FindByMongodb(filter);
        }

        
        //public List<object> Demo()
        //{
        //    const string pipelineJson1 = " {$skip : 5}";
        //    const string pipelineJson2 = " {$project : {_id : 0 ,author : 1}}";
        //    const string pipelineJson3 = " {$group: {_id: \"$author\", count: {$sum: 1}}}";
        //    const string pipelineJson4 = " {$group: {_id: \"$tags.python\", count: {$sum: 1}}}";
        //    const string pipelineJson5 = " {$match: {tags.pymongo:\"888\"}";  //得到匹配条件满足的结果  
        //    const string pipelineJson6 = " {$sort: {count:-1}";  //降序  
        //    const string pipelineJson7 = " {$limit: 5}";         //返回当前结果的前5个文档  
        //    IList<IPipelineStageDefinition> stages = new List<IPipelineStageDefinition>();
        //    PipelineStageDefinition<BsonDocument, BsonDocument> stage1 = new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(pipelineJson1);
        //    PipelineStageDefinition<BsonDocument, BsonDocument> stage2 = new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(pipelineJson2);
        //    PipelineStageDefinition<BsonDocument, BsonDocument> stage3 = new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(pipelineJson3);
        //    PipelineStageDefinition<BsonDocument, BsonDocument> stage4 = new JsonPipelineStageDefinition<BsonDocument, BsonDocument>(pipelineJson4);
        //    //stages.Add(stage1);  
        //    //stages.Add(stage2);  
        //    //stages.Add(stage3);  
        //    stages.Add(stage4);
        //    PipelineDefinition<BsonDocument, BsonDocument> pipeline = new PipelineStagePipelineDefinition<BsonDocument, BsonDocument>(stages);
        //    var result = Aggregate(pipeline);
        //}
    }
}

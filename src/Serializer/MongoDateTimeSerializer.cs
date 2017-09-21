﻿using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TianCheng.DAL.MongoDB
{
    /// <summary>
    /// MongoDB将时间保存为UTC标准时间，而系统中使用的时间为Local时间
    /// 此处重写以做出转换
    /// </summary>
    public class MongoDateTimeSerializer : DateTimeSerializer
    {
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var utcTime = base.Deserialize(context, args);
            return new DateTime(utcTime.Ticks, DateTimeKind.Local);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime localTime)
        {
            var utcTime = new DateTime(localTime.Ticks, DateTimeKind.Utc);
            base.Serialize(context, args, utcTime);
        }


    }
}
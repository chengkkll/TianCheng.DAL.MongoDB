using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TianCheng.DAL.MongoDB;
using WebApi.Model;

namespace WebApi.DAL
{
    /// <summary>
    /// 按数据库连接名debug的配置操作表test_demo
    /// </summary>
    [DBMapping("test_demo", "debug")]
    class DemoDebugDAL : MongoOperation<DemoInfo>
    {

    }
}

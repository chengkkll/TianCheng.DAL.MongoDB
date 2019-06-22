using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TianCheng.DAL.MongoDB;
using WebApi.Model;

namespace WebApi.DAL
{
    /// <summary>
    /// 按默认数据库连接操作表test_demo
    /// </summary>
    [DBMapping("test_demo")]
    public class DemoDAL : MongoOperation<DemoInfo>
    {

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApi.DAL;
using WebApi.Model;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// 读取default库的数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("default")]
        public IEnumerable<DemoInfo> GetDefault()
        {
            return new DemoDAL().Queryable();
        }

        /// <summary>
        /// 读取debug库的数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("debug")]
        public IEnumerable<DemoInfo> GetDebug()
        {
            return new DemoDebugDAL().Queryable();
        }

        /// <summary>
        /// 写default库的数据
        /// </summary>
        [HttpPost("default")]
        public void InsertDefault()
        {
            new DemoDAL().InsertObject(new DemoInfo() { Name = Guid.NewGuid().ToString().Substring(0, 7), Code = "default", Date = DateTime.Now });
        }

        /// <summary>
        /// 写debug库的数据
        /// </summary>
        [HttpPost("debug")]
        public void InsertDebug()
        {
            new DemoDebugDAL().InsertObject(new DemoInfo() { Name = Guid.NewGuid().ToString(), Code = "debug", Date = DateTime.Now });
        }
    }
}

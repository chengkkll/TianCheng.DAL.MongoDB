using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SamplesMongoDB.DAL;
using SamplesMongoDB.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SamplesMongoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                DemoDAL dal = new DemoDAL();

                DemoInfo info = new DemoInfo() { Name = Guid.NewGuid().ToString(), Date = DateTime.Now };
                dal.Insert(info);
                dal.Drop();

                //new Demo2DAL().Insert(new DemoInfo() { Name = Guid.NewGuid().ToString(), Date = DateTime.Now });
                //new Demo3DAL().Insert(new DemoInfo() { Name = Guid.NewGuid().ToString(), Date = DateTime.Now });

                //DemoInfo info3 = new Demo3DAL().SearchById("5a0952fba1ef684b5810bdff1");
                //Console.WriteLine(info3.Date.ToString());
                //Console.WriteLine("操作完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Read();
        }
    }
}

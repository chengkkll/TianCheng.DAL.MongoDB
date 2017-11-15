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
            //DemoInfo info = new DemoInfo();
            //info.Date = DateTime.Now;
            //DemoDAL dal = new DemoDAL();
            //dal.Insert(info);
            //Console.WriteLine("date:" + info.Date);

            //var demo = dal.SearchQueryable().FirstOrDefault();
            //Console.WriteLine("show:" + demo.Date);

            //Console.WriteLine("insert ok");


            //ContactClientDAL ccDal = new ContactClientDAL();

            //List<ContactClientReport> result = ccDal.Report();

            // var result = ccDal.TestFind();


            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            //IConfigurationRoot Configuration = builder.Build();


            //#region Logger

            //ILoggerFactory loggerFactory = new LoggerFactory();
            //loggerFactory.AddConsole(LogLevel.Warning);
            //loggerFactory.AddDebug();
            //loggerFactory.AddFile("Logs/DBOperation-{Date}.txt");

            //ILoggerFactory loggerFactory2 = new LoggerFactory();
            //loggerFactory2.AddConsole();
            //loggerFactory2.AddDebug();
            //loggerFactory2.AddFile("Logs/DBOperation2-{Date}.txt");

            //#endregion

            //ILogger logger = loggerFactory.CreateLogger("MongoDB_Operation");

            //logger.LogInformation("测试 info 级别日志信息");
            //logger.LogWarning("测试 warning 级别日志信息");
            //logger.LogError( "测试 error 级别日志信息");


            //ILogger logger2 = loggerFactory2.CreateLogger("Default");

            //logger2.LogInformation("测试2 info 级别日志信息");
            //logger2.LogWarning("测试2 warning 级别日志信息");
            //logger2.LogError("测试2 error 级别日志信息");

            //Console.Read();
            //return;
            try
            {
                //var DBLoad = new TianCheng.DAL.MongoDB.LoadAttribute();
                //var result = DBLoad.Load();
                //var options = DBLoad.LoadOptions();

                //DemoDAL dal = new DemoDAL();
                //DemoInfo info = new DemoInfo() { Name = Guid.NewGuid().ToString(), Date = DateTime.Now };
                //dal.Insert(info);

                //new Demo2DAL().Insert(new DemoInfo() { Name = Guid.NewGuid().ToString(), Date = DateTime.Now });
                //new Demo3DAL().Insert(new DemoInfo() { Name = Guid.NewGuid().ToString(), Date = DateTime.Now });

                DemoInfo info3 = new Demo3DAL().SearchById("5a0952fba1ef684b5810bdff1");
                Console.WriteLine(info3.Date.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            //DemoDAL dal = new DemoDAL();
            //dal.Test();

            Console.Read();
        }
    }
}

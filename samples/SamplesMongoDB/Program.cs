using SamplesMongoDB.DAL;
using SamplesMongoDB.Model;
using System;
using System.Collections.Generic;
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


            ContactClientDAL ccDal = new ContactClientDAL();

            List<ContactClientReport> result = ccDal.Report();

            Console.Read();
        }
    }
}

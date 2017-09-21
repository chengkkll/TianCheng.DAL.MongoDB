using MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TianCheng.DAL.MongoDB;
using TianCheng.Model;

namespace TianCheng.DAL.MongoDB
{
    public class DALCommon<T> : MongoSet<T>
        where T : MongoIdModel, new()
    {
        //private MongoRegister register = new MongoRegister();

        //private MongoSet<T> _ms = new MongoSet<T>();
        //public MongoSet<T> Set
        //{
        //    get { return _ms; }
        //}
    }
}

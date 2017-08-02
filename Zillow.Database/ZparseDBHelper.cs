using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;
using Zillow.Models;

namespace Zillow.Database
{
    public static class ZparseDBHelper
    {
        public static LiteDatabase GetZparseDb()
        {
            return new LiteDatabase(@"C:\ZparseDB\ZparseDB.db");
        }

        public static LiteCollection<ZillowEntityModel> GetZparseTable(LiteDatabase db)
        {
            // Get a collection (or create, if doesn't exist)
            return db.GetCollection<ZillowEntityModel>("ZillowEntities");
        }

        public static LiteDatabase GetZparseEmailMsgsDb()
        {
            return new LiteDatabase(@"C:\ZparseDB\ZillowEmailMsgsDB.db");
        }

        public static LiteCollection<ZillowEmailMsgModel> GetZparseEmailMsgsTable(LiteDatabase db)
        {
            // Get a collection (or create, if doesn't exist)
            return db.GetCollection<ZillowEmailMsgModel>("ZillowEmailMsgs");
        }
    }
}

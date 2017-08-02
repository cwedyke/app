using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zillow.Models
{
    public class ZillowEmailMsgModel
    {
        public LiteDB.ObjectId _id { get; set; }
        public string EmailMsg { get; set; }
    }
}

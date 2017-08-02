using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ZparseWeb.DTOs;
using Zillow.Models;
using LiteDB;


namespace ZparseWeb.Controllers
{
    public class ZillowController : ApiController
    {
        // POST: api/Zillow
        public void Post([FromBody]string value)
        {
            throw new NotImplementedException("void Post([FromBody]string value)");
            //var results = new List<ZillowDTO>();

            ////parse value coming from textbox to get all addresses
            //string val = Regex.Replace(value, @"\r\n?|\n", "");
            //string[] v = val.Split(new[] { "FOR SALE" }, StringSplitOptions.None);

            //var addressList = new List<StreetCityStModel>();
            //foreach (string s in v)
            //{
            //    string address = string.Empty;
            //    string askPrice = string.Empty;

            //    //grab everything after SQFT and stop at FL
            //    int sqft = s.ToLower().IndexOf("sqft");
            //    int state = s.ToLower().IndexOf(" ut");
            //    int cost = s.ToLower().IndexOf("$");

            //    if (sqft != -1 && state != -1 && cost != -1)
            //    {
            //        int length = (state - 1) - sqft;

            //        address = s.Substring(sqft + 4, length).Trim();
            //        askPrice = s.Substring(cost + 1, 8).Trim();
            //        askPrice = askPrice.Replace(",", string.Empty);

            //        //now that i have address I need to seperate street from city,st
            //        int endOfStreetComma = address.IndexOf(',');
            //        string street = address.Substring(0, endOfStreetComma);
            //        string citySt = address.Substring(endOfStreetComma + 1);

            //        addressList.Add(new StreetCityStModel() { street = street, citySt = citySt, askPrice = askPrice });
            //    }
            //}

            //foreach (StreetCityStModel s in addressList)
            //{
            //    var key = ZillowClientHelper.GetAvailableClientKey();
            //    ZillowClient client = new ZillowClient(key);


            //    ZillowClientHelper.IncrementKeyCount(key);


            //    searchresults result = new searchresults();
            //    try
            //    {
            //        result = client.GetSearchResults(s.street, s.citySt);
            //    }
            //    catch { }

            //    if (result.response != null)
            //    {
            //        foreach (SimpleProperty prop in result.response.results)
            //        {
            //            //var key1 = ZillowClientHelper.GetAvailableClientKey();
            //            //ZillowClient client1 = new ZillowClient(key1);

            //            //ZillowClientHelper.IncrementKeyCount(key1);
            //            //var propDets = client1.GetUpdatedPropertyDetails(prop.zpid);
            //            ZillowDTO dto;

            //            try
            //            {
            //                dto = new ZillowDTO()
            //                {
            //                    address = s.street + " " + s.citySt,
            //                    rentZestimate = Convert.ToInt32(prop.rentzestimate.amount.Value),
            //                    zestimate = Convert.ToInt32(prop.zestimate.amount.Value),
            //                    askingPrice = Convert.ToInt32(s.askPrice)
            //                };
            //            }
            //            catch //eat any converting errors and just create empty dto with address.
            //            {
            //                dto = new ZillowDTO()
            //                {
            //                    address = s.street + " " + s.citySt
            //                };
            //            }

            //            results.Add(dto);
            //        }
            //    }
            //}

            //return results;
        }

        // GET: api/Zillow
        public IEnumerable<ZillowDTO> Get()
        {
            List<ZillowDTO> list = new List<ZillowDTO>();

            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                var zillowEntities = table.Find(Query.EQ("isDeleted", false));

                foreach (var z in zillowEntities)
                    list.Add(ZillowDTO.ToZillowDTO(z));
            }

            return list;
        }

        [Route("api/zillow/getrecent")]
        public IEnumerable<ZillowDTO> GetRecent()
        {
            List<ZillowDTO> list = new List<ZillowDTO>();

            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                var zillowEntities = table.Find(Query.And(
                    Query.EQ("isDeleted", false),
                    Query.Or(Query.GTE("modifiedOn", DateTime.Now.AddDays(-5)), Query.GTE("createdOn", DateTime.Now.AddDays(-5)))
                ));

                foreach (var z in zillowEntities)
                    list.Add(ZillowDTO.ToZillowDTO(z));
            }

            return list;
        }

        [Route("api/zillow/getfavorites")]
        public IEnumerable<ZillowDTO> GetFavorites()
        {
            List<ZillowDTO> list = new List<ZillowDTO>();

            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                var zillowEntities = table.Find(Query.And(
                    Query.EQ("isDeleted", false), Query.EQ("isFavorite", true)
                ));

                foreach (var z in zillowEntities)
                    list.Add(ZillowDTO.ToZillowDTO(z));
            }

            return list;
        }

        // GET: api/Zillow/5
        public string Get(int id)
        {
            throw new NotImplementedException("string Get(int id)");
            //return "value";
        }

        // PUT: api/Zillow/5
        public void Put(int id)
        {
            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                var entity = table.FindOne(x => x.zpid == id);
                if (entity != null)
                {
                    //if (value.ToLower().Contains("isnew") && value.ToLower().Contains("false"))
                    //{
                    //    entity.isNew = false;
                    //}
                    //if (value.ToLower().Contains("isdeleted") && value.ToLower().Contains("true"))
                    //{
                    //    entity.isDeleted = true;
                    //    entity.deletedDate = DateTime.Now;
                    //}

                    table.Update(entity);
                }
            }
        }

        // DELETE: api/Zillow/5
        public void Delete(int id)
        {
            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                var entity = table.FindOne(x => x.zpid == id);
                if (entity != null)
                {
                    entity.isDeleted = true;
                    entity.deletedDate = DateTime.Now;

                    table.Update(entity);
                }
            }
        }

        [Route("api/zillow/setfavorite/{id}")]
        public void SetFavorite(int id)
        {
            using (var db = Zillow.Database.ZparseDBHelper.GetZparseDb())
            {
                var table = Zillow.Database.ZparseDBHelper.GetZparseTable(db);

                var entity = table.FindOne(x => x.zpid == id);
                if (entity != null)
                {
                    entity.isFavorite = true;
                    entity.modifiedOn = DateTime.Now;

                    table.Update(entity);
                }
            }
        }


    }
}

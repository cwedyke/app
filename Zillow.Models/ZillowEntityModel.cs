using System;

namespace Zillow.Models
{

    public class ZillowEntityModel
    {
        public LiteDB.ObjectId _id { get; set; }
        public uint zpid { get; set; }
        public string address { get; set; }

        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }

        public string urlHome { get; set; }
        public int rentZestimate { get; set; }
        public int zestimate { get; set; }
        public int askingPrice { get; set; }
        public bool isFavorite { get; set; }
        public DateTime? priceDropOn { get; set; }
        public DateTime createdOn { get; set; }
        public DateTime modifiedOn { get; set; }
        public bool isDeleted { get; set; }
        public DateTime deletedDate { get; set; }
        public decimal? chalculate //higher is better
        {
            get
            {
                if (askingPrice > 0 && rentZestimate > 0 && zestimate > 0)
                    return (zestimate / rentZestimate) - (askingPrice / rentZestimate);
                else
                    return null;
            }
        }
        public decimal? ralculate //higher is better
        {
            get
            {
                if (rentZestimate > 0 && askingPrice > 0)
                    return (askingPrice / rentZestimate) * -1;
                else if (rentZestimate > 0 && zestimate > 0)
                    return (zestimate / rentZestimate) * -1;
                else
                    return null;
            }
        }
        public decimal? zestimateMinusCostValue
        {
            get
            {
                if (askingPrice > 0 && zestimate > 0)
                    return zestimate - askingPrice;
                else
                    return null;
            }
        }

    }
}

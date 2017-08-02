using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Zillow.Models;

namespace ZparseWeb.DTOs
{
    public class ZillowDTO
    {
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
        public DateTime? priceDropOn { get; set; }
        public DateTime updatedOn { get; set; }
        public decimal? chalculate { get; set; } //higher is better
        public decimal? ralculate { get; set; } //higher is better
        public decimal? zestimateMinusCostValue { get; set; } //higher is better


        public static ZillowDTO ToZillowDTO(ZillowEntityModel entity)
        {
            return new ZillowDTO()
            {
                zpid = entity.zpid,
                address = entity.address,
                street = entity.street,
                city = entity.city,
                state = entity.state,
                zipcode = entity.zipcode,
                urlHome = entity.urlHome,
                rentZestimate = entity.rentZestimate,
                zestimate = entity.zestimate,
                askingPrice = entity.askingPrice,
                priceDropOn = entity.priceDropOn,
                updatedOn = (entity.modifiedOn < DateTime.Now.AddYears(-10)) ? entity.createdOn : entity.modifiedOn,
                chalculate = entity.chalculate,
                ralculate = entity.ralculate,
                zestimateMinusCostValue = entity.zestimateMinusCostValue
            };
        }

    }
}
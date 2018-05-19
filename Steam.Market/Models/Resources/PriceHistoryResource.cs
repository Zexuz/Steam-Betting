using System;
using System.Collections.Generic;

namespace Steam.Market.Models.Resources
{
    public class PriceHistoryResource
    {
        public bool        Success { get; set; }
        public List<Price> Prices  { get; set; }

        public PriceHistoryResource()
        {
            Prices = new List<Price>();
        }

        public class Price
        {
            public DateTime Time        { get; set; }
            public int    AmountSold  { get; set; }
            public double MedianPrice { get; set; }
        }
    }
}
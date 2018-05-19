using System.Collections.Generic;

namespace Betting.WebApi.Models
{
    public class PlayerInventory
    {
        public PlayerInventory()
        {
            Items = new List<Item>();
        }

        public List<Item> Items { get; set; }

        public class Item
        {
            public string  AssetId       { get; set; }
            public int     DescriptionId { get; set; }
            public string  Name          { get; set; }
            public string  ImgUrl        { get; set; }
            public decimal Value         { get; set; }
        }
    }
}
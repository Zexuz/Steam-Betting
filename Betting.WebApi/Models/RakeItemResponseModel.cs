using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.WebApi.Models
{
    public class RakeItemResponseModel
    {
        public List<RakeItemModel> Items { get; set; }

        public RakeItemResponseModel()
        {
            Items = new List<RakeItemModel>();
        }
    }

    public class RakeItemModel
    {
        public DatabaseModel.RakeItem RakeItem { get; set; }
        public DatabaseModel.ItemDescription Description { get; set; }
        public DatabaseModel.Bot Location { get; set; }
    }
}
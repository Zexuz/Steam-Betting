using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Repository.Extensions
{
    public static class ItemHelperExtension
    {
        public static List<Item> MapItemsToItemsDescription(this List<DatabaseModel.Item> rawItems,
                                                            List<DatabaseModel.ItemDescription> itemDescription)
        {
            var list = new List<Item>();

            foreach (var rawItem in rawItems)
            {
                foreach (var description in itemDescription)
                {
                    if (rawItem.DescriptionId != description.Id) continue;

                    var item = new Item
                    {
                        IconUrl = description.ImageUrl,
                        Name = description.Name,
                        Value = description.Value,
                        AssetId = rawItem.AssetId,
                        DescriptionId = rawItem.DescriptionId,
                        Id = rawItem.Id,
                        AppId = description.AppId,
                        Valid = description.Valid,
                        ReleaseTime = rawItem.ReleaseTime
                    };

                    list.Add(item);
                    break;
                }
            }

            return list;
        }
    }
}
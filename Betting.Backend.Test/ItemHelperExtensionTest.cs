using System;
using System.Collections.Generic;
using System.Linq;
using Betting.Models.Models;
using Betting.Repository.Extensions;
using Xunit;

namespace Betting.Backend.Test
{
    public class ItemHelperExtensionTest
    {
        [Fact]
        public void Test()
        {
            var rawItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 2, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 3, -1, -1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId5", 4, -1, -1, DateTimeOffset.Now),
            };

            var itemDescription = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc1", 10, "730", "2", "imageUrl1", true, 1),
                new DatabaseModel.ItemDescription("itemDesc2", 20, "730", "2", "imageUrl2", true, 2),
                new DatabaseModel.ItemDescription("itemDesc3", 30, "730", "2", "imageUrl3", true, 3),
                new DatabaseModel.ItemDescription("itemDesc4", 40, "730", "2", "imageUrl4", true, 4),
            };

            var items = rawItems.MapItemsToItemsDescription(itemDescription);


            Assert.Equal(5, items.Count);
            Assert.True(items.SingleOrDefault(item => item.AssetId == "assetId1").Name == "itemDesc1");
            Assert.True(items.SingleOrDefault(item => item.AssetId == "assetId2").Name == "itemDesc2");
            Assert.True(items.SingleOrDefault(item => item.AssetId == "assetId3").Name == "itemDesc1");
            Assert.True(items.SingleOrDefault(item => item.AssetId == "assetId4").Name == "itemDesc3");
            Assert.True(items.SingleOrDefault(item => item.AssetId == "assetId5").Name == "itemDesc4");
        }
    }
}
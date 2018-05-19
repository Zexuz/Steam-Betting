using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class ItemQueries : IItemQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.Item> items)
        {
            var listOfSqlValues = new List<string>();
            var dict            = new Dictionary<string, object>();
            for (var index = 0; index < items.Count; index++)
            {
                listOfSqlValues.Add($"(@assetId{index} ,@descriptionId{index}, @locationId{index},@ownerId{index}, @releaseTime{index})");
                dict.Add($"@assetId{index}", items[index].AssetId);
                dict.Add($"@descriptionId{index}", items[index].DescriptionId);
                dict.Add($"@locationId{index}", items[index].LocationId);
                dict.Add($"@ownerId{index}", items[index].OwnerId);
                dict.Add($"@releaseTime{index}", items[index].ReleaseTime);
            }

            return new SqlQuery($"INSERT INTO [Item] (AssetId, DescriptionId, LocationId, OwnerId, ReleaseTime) VALUES {string.Join(",", listOfSqlValues)};",
                dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.Item item)
        {
            var dict = new Dictionary<string, object>
            {
                {"@assetId", item.AssetId},
                {"@descId", item.DescriptionId},
                {"@locId", item.LocationId},
                {"@ownerId", item.OwnerId},
                {"@releaseTime", item.ReleaseTime}
            };

            var queryStr =
                "INSERT INTO [Item] (AssetId, DescriptionId, LocationId, OwnerId, ReleaseTime) OUTPUT INSERTED.Id VALUES (@assetId ,@descId, @locId,@ownerId,@releaseTime)";
            return new SqlQuery(queryStr, dict);
        }

        public SqlQuery Insert(DatabaseModel.Item item)
        {
            return InsertRange(new List<DatabaseModel.Item> {item});
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM [Item]", null);
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [Item] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetItemsFromId(List<int> ids)
        {
            var strArr = new List<string>();
            var dict   = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@ids{index}");
                dict.Add($"@ids{index}", ids[index]);
            }

            var str = string.Join(",", strArr);
            return new SqlQuery($"SELECT * FROM [ITEM] WHERE Id IN ({str})", dict);
        }

        public SqlQuery GetFromAssetId(AssetAndDescriptionId info)
        {
            return new SqlQuery("SELECT * FROM [Item] WHERE AssetId =@assetId AND DescriptionId =@descId", new Dictionary<string, object>
            {
                {"@assetId", info.AssetId},
                {"@descId", info.DescriptionId}
            });
        }

        public SqlQuery GetItemsFromAssetId(List<AssetAndDescriptionId> info)
        {
            var assetsIds = new List<string>();
            var descIds   = new List<string>();
            var dict      = new Dictionary<string, object>();
            for (var index = 0; index < info.Count; index++)
            {
                assetsIds.Add($"@assetId{index}");
                descIds.Add($"@descId{index}");
                dict.Add($"@assetId{index}", info[index].AssetId);
                dict.Add($"@descId{index}", info[index].DescriptionId);
            }

            var assetStr = string.Join(",", assetsIds);
            var descStr  = string.Join(",", descIds);
            return new SqlQuery($"SELECT * FROM [ITEM] WHERE AssetId IN ({assetStr}) AND DescriptionId IN ({descStr}) ", dict);
        }

        public SqlQuery GetItemsThatUserOwns(int userId)
        {
            return new SqlQuery("SELECT * FROM [ITEM] WHERE OwnerId =@ownerId ", new Dictionary<string, object> {{"@ownerId", userId}});
        }

        public SqlQuery DeleteRange(List<AssetAndDescriptionId> items)
        {
            var assetsIds = new List<string>();
            var descIds   = new List<string>();
            var dict      = new Dictionary<string, object>();
            for (var index = 0; index < items.Count; index++)
            {
                assetsIds.Add($"@assetId{index}");
                descIds.Add($"@descId{index}");
                dict.Add($"@assetId{index}", items[index].AssetId);
                dict.Add($"@descId{index}", items[index].DescriptionId);
            }

            var assetStr = string.Join(",", assetsIds);
            var descStr  = string.Join(",", descIds);
            return new SqlQuery($"DELETE FROM [ITEM] WHERE AssetId IN ({assetStr}) AND DescriptionId IN ({descStr})", dict);
        }

        public SqlQuery DeleteSingle(AssetAndDescriptionId item)
        {
            return new SqlQuery($"DELETE FROM [ITEM] WHERE AssetId =@assetId AND DescriptionId =@descId", new Dictionary<string, object>
            {
                {"@assetId", item.AssetId},
                {"@descId", item.DescriptionId}
            });
        }

        public SqlQuery ChangeOwner(List<int> itemIds, int newUserId)
        {
            var ids  = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < itemIds.Count; index++)
            {
                ids.Add($"@id{index}");
                dict.Add($"@id{index}", itemIds[index]);
            }

            dict.Add("@newOwnerId", newUserId);

            var idsStr = string.Join(",", ids);
            return new SqlQuery($"UPDATE [Item] SET OwnerId = @newOwnerId WHERE Id IN({idsStr});", dict);
        }

        public SqlQuery DeleteRange(List<int> idsToDelete)
        {
            var ids  = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < idsToDelete.Count; index++)
            {
                ids.Add($"@id{index}");
                dict.Add($"@id{index}", idsToDelete[index]);
            }

            var idStr = string.Join(",", ids);
            return new SqlQuery($"DELETE FROM [ITEM] WHERE Id IN ({idStr})", dict);
        }

        public SqlQuery ChangeOwner(List<AssetAndDescriptionId> items, int newUserId)
        {
            var assetsIds = new List<string>();
            var descIds   = new List<string>();
            var dict      = new Dictionary<string, object>();
            for (var index = 0; index < items.Count; index++)
            {
                assetsIds.Add($"@assetId{index}");
                descIds.Add($"@descId{index}");
                dict.Add($"@assetId{index}", items[index].AssetId);
                dict.Add($"@descId{index}", items[index].DescriptionId);
            }

            dict.Add("@newOwnerId", newUserId);

            var assetStr = string.Join(",", assetsIds);
            var descStr  = string.Join(",", descIds);
            return new SqlQuery($"UPDATE [ITEM] SET OwnerId = @newOwnerId WHERE AssetId IN ({assetStr}) AND DescriptionId IN ({descStr})", dict);
        }


        public SqlQuery ChangeOwner(int itemId, int newUserId)
        {
            return new SqlQuery("UPDATE [Item] SET OwnerId = @newOwnerId WHERE Id = @id;", new Dictionary<string, object>
            {
                {"@id", itemId},
                {"@newOwnerId", newUserId}
            });
        }
    }
}
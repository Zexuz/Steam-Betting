using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class RakeItemQueries : IRakeItemQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.RakeItem> items)
        {
            var listOfSqlValues = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < items.Count; index++)
            {
                listOfSqlValues.Add(
                    $"(@assetId{index} ,@descriptionId{index}, @locationId{index},@matchId{index}, @created{index},@isSold{index}, @gameModeId{index})");
                dict.Add($"@assetId{index}", items[index].AssetId);
                dict.Add($"@descriptionId{index}", items[index].DescriptionId);
                dict.Add($"@locationId{index}", items[index].LocationId);
                dict.Add($"@created{index}", items[index].Received);
                dict.Add($"@matchId{index}", items[index].MatchId);
                dict.Add($"@isSold{index}", items[index].IsSold);
                dict.Add($"@gameModeId{index}", items[index].GameModeId);
            }

            return new SqlQuery(
                $"INSERT INTO [RakeItem] (AssetId, DescriptionId, LocationId,MatchId, Received, IsSold, GameModeId) VALUES {string.Join(",", listOfSqlValues)};",
                dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.RakeItem rakeItem)
        {
            var dict = new Dictionary<string, object>
            {
                {"@assetId", rakeItem.AssetId},
                {"@descId", rakeItem.DescriptionId},
                {"@locId", rakeItem.LocationId},
                {"@created", rakeItem.Received},
                {"@matchId", rakeItem.MatchId},
                {"@isSold", rakeItem.IsSold},
                {"@gameModeId", rakeItem.GameModeId},
            };

            var queryStr =
                "INSERT INTO [RakeItem] (AssetId, DescriptionId, LocationId, MatchId,Received, IsSold, GameModeId) OUTPUT INSERTED.Id VALUES (@assetId ,@descId, @locId,@matchId, @created,@isSold,@gameModeId)";
            return new SqlQuery(queryStr, dict);
        }

        public SqlQuery Insert(DatabaseModel.RakeItem rakeItem)
        {
            return InsertRange(new List<DatabaseModel.RakeItem> {rakeItem});
        }


        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM [RakeItem]", null);
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [RakeItem] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetItemsFromId(List<int> ids)
        {
            var strArr = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@ids{index}");
                dict.Add($"@ids{index}", ids[index]);
            }

            var str = string.Join(",", strArr);
            return new SqlQuery($"SELECT * FROM [RakeItem] WHERE Id IN ({str})", dict);
        }

        public SqlQuery GetFromAssetId(AssetAndDescriptionId info)
        {
            return new SqlQuery("SELECT * FROM [RakeItem] WHERE AssetId =@assetId AND DescriptionId =@descId", new Dictionary<string, object>
            {
                {"@assetId", info.AssetId},
                {"@descId", info.DescriptionId}
            });
        }

        public SqlQuery GetFromMatchId(int matchId)
        {
            return new SqlQuery("SELECT * FROM [RakeItem] WHERE MatchId =@matchId", new Dictionary<string, object>
            {
                {"@matchId", matchId}
            });
        }

        public SqlQuery GetItemsFromAssetId(List<AssetAndDescriptionId> info)
        {
            var assetsIds = new List<string>();
            var descIds = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < info.Count; index++)
            {
                assetsIds.Add($"@assetId{index}");
                descIds.Add($"@descId{index}");
                dict.Add($"@assetId{index}", info[index].AssetId);
                dict.Add($"@descId{index}", info[index].DescriptionId);
            }

            var assetStr = string.Join(",", assetsIds);
            var descStr = string.Join(",", descIds);
            return new SqlQuery($"SELECT * FROM [RakeItem] WHERE AssetId IN ({assetStr}) AND DescriptionId IN ({descStr}) ", dict);
        }


        public SqlQuery SetAsSold(List<string> info)
        {
            var assetsIds = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < info.Count; index++)
            {
                assetsIds.Add($"@assetId{index}");
                dict.Add($"@assetId{index}", info[index]);
            }

            var assetStr = string.Join(",", assetsIds);
            return new SqlQuery($"UPDATE [RakeItem] SET IsSold = 1 WHERE AssetId IN ({assetStr})", dict);
        }

        public SqlQuery GetAllWithSoldStatus(bool status)
        {
            return new SqlQuery("SELECT * FROM [RakeItem] WHERE IsSold = @status", new Dictionary<string, object>
            {
                {"@status", status}
            });
        }

//        public SqlQuery DeleteRange(List<AssetAndDescriptionId> items)
//        {
//            var assetsIds = new List<string>();
//            var descIds   = new List<string>();
//            var dict      = new Dictionary<string, object>();
//            for (var index = 0; index < items.Count; index++)
//            {
//                assetsIds.Add($"@assetId{index}");
//                descIds.Add($"@descId{index}");
//                dict.Add($"@assetId{index}", items[index].AssetId);
//                dict.Add($"@descId{index}", items[index].DescriptionId);
//            }
//
//            var assetStr = string.Join(",", assetsIds);
//            var descStr  = string.Join(",", descIds);
//            return new SqlQuery($"DELETE FROM [RakeItem] WHERE AssetId IN ({assetStr}) AND DescriptionId IN ({descStr})", dict);
//        }
//
//        public SqlQuery DeleteSingle(AssetAndDescriptionId item)
//        {
//            return new SqlQuery($"DELETE FROM [RakeItem] WHERE AssetId =@assetId AND DescriptionId =@descId", new Dictionary<string, object>
//            {
//                {"@assetId", item.AssetId},
//                {"@descId", item.DescriptionId}
//            });
//        }
    }
}
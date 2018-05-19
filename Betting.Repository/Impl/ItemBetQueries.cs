using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class ItemBetQueries : IItemBetQueries
    {
        public SqlQuery InsertRangeAsync(List<DatabaseModel.ItemBetted> itemBets)
        {
            var listOfSqlValues = new List<string>();
            var dict            = new Dictionary<string, object>();
            for (var index = 0; index < itemBets.Count; index++)
            {
                listOfSqlValues.Add($"(@betId{index} ,@descId{index},@assetId{index}, @value{index})");
                dict.Add($"@betId{index}", itemBets[index].BetId);
                dict.Add($"@descId{index}", itemBets[index].DescriptionId);
                dict.Add($"@assetId{index}", itemBets[index].AssetId);
                dict.Add($"@value{index}", itemBets[index].Value);
            }

            return new SqlQuery($"INSERT INTO [ItemBetted] (BetId,DescriptionId,AssetId,Value) VALUES {string.Join(",", listOfSqlValues)};", dict);
        }

        public SqlQuery InsertAsync(DatabaseModel.ItemBetted itemBetted)
        {
            return InsertRangeAsync(new List<DatabaseModel.ItemBetted> {itemBetted});
        }

        public SqlQuery GetItemBetsOnBet(int betId)
        {
            return new SqlQuery("SELECT * FROM [ItemBetted] WHERE BetId =@betId ", new Dictionary<string, object>
            {
                {"@betId", betId},
            });
        }

        public SqlQuery GetItemBetsOnBets(List<int> ids)
        {
            var strArr = new List<string>();
            var dict   = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@id{index}");
                dict.Add($"@id{index}", ids[index]);
            }

            var str = string.Join(",", strArr);
            return new SqlQuery($"SELECT * FROM [ItemBetted] WHERE BetId IN ({str})", dict);
        }
    }
}
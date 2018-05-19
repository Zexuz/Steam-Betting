using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class ItemInOfferTransactionQueries : IItemInOfferTransactionQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.ItemInOfferTransaction> itemInOfferTransactions)
        {
            var listOfSqlValues = new List<string>();
            var dict            = new Dictionary<string, object>();
            for (var index = 0; index < itemInOfferTransactions.Count; index++)
            {
                listOfSqlValues.Add($"(@offerTransId{index} ,@itemDescId{index},@assetId{index}, @value{index})");
                dict.Add($"@offerTransId{index}", itemInOfferTransactions[index].OfferTransactionId);
                dict.Add($"@itemDescId{index}", itemInOfferTransactions[index].ItemDescriptionId);
                dict.Add($"@assetId{index}", itemInOfferTransactions[index].AssetId);
                dict.Add($"@value{index}", itemInOfferTransactions[index].Value);
            }

            return new SqlQuery(
                $"INSERT INTO [ItemInOfferTransaction] (OfferTransactionId, ItemDescriptionId, AssetId, Value) VALUES {string.Join(",", listOfSqlValues)};",
                dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.ItemInOfferTransaction itemInOfferTransaction)
        {
            var dict = new Dictionary<string, object>
            {
                {"@offerTransId", itemInOfferTransaction.OfferTransactionId},
                {"@itemDescId", itemInOfferTransaction.ItemDescriptionId},
                {"@assetId", itemInOfferTransaction.AssetId},
                {"@value", itemInOfferTransaction.Value}
            };

            return new SqlQuery(
                "INSERT INTO [ItemInOfferTransaction] (OfferTransactionId, ItemDescriptionId,AssetId, Value) OUTPUT INSERTED.Id VALUES (@offerTransId,@itemDescId,@assetId,@value);",
                dict);
        }

        public SqlQuery Insert(DatabaseModel.ItemInOfferTransaction itemInOfferTransaction)
        {
            return InsertRange(new List<DatabaseModel.ItemInOfferTransaction> {itemInOfferTransaction});
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM [ItemInOfferTransaction]", null);
        }

        public SqlQuery GetFromTransactionsIds(List<int> ids)
        {
            var strArr = new List<string>();
            var dict   = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@id{index}");
                dict.Add($"@id{index}", ids[index]);
            }

            var str = string.Join(",", strArr);
            return new SqlQuery($"SELECT * FROM [ItemInOfferTransaction] WHERE OfferTransactionId IN ({str})", dict);
        }


        public SqlQuery GetFromIds(List<int> ids)
        {
            var strArr = new List<string>();
            var dict   = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@id{index}");
                dict.Add($"@id{index}", ids[index]);
            }

            var str = string.Join(",", strArr);
            return new SqlQuery($"SELECT * FROM [ItemInOfferTransaction] WHERE Id IN ({str})", dict);
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [ItemInOfferTransaction] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetFromTransactionId(int id)
        {
            return new SqlQuery("SELECT * FROM [ItemInOfferTransaction] WHERE OfferTransactionId =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery Remove(int offerTransactionId)
        {
            return new SqlQuery("DELETE FROM [ItemInOfferTransaction] WHERE OfferTransactionId =@id",
                new Dictionary<string, object> {{"@id", offerTransactionId}});
        }

        public SqlQuery GetItemCountFromOfferId(int offerId)
        {
            return new SqlQuery("SELECT Count(Id) FROM [ItemInOfferTransaction] WHERE OfferTransactionId =@id", new Dictionary<string, object>
            {
                {"@id", offerId}
            });
        }
    }
}
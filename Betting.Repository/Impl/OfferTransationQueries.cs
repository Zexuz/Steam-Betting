using System;
using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class OfferTransationQueries : IOfferTransationQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.OfferTransaction> offerTransactions)
        {
            var listOfSqlValues = new List<string>();
            var dict            = new Dictionary<string, object>();
            for (var index = 0; index < offerTransactions.Count; index++)
            {
                listOfSqlValues.Add($"(@userId{index} ,@botId{index}, @totalValue{index}, @isDeposit{index},@steamOfferId{index}, @accepted{index})");
                dict.Add($"@userId{index}", offerTransactions[index].UserId);
                dict.Add($"@botId{index}", offerTransactions[index].BotId);
                dict.Add($"@totalValue{index}", offerTransactions[index].TotalValue);
                dict.Add($"@isDeposit{index}", offerTransactions[index].IsDeposit);
                dict.Add($"@steamOfferId{index}", offerTransactions[index].SteamOfferId);
                dict.Add($"@accepted{index}", offerTransactions[index].Accepted);
            }

            return new SqlQuery(
                $"INSERT INTO [OfferTransaction] (UserId, BotId, TotalValue, isDeposit,SteamOfferId, Accepted) VALUES {string.Join(",", listOfSqlValues)};",
                dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.OfferTransaction offerTransactions)
        {
            var dict = new Dictionary<string, object>
            {
                {"@userId", offerTransactions.UserId},
                {"@botId", offerTransactions.BotId},
                {"@totalValue", offerTransactions.TotalValue},
                {"@isDeposit", offerTransactions.IsDeposit},
                {"@steamOfferId", offerTransactions.SteamOfferId},
                {"@accepted", offerTransactions.Accepted}
            };

            return new SqlQuery(
                "INSERT INTO [OfferTransaction] (UserId, BotId, TotalValue, isDeposit,SteamOfferId, Accepted) OUTPUT INSERTED.Id  " +
                "VALUES (@userId ,@botId, @totalValue, @isDeposit,@steamOfferId, @accepted);",
                dict);
        }

        public SqlQuery Insert(DatabaseModel.OfferTransaction itemDescription)
        {
            return InsertRange(new List<DatabaseModel.OfferTransaction> {itemDescription});
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM [OfferTransaction]", null);
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
            return new SqlQuery($"SELECT * FROM [OfferTransaction] WHERE Id IN ({str})", dict);
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [OfferTransaction] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetFromSteamOfferid(string id)
        {
            return new SqlQuery("SELECT * FROM [OfferTransaction] WHERE SteamOfferId =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetFromUserId(int userId)
        {
            return new SqlQuery("SELECT * FROM [OfferTransaction] WHERE UserId =@id", new Dictionary<string, object> {{"@id", userId}});
        }

        public SqlQuery GetFromUserId(int userId, int limit, int? from)
        {
            if(from.HasValue)
            return new SqlQuery("SELECT TOP(@top) * FROM [OfferTransaction] WHERE UserId =@id AND Id < @from ORDER BY Id DESC",
                new Dictionary<string, object>
                {
                    {"@id", userId},
                    {"@top", limit},
                    {"@from", from.Value}
                });
            
            return new SqlQuery("SELECT TOP(@top) * FROM [OfferTransaction] WHERE UserId =@id ORDER BY Id DESC",
                new Dictionary<string, object>
                {
                    {"@id", userId},
                    {"@top", limit}
                });
            
        }

        public SqlQuery GetActiveFromUserId(int userId)
        {
            return new SqlQuery("SELECT * FROM [OfferTransaction] WHERE UserId =@id AND Accepted is null",
                new Dictionary<string, object> {{"@id", userId}});
        }

        public SqlQuery UpdateSteamOfferId(int offerTransactionId, string steamOfferId)
        {
            var dict = new Dictionary<string, object>
            {
                {"@steamOfferId", steamOfferId},
                {"@id", offerTransactionId}
            };
            return new SqlQuery("UPDATE OfferTransaction SET SteamOfferId =@steamOfferId WHERE Id = @id", dict);
        }

        public SqlQuery Remove(int offerTransactionId)
        {
            return new SqlQuery("DELETE FROM [OfferTransaction] WHERE Id =@id", new Dictionary<string, object> {{"@id", offerTransactionId}});
        }

        public SqlQuery AddAcceptedTimeOnOfferWithOfferId(DateTime time, int id)
        {
            return new SqlQuery("UPDATE [OfferTransaction] SET Accepted = @time WHERE Id =@id", new Dictionary<string, object>
            {
                {"@id", id},
                {"@time", time}
            });
        }
    }
}
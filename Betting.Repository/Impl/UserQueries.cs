using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class UserQueries : IUserQueries
    {
        public SqlQuery InsertUsersAsync(List<DatabaseModel.User> usersToInsert)
        {
            var listOfSqlValues = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < usersToInsert.Count; index++)
            {
                listOfSqlValues.Add(
                    $"(@steamId{index}, @name{index}, @imageUrl{index}, @tradeLink{index}, @crated{index},@lastActive{index},@quote{index},@SuspendedFromQuote{index})");
                dict.Add($"@steamId{index}", usersToInsert[index].SteamId);
                dict.Add($"@name{index}", usersToInsert[index].Name);
                dict.Add($"@imageUrl{index}", usersToInsert[index].ImageUrl);
                dict.Add($"@creatd{index}", usersToInsert[index].Created);
                dict.Add($"@lastActive{index}", usersToInsert[index].LastActive);
                dict.Add($"@quote{index}", usersToInsert[index].Quote);
                dict.Add($"@SuspendedFromQuote{index}", usersToInsert[index].SuspendedFromQuote);
            }


            return new SqlQuery(
                $"INSERT INTO [User] (SteamId, Name, ImageUrl, TradeLink, Created, LastActive, Quote, SuspendedFromQuote) VALUES {string.Join(",", listOfSqlValues)};"
                , dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.User user)
        {
            var dict = new Dictionary<string, object>
            {
                {"@steamId", user.SteamId},
                {"@name", user.Name},
                {"@imgUrl", user.ImageUrl},
                {"@tLink", user.TradeLink},
                {"@cred", user.Created},
                {"@lastA", user.LastActive},
                {"@quote", user.Quote},
                {"@squote", user.SuspendedFromQuote},
            };

            return new SqlQuery(
                "INSERT INTO [User] (SteamId, Name, ImageUrl, TradeLink, Created, LastActive, Quote, SuspendedFromQuote) OUTPUT INSERTED.Id VALUES(@steamId, @name, @imgUrl, @tLink, @cred, @lastA, @quote,@squote);",
                dict);
        }

        public SqlQuery GetAllUsersAsync()
        {
            return new SqlQuery("SELECT * FROM [USER]", null);
        }

        public SqlQuery GetUserWithSteamId(string steamId)
        {
            return new SqlQuery("SELECT * FROM [USER] WHERE SteamId =@steamId", new Dictionary<string, object> {{"@steamId", steamId}});
        }

        public SqlQuery InsertAsync(DatabaseModel.User user)
        {
            var list = new List<DatabaseModel.User> {user};
            return InsertUsersAsync(list);
        }

        public SqlQuery UpdateUsersName(string steamId, string newName)
        {
            return new SqlQuery("UPDATE [User] SET Name = @name WHERE SteamId = @steamId;", new Dictionary<string, object>
            {
                {"@steamId", steamId},
                {"@name", newName}
            });
        }

        public SqlQuery UpdateUsersTradeLink(string steamId, string newTradelink)
        {
            return new SqlQuery("UPDATE [User] SET TradeLink = @tradeLink WHERE SteamId = @steamId;", new Dictionary<string, object>
            {
                {"@steamId", steamId},
                {"@tradeLink", newTradelink}
            });
        }

        public SqlQuery UpdateUsersImage(string steamId, string newImage)
        {
            return new SqlQuery("UPDATE [User] SET ImageUrl = @imageUrl WHERE SteamId = @steamId;", new Dictionary<string, object>
            {
                {"@steamId", steamId},
                {"@imageUrl", newImage}
            });
        }

        public SqlQuery UpdateUsersNameAndImage(string steamId, string newName, string newImage)
        {
            return new SqlQuery("UPDATE [User] SET ImageUrl = @imageUrl, Name = @name WHERE SteamId = @steamId;", new Dictionary<string, object>
            {
                {"@steamId", steamId},
                {"@imageUrl", newImage},
                {"@name", newName}
            });
        }

        public SqlQuery Delete(string steamId)
        {
            return new SqlQuery("DELETE FROM [USER] WHERE SteamId =@steamId;", new Dictionary<string, object> {{"@steamId", steamId}});
        }

        public SqlQuery GetUserWithId(int id)
        {
            return new SqlQuery("SELECT * FROM [USER] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
        }

        public SqlQuery GetUsersWithId(List<int> ids)
        {
            var strArr = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < ids.Count; index++)
            {
                strArr.Add($"@id{index}");
                dict.Add($"@id{index}", ids[index]);
            }

            var str = string.Join(",", strArr);

            return new SqlQuery($"SELECT * FROM [USER] WHERE Id IN ({str})", dict);
        }

        public SqlQuery UpdateUsersQuote(string steamId, string quote)
        {
            return new SqlQuery("UPDATE [User] SET Quote  = @quote WHERE SteamId = @steamId;", new Dictionary<string, object>
            {
                {"@steamId", steamId},
                {"@quote", quote}
            });
        }

        public SqlQuery GetUsersWithSteamIds(List<string> steamId)
        {
            var strArr = new List<string>();
            var dict = new Dictionary<string, object>();
            for (var index = 0; index < steamId.Count; index++)
            {
                strArr.Add($"@id{index}");
                dict.Add($"@id{index}", steamId[index]);
            }

            var str = string.Join(",", strArr);

            return new SqlQuery($"SELECT * FROM [USER] WHERE SteamId IN ({str})", dict);
        }
    }
}
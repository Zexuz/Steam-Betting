using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class BotQueries : IBotQueries
    {
        public SqlQuery InsertRange(List<DatabaseModel.Bot> bots)
        {
            var listOfSqlValues = new List<string>();
            var dict            = new Dictionary<string, object>();
            for (var index = 0; index < bots.Count; index++)
            {
                listOfSqlValues.Add($"(@steamId{index} ,@name{index})");
                dict.Add($"@steamId{index}", bots[index].SteamId);
                dict.Add($"@name{index}", bots[index].Name);
            }

            return new SqlQuery($"INSERT INTO [Bot] (SteamId, Name) VALUES {string.Join(",", listOfSqlValues)};", dict);
        }

        public SqlQuery InsertReturnsId(DatabaseModel.Bot bot)
        {
            var dict = new Dictionary<string, object>
            {
                {"@steamId", bot.SteamId},
                {"@name", bot.Name}
            };

            return new SqlQuery("INSERT INTO [Bot] (SteamId, Name) OUTPUT INSERTED.Id VALUES (@steamId ,@name);", dict);
        }

        public SqlQuery GetAll()
        {
            var readQuery = new SqlQuery("SELECT * FROM [Bot]", null);
            return readQuery;
        }

        public SqlQuery GetFromId(int id)
        {
            return new SqlQuery("SELECT * FROM [Bot] WHERE Id =@id", new Dictionary<string, object> {{"@id", id}});
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
            return new SqlQuery($"SELECT * FROM [Bot] WHERE Id IN ({str})", dict);
        }

        public SqlQuery GetFromSteamId(string steamId)
        {
            return new SqlQuery("SELECT * FROM [Bot] WHERE SteamId =@steamId", new Dictionary<string, object> {{"@steamId", steamId}});
        }

        public SqlQuery Insert(DatabaseModel.Bot bot)
        {
            return InsertRange(new List<DatabaseModel.Bot> {bot});
        }

        public SqlQuery Delete(string steamId)
        {
            return new SqlQuery("DELETE FROM [Bot] WHERE SteamId =@steamId;", new Dictionary<string, object> {{"@name", steamId}});
        }
    }
}
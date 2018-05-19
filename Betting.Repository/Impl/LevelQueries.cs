using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class LevelQueries : ILevelQueries
    {
        public SqlQuery GetLevelFromId(int id)
        {
            return new SqlQuery("SELECT * FROM Level WHERE Id = @id", new Dictionary<string, object>
            {
                {"@id", id}
            });
        }

        public SqlQuery CreteNewLevel(DatabaseModel.Level level)
        {
            return new SqlQuery("INSERT INTO Level (Name, Chat, Ticket, Admin) OUTPUT INSERTED.Id VALUES (@name,@chat,@ticket,@admin);",
                new Dictionary<string, object>
                {
                    {"@name", level.Name},
                    {"@chat", level.Chat},
                    {"@ticket", level.Ticket},
                    {"@admin", level.Admin}
                });
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM Level ", new Dictionary<string, object>());
        }

        public SqlQuery Remove(int id)
        {
            return new SqlQuery("DELETE FROM Level WHERE Id = @id", new Dictionary<string, object>
            {
                {"@id", id}
            });
        }
    }
}
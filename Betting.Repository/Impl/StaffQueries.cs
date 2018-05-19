using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class StaffQueries : IStaffQueries
    {
        public SqlQuery GetStaffFromSteamId(string steamId)
        {
            return new SqlQuery("SELECT * FROM Staff WHERE SteamId = @steamId", new Dictionary<string, object>
            {
                {"@steamId", steamId}
            });
        }

        public SqlQuery AddStaff(string steamId, int levelId)
        {
            return new SqlQuery("INSERT INTO Staff (SteamId,Level) OUTPUT INSERTED.Id VALUES (@steamId,@level);", new Dictionary<string, object>
            {
                {"@steamId", steamId},
                {"@level", levelId}
            });
        }

        public SqlQuery GetAll()
        {
            return new SqlQuery("SELECT * FROM Staff ", new Dictionary<string, object>());
        }

        public SqlQuery Remove(int id)
        {
            return new SqlQuery("DELETE FROM Staff WHERE Id = @id", new Dictionary<string, object>
            {
                {"@id", id}
            });
        }
    }
}
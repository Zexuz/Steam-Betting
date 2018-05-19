using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class SettingRepoService : ISettingRepoService
    {
        private IDatabaseConnection _databaseConnection;

        public SettingRepoService(IDatabaseConnectionFactory databaseConnectionFactory)
        {
            _databaseConnection = databaseConnectionFactory.GetDatabaseConnection(Database.Settings);
        }

        public async Task<DatabaseModel.Settings> GetSettingsAsync()
        {
            var query = new SqlQuery("SELECT * FROM [Settings] WHERE Id = 1", new Dictionary<string, object>());
            var res   = await _databaseConnection.ExecuteSqlQueryAsync(query);
            return await res.GetSingleAsync<DatabaseModel.Settings>();
        }

        public async Task SetSettingsAsync(DatabaseModel.Settings settings)
        {
            var dict = new Dictionary<string, object>
            {
                {"@InventoryLimit", settings.InventoryLimit},
                {"@ItemValueLimit", settings.ItemValueLimit},
                {"@SteamInventoryCacheTimerInSec", settings.SteamInventoryCacheTimerInSec},
                {"@UpdatedPricingTime", settings.UpdatedPricingTime},
                {"@NrOfLatestChatMessages", settings.NrOfLatestChatMessages},
            };

            if (await HasAlreadyAdded())
            {
                var sqlQueryUpdate = new SqlQuery("UPDATE [Settings] SET "                                           +
                                                  "InventoryLimit = @InventoryLimit, "                               +
                                                  "ItemValueLimit = @ItemValueLimit, "                               +
                                                  "SteamInventoryCacheTimerInSec = @SteamInventoryCacheTimerInSec, " +
                                                  "UpdatedPricingTime = @UpdatedPricingTime, "                       +
                                                  "NrOfLatestChatMessages= @NrOfLatestChatMessages "                 +
                                                  "WHERE Id = 1", dict);

                await _databaseConnection.ExecuteNonQueryAsync(sqlQueryUpdate);
                return;
            }

            //insert
            var sqlQuery = new SqlQuery(
                "INSERT INTO [Settings]  (InventoryLimit, ItemValueLimit, SteamInventoryCacheTimerInSec, UpdatedPricingTime, NrOfLatestChatMessages) " +
                "VALUES                 (@InventoryLimit,@ItemValueLimit,@SteamInventoryCacheTimerInSec,@UpdatedPricingTime,@NrOfLatestChatMessages);",
                dict);


            await _databaseConnection.ExecuteNonQueryAsync(sqlQuery);
        }

        private async Task<bool> HasAlreadyAdded()
        {
            var res  = await _databaseConnection.ExecuteSqlQueryAsync(new SqlQuery("select * from [Settings]", null));
            var data = await res.GetListAsync<DatabaseModel.Settings>();
            return data.Count > 0;
        }
    }
}
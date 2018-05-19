using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Dapper;
using Dapper.Contrib.Extensions;

namespace Betting.Repository.Services.Interfaces
{
    public interface IJackpotSettingRepo
    {
        Task<DatabaseModel.JackpotSetting> InsertAsync(DatabaseModel.JackpotSetting setting);
        Task<DatabaseModel.JackpotSetting> Find(int i);
        Task<DatabaseModel.JackpotSetting> InsertAsync(DatabaseModel.JackpotSetting jackpotSetting, ITransactionWrapper transaction);
        Task                               RemoveAsync(DatabaseModel.JackpotSetting setting);
    }

    public class JackpotSettingRepo : DapperRepoBase, IJackpotSettingRepo
    {
        public JackpotSettingRepo(IDatabaseConnectionFactory factory) : base(factory)
        {
        }

        public async Task<DatabaseModel.JackpotSetting> InsertAsync(DatabaseModel.JackpotSetting setting)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                await cn.InsertAsync<DatabaseModel.JackpotSetting>(setting);
                cn.Close();
                return setting;
            }
        }

        public async Task<DatabaseModel.JackpotSetting> Find(int i)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                var res = await cn.GetAsync<DatabaseModel.JackpotSetting>(i);
                cn.Close();
                return res;
            }
        }

        public async Task<DatabaseModel.JackpotSetting> InsertAsync(DatabaseModel.JackpotSetting setting, ITransactionWrapper transactionWrapper)
        {
            var cn = transactionWrapper.SqlConnection;
            var transaction = transactionWrapper.Transaction;

            var insertedId = await cn.InsertAsync(setting, transaction);
            setting.Id = insertedId;
            return setting;
        }

        public async Task RemoveAsync(DatabaseModel.JackpotSetting setting)
        {
            using (var cn = DatabaseConnection.GetNewOpenConnection())
            {
                await cn.ExecuteAsync("DELETE FROM JackpotSetting WHERE Id = @id", new {Id = setting.Id});
                cn.Close();
            }
        }
    }
}
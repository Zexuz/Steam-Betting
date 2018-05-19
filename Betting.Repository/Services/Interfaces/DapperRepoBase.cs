using Betting.Repository.Factories;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Services.Interfaces
{
    public class DapperRepoBase
    {
        protected IDatabaseConnection DatabaseConnection { get; }

        protected DapperRepoBase(IDatabaseConnectionFactory connectionFactory)
        {
            DatabaseConnection = connectionFactory.GetDatabaseConnection(Database.Main);
        }
    }
}
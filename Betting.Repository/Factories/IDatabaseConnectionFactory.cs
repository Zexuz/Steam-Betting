using Betting.Repository.Interfaces;

namespace Betting.Repository.Factories
{
    public interface IDatabaseConnectionFactory
    {
        IDatabaseConnection GetDatabaseConnection(Database database);
    }
}
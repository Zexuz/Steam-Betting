using Betting.Repository.Interfaces;

namespace Betting.Repository.Factories
{
    public interface IDatabaseHelperFactory
    {
        IDatabaseHelper GetDatabaseHelperForType(Database database, string connectionString, string databaseName = "Betting");
    }
}
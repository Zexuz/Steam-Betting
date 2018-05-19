using System;
using Betting.Repository.Helpers;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Factories
{
    public class DatabaseHelperFactory : IDatabaseHelperFactory
    {
        public IDatabaseHelper GetDatabaseHelperForType(Database database, string connectionString, string databaseName = "Betting")
        {
            switch (database)
            {
                case Database.Main:
                    return databaseName == "Betting"
                        ? new DatabaseHelper(connectionString, SqlQueries.CreateMainDatabaseTablesQuery)
                        : new DatabaseHelper(connectionString, SqlQueries.CreateMainDatabaseTablesQuery, databaseName);
                case Database.Settings:
                    return databaseName == "Betting"
                        ? new DatabaseHelper(connectionString, SqlQueries.CreateSettingsDatabaseTablesQuery)
                        : new DatabaseHelper(connectionString, SqlQueries.CreateSettingsDatabaseTablesQuery, databaseName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(database), database, null);
            }
        }
    }

    public enum Database
    {
        Main,
        Settings
    }
}
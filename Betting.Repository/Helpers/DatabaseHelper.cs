using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Helpers
{
    public class DatabaseHelper : IDatabaseHelper
    {
        private readonly IDatabaseConnection _databaseConnection;
        private readonly string              _createTablesString;
        private readonly string              _databaseName;

        public DatabaseHelper(string connectionStringForMaster, string createTablesString, string databaseName = "Betting")
        {
            _databaseConnection = new DatabaseConnection(connectionStringForMaster);
            _createTablesString = createTablesString;
            _databaseName       = databaseName;
        }

        public async Task<bool> DoesDatabaseExist()
        {
            var databaseName = _databaseName.Replace("[", "");
            databaseName     = databaseName.Replace("]", "");
            var query        = new SqlQuery("SELECT database_id FROM sys.databases WHERE Name = @databaseName",
                new Dictionary<string, object> {{"@databaseName", databaseName}});
            var obj        = await _databaseConnection.ExecuteScalarOnDiffrentDatabaseAsync(query);
            int databaseId = 0;

            if (obj != null)
            {
                int.TryParse(obj.ToString(), out databaseId);
            }

            return databaseId > 0;
        }

        public void ResetDatabase()
        {
            DropDatabase();
            CreateDatabase();
            CreateTables();
        }

        public int CreateDatabase()
        {
            return _databaseConnection.ExecuteNonQueryOnDiffrentDatabase(new SqlQuery($"CREATE DATABASE {_databaseName}", null));
        }

        public int DropDatabase()
        {
            return _databaseConnection.ExecuteNonQueryOnDiffrentDatabase(new SqlQuery($"DROP DATABASE IF EXISTS {_databaseName}", null));
        }

        public int CreateTables()
        {
            var createQuery = _createTablesString.Replace("@databaseName", _databaseName);
            return _databaseConnection.ExecuteNonQueryOnDiffrentDatabase(new SqlQuery(createQuery, null));
        }
    }
}
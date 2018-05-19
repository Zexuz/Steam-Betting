using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Impl;

namespace Betting.Repository.Interfaces
{
    public interface IDatabaseConnection
    {
        Task            ExecuteNonQueriesAsync(IEnumerable<string> sqlQueries);
        Task<SqlResult> ExecuteSqlQueryAsync(SqlQuery sqlQuery);
        Task<int>       ExecuteNonQueryAsync(SqlQuery sqlQuery);
        Task<object>    ExecuteScalarAsync(SqlQuery sqlQuery);
        Task<object>    ExecuteScalarOnDiffrentDatabaseAsync(SqlQuery sqlQuery, string database = "master");
        Task<int>       ExecuteNonQueryOnDiffrentDatabaseAsync(SqlQuery sqlQueries, string database = "master");
        int             ExecuteNonQueryOnDiffrentDatabase(SqlQuery sqlQueries, string database = "master");
        Task<SqlResult> ExecuteSqlQueryOnDiffrentDatabaseAsync(SqlQuery sqlQueries, string database = "master");
        SqlResult       ExecuteSqlQueryOnDiffrentDatabase(SqlQuery sqlQueries, string database = "master");
        SqlCommand      SetQueryParameters(SqlQuery sqlQuery, SqlConnection connection);
        SqlConnection   GetNewOpenConnection();
    }
}
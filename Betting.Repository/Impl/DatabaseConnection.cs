using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Impl
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private readonly string _connectionString;

        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task ExecuteNonQueriesAsync(IEnumerable<string> sqlQueries)
        {
            using (var connection = GetNewOpenConnection())
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    foreach (var sqlQuery in sqlQueries)
                    {
                        command.CommandText = sqlQuery;
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<SqlResult> ExecuteSqlQueryAsync(SqlQuery sqlQuery)
        {
            var connection = GetNewOpenConnection();
            var command    = SetQueryParameters(sqlQuery, connection);
            var dataReder  = await command.ExecuteReaderAsync();
            return new SqlResult(connection, command, dataReder);
        }

        public async Task<int> ExecuteNonQueryAsync(SqlQuery sqlQuery)
        {
            using (var connection = GetNewOpenConnection())
            using (var command = SetQueryParameters(sqlQuery, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<object> ExecuteScalarAsync(SqlQuery sqlQuery)
        {
            using (var connection = GetNewOpenConnection())
            using (var command = SetQueryParameters(sqlQuery, connection))
            {
                return await command.ExecuteScalarAsync();
            }
        }

        public async Task<object> ExecuteScalarOnDiffrentDatabaseAsync(SqlQuery sqlQuery, string database = "master")
        {
            using (var connection = GetNewOpenConnection())
            {
                connection.ChangeDatabase(database);
                using (var command = SetQueryParameters(sqlQuery, connection))
                {
                    return await command.ExecuteScalarAsync();
                }
            }
        }

        public async Task<int> ExecuteNonQueryOnDiffrentDatabaseAsync(SqlQuery sqlQueries, string database = "master")
        {
            using (var connection = GetNewOpenConnection())
            {
                connection.ChangeDatabase(database);
                using (var command = SetQueryParameters(sqlQueries, connection))
                {
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        public int ExecuteNonQueryOnDiffrentDatabase(SqlQuery sqlQueries, string database = "master")
        {
            using (var connection = GetNewOpenConnection())
            {
                connection.ChangeDatabase(database);
                using (var command = SetQueryParameters(sqlQueries, connection))
                {
                    return command.ExecuteNonQuery();
                }
            }
        }

        public async Task<SqlResult> ExecuteSqlQueryOnDiffrentDatabaseAsync(SqlQuery sqlQueries, string database = "master")
        {
            using (var connection = GetNewOpenConnection())
            {
                connection.ChangeDatabase(database);
                using (var command = SetQueryParameters(sqlQueries, connection))
                {
                    var dataReder = await command.ExecuteReaderAsync();
                    return new SqlResult(connection, command, dataReder);
                }
            }
        }

        public SqlResult ExecuteSqlQueryOnDiffrentDatabase(SqlQuery sqlQueries, string database = "master")
        {
            using (var connection = GetNewOpenConnection())
            {
                connection.ChangeDatabase(database);
                using (var command = SetQueryParameters(sqlQueries, connection))
                {
                    var dataReder = command.ExecuteReader();
                    return new SqlResult(connection, command, dataReder);
                }
            }
        }


        public SqlCommand SetQueryParameters(SqlQuery sqlQuery, SqlConnection connection)
        {
            using (var sqlCommand = new SqlCommand(sqlQuery.Text, connection))
            {
                if (sqlQuery.Parameters == null) return sqlCommand;

                foreach (var parameter in sqlQuery.Parameters)
                {
                    var s     = parameter.Value as string;
                    var value = s != null ? s.Trim() : (parameter.Value ?? SqlString.Null);
                    sqlCommand.Parameters.AddWithValue(parameter.Key, value);
                }

                return sqlCommand;
            }
        }

        public SqlConnection GetNewOpenConnection()
        {
            var conn = new SqlConnection
            {
                ConnectionString = _connectionString
            };
            conn.Open();
            return conn;
        }
    }

    public class SqlResult : IDisposable
    {
        public SqlConnection Connection { get; }
        public SqlCommand    Command    { get; }
        public SqlDataReader Reader     { get; }


        public SqlResult(SqlConnection connection, SqlCommand command, SqlDataReader reader)
        {
            Connection = connection;
            Command    = command;
            Reader     = reader;
        }

        public void Dispose()
        {
            Connection?.Dispose();
            Command?.Dispose();
            Reader?.Dispose();
        }
    }
}
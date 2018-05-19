using System.Data.SqlClient;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class TransactionWrapperWrapper : ITransactionWrapper
    {
        private readonly IDatabaseConnection _connection;
        private readonly SqlTransaction      _transaction;

        public SqlTransaction Transaction => _transaction;
        public SqlConnection SqlConnection => _transaction.Connection;

        public TransactionWrapperWrapper(IDatabaseConnection connection)
        {
            _connection  = connection;
            _transaction = connection.GetNewOpenConnection().BeginTransaction();
        }

        public async Task<T> ExecuteSqlCommand<T>(SqlQuery query)
        {
            try
            {
                var sqlCommand         = _connection.SetQueryParameters(query, Transaction.Connection);
                sqlCommand.Transaction = _transaction;
                return (T) await sqlCommand.ExecuteScalarAsync();
            }
            catch (SqlException e)
            {
                throw new CantCompleteTransaction(e);
            }
        }

        public async Task ExecuteSqlCommand(SqlQuery query)
        {
            try
            {
                var sqlCommand         = _connection.SetQueryParameters(query, Transaction.Connection);
                sqlCommand.Transaction = _transaction;
                await sqlCommand.ExecuteNonQueryAsync();
            }
            catch (SqlException e)
            {
                throw new CantCompleteTransaction(e);
            }
        }

        public void Commit()
        {
            _transaction.Commit();
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Dispose()
        {
            _transaction.Connection?.Dispose();
            _transaction.Dispose();
        }
    }
}
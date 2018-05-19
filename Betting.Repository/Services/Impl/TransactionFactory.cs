using Betting.Repository.Factories;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Services.Impl
{
    public class TransactionFactory : ITransactionFactory
    {
        private readonly IDatabaseConnection _connection;

        public TransactionFactory(IDatabaseConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.GetDatabaseConnection(Database.Main);
        }

        public ITransactionWrapper BeginTransaction()
        {
            return new TransactionWrapperWrapper(_connection);
        }
    }
}
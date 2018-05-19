using System.Threading.Tasks;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using FakeItEasy;

namespace Betting.Repository.Test.Repo
{
    public abstract class RepoServiceTestBase
    {
        protected abstract string                     DatabaseName { get; }
        private            string                     _connectionString;
        public             IDatabaseConnectionFactory FakedFactory { get; protected set; }
        public             IDatabaseConnection        Database     { get; private set; }

        protected virtual async Task InitTest()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();

            _connectionString = new ConnectionStringsForTest().GetConnectionString(DatabaseName);
            Database          = new DatabaseConnection(_connectionString);

            FakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => FakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(Database);
        }
    }
}
using System;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test
{
    public class TransactionIntegrationTestSetup
    {
        public string DatabaseName => "BettingTestTransactionUser";

        public TransactionIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Database.Main, connectionString, DatabaseName).ResetDatabase();
        }
    }


    public class TransactionTest : IClassFixture<TransactionIntegrationTestSetup>
    {
        private readonly UserRepoService    _userRepoService;
        private          DatabaseConnection _databaseConnection;

        public TransactionTest(TransactionIntegrationTestSetup setup)
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString(setup.DatabaseName);
            _databaseConnection  = new DatabaseConnection(connectionString);
        }

        [Fact]
        public async Task TransactionInsertSuccessfully()
        {
            var userToInsert     = new DatabaseModel.User("randomSteamId", "name", "imgUrl", "tradeLink", DateTime.Now, DateTime.Now,false);
            var userToInsert1    = new DatabaseModel.User("randomSteamId1", "name", "imgUrl", "tradeLink", DateTime.Now, DateTime.Now,false);
            var insertUserQuery  = new QueryFactory().UserQueries.InsertReturnsId(userToInsert);
            var insertUserQuery1 = new QueryFactory().UserQueries.InsertReturnsId(userToInsert1);

            using (var transaction = new TransactionWrapperWrapper(_databaseConnection))
            {
                var userId  = await transaction.ExecuteSqlCommand<int>(insertUserQuery);
                var userId1 = await transaction.ExecuteSqlCommand<int>(insertUserQuery1);

                Assert.NotEqual(userId, userId1);
                transaction.Commit();
            }
        }

        [Fact]
        public async Task TransactionInsertThrows()
        {
            var userToInsert    = new DatabaseModel.User("werryRandomSteamId", "name", "imgUrl", "tradeLink", DateTime.Now, DateTime.Now,false);
            var insertUserQuery = new QueryFactory().UserQueries.InsertReturnsId(userToInsert);

            var didTrow = false;
            using (var transaction = new TransactionWrapperWrapper(_databaseConnection))
            {
                try
                {
                    var userId = await transaction.ExecuteSqlCommand<int>(insertUserQuery);

                    var userId1 = await transaction.ExecuteSqlCommand<int>(insertUserQuery);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    didTrow = true;
                    transaction.Rollback();
                }

                Assert.True(didTrow);

                var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
                A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(_databaseConnection);

                var userRepoService = new UserRepoService(fakedFactory, new QueryFactory().UserQueries);
                var user            = await userRepoService.FindAsync("werryRandomSteamId");
                Assert.Null(user);
            }
        }
    }
}
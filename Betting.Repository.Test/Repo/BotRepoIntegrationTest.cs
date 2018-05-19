using System.Collections.Generic;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class BotRepoIntegrationTestSetup
    {
        public string DatabaseName => "BettingTestBot";


        public BotRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();
        }
    }

    public class BotIntegrationTest : IClassFixture<BotRepoIntegrationTestSetup>
    {
        private string                     _connectionString;
        private IDatabaseConnectionFactory _fakedFactory;

        public BotIntegrationTest(BotRepoIntegrationTestSetup setup)
        {
            _connectionString = new ConnectionStringsForTest().GetConnectionString(setup.DatabaseName);
            var database      = new DatabaseConnection(_connectionString);
            _fakedFactory     = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => _fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(database);
        }


        [Fact]
        public async void BotInsertOneSuccess()
        {
            var botReposeService = new BotRepoService(_fakedFactory, new BotQueries());
            var bot              = new DatabaseModel.Bot("someSteamId", "Bot 1");
            var insertRes        = await botReposeService.InsertAsync(bot);
            Assert.True(insertRes.Id > 0);

            var getRes = await botReposeService.FindAsync(1);
            Assert.Equal("someSteamId", getRes.SteamId);
            Assert.Equal("Bot 1", getRes.Name);
        }

        [Fact]
        public async void BotFindRangeSuccess()
        {
            var botReposeService = new BotRepoService(_fakedFactory, new BotQueries());
            var id1              = await botReposeService.InsertAsync(new DatabaseModel.Bot("someSteamId1", "Bot 11"));
            var id2              = await botReposeService.InsertAsync(new DatabaseModel.Bot("someSteamId2", "Bot 12"));
            var id3              = await botReposeService.InsertAsync(new DatabaseModel.Bot("someSteamId3", "Bot 13"));
            var id4              = await botReposeService.InsertAsync(new DatabaseModel.Bot("someSteamId4", "Bot 14"));

            var getRes = await botReposeService.FindAsync(new List<int>
            {
                id1.Id,
                id3.Id,
                id2.Id,
            });

            Assert.Equal(3, getRes.Count);
        }
    }
}
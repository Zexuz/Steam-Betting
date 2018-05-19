using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class LevelRepoIntegrationTestSetup
    {
        public string DatabaseName => "BettingLevelTest";


        public LevelRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Settings, connectionString, DatabaseName).ResetDatabase();
        }
    }

    public class LevelRepoIntegrationTest : IClassFixture<LevelRepoIntegrationTestSetup>
    {
        private readonly LevelRepoService _levelService;

        public LevelRepoIntegrationTest(LevelRepoIntegrationTestSetup setup)
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString(setup.DatabaseName);
            var database         = new DatabaseConnection(connectionString);
            var fakedFactory     = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Settings)).Returns(database);
            _levelService = new LevelRepoService(fakedFactory, new LevelQueries());
        }

        [Fact]
        public async void AddStaffSuccess()
        {
            var insertLevel = await _levelService.Add(new DatabaseModel.Level("Super Admin", true, true, true));
            Assert.True(insertLevel.Id > 0);
            Assert.Equal("Super Admin", insertLevel.Name);
            Assert.True(insertLevel.Admin);
            Assert.True(insertLevel.Chat);
            Assert.True(insertLevel.Ticket);
        }

        [Fact]
        public async void FindSuccess()
        {
            var insertLevel = await _levelService.Add(new DatabaseModel.Level("Super Admin1", true, true, true));
            var res         = await _levelService.Find(insertLevel.Id);
            Assert.Equal("Super Admin1", res.Name);
        }
    }
}
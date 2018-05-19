using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class StaffRepoIntegrationTestSetup
    {
        public string DatabaseName => "BettingStaffTest";


        public StaffRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Settings, connectionString, DatabaseName).ResetDatabase();
        }
    }

    public class StaffRepoIntegrationTest : IClassFixture<StaffRepoIntegrationTestSetup>
    {
        private readonly StaffRepoService _staffRepoService;
        private readonly LevelRepoService _levelService;

        public StaffRepoIntegrationTest(StaffRepoIntegrationTestSetup setup)
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString(setup.DatabaseName);
            var database         = new DatabaseConnection(connectionString);
            var fakedFactory     = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Settings)).Returns(database);

            _staffRepoService = new StaffRepoService(fakedFactory, new StaffQueries());
            _levelService     = new LevelRepoService(fakedFactory, new LevelQueries());
        }

        [Fact]
        public async void AddStaff()
        {
            var insertLevel = await _levelService.Add(new DatabaseModel.Level("Super Admin", true, true, true));
            await _staffRepoService.Add("someSteamId", insertLevel.Id);
            var staff = await _staffRepoService.Find("someSteamId");
            Assert.Equal(staff.Level, insertLevel.Id);
        }

        [Fact]
        public async void GetAllSuccess()
        {
            var insertLevel = await _levelService.Add(new DatabaseModel.Level("Super Admin2", true, true, true));
            await _staffRepoService.Add("someSteamId1", insertLevel.Id);
            await _staffRepoService.Add("someSteamId2", insertLevel.Id);
            await _staffRepoService.Add("someSteamId3", insertLevel.Id);
            var staffs = await _staffRepoService.GetAll();
            Assert.True(staffs.Count >= 3);
        }
    }
}
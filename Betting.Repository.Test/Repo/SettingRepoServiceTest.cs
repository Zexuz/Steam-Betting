using System;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class SettingRepoServiceTestSetup
    {
        public string DatabaseName => "BettingSettingsTest";

        public SettingRepoServiceTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Database.Settings, connectionString, DatabaseName).ResetDatabase();
        }
    }


    public class SettingRepoServiceTest : IClassFixture<SettingRepoServiceTestSetup>
    {
        private readonly SettingRepoServiceTestSetup _setup;

        public SettingRepoServiceTest(SettingRepoServiceTestSetup setup)
        {
            _setup = setup;
        }

        [Fact]
        public async void WholeFlowInOneTest()
        {
            var connectionString   = new ConnectionStringsForTest().GetConnectionString(_setup.DatabaseName);
            var databaseConnection = new DatabaseConnection(connectionString);

            var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Settings)).Returns(databaseConnection);

            var settingsRepo = new SettingRepoService(fakedFactory);

            await settingsRepo.SetSettingsAsync(new DatabaseModel.Settings(100, new decimal(14), 10, DateTime.Today, 20));

            var res = await settingsRepo.GetSettingsAsync();
            Assert.Equal(100, res.InventoryLimit);
            Assert.Equal(new decimal(14), res.ItemValueLimit);
            Assert.Equal(10, res.SteamInventoryCacheTimerInSec);
            Assert.Equal(DateTime.Today, res.UpdatedPricingTime);
            Assert.Equal(20, res.NrOfLatestChatMessages);

            await settingsRepo.SetSettingsAsync(new DatabaseModel.Settings(10, new decimal(1337), 5, DateTime.Today, 10));

            var res2 = await settingsRepo.GetSettingsAsync();
            Assert.Equal(10, res2.InventoryLimit);
            Assert.Equal(new decimal(1337), res2.ItemValueLimit);
            Assert.Equal(5, res2.SteamInventoryCacheTimerInSec);
            Assert.Equal(DateTime.Today, res2.UpdatedPricingTime);
            Assert.Equal(10, res2.NrOfLatestChatMessages);
        }
    }
}
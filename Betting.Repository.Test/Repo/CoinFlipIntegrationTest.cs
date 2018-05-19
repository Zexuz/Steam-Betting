using System;
using System.Globalization;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using Betting.Repository.Services.Interfaces;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public sealed class CoinFlipIntegrationTestServiceIntegrationTestSetup : RepoServiceTestBase
    {
        protected override string DatabaseName => "CoinFlipMatchTest";

        public DatabaseModel.User     User1    { get; set; }
        public DatabaseModel.GameMode GameMode { get; set; }

        public CoinFlipIntegrationTestServiceIntegrationTestSetup()
        {
            InitTest().Wait();
        }

        protected override async Task InitTest()
        {
            await base.InitTest();

            User1 = await new UserRepoService(FakedFactory, new UserQueries()).InsertAsync("randomSteamId", "randomName", "imageUr");
            GameMode = await new GameModeRepoService(FakedFactory).Insert(new DatabaseModel.GameMode("CoinFlip", 1));
        }
    }

    public class CoinFlipIntegrationTest : IClassFixture<CoinFlipIntegrationTestServiceIntegrationTestSetup>
    {
        private readonly CoinFlipIntegrationTestServiceIntegrationTestSetup _setup;

        private readonly CoinFlipMatchRepoService _coinFlipMatchRepoService;

        public CoinFlipIntegrationTest(CoinFlipIntegrationTestServiceIntegrationTestSetup setup)
        {
            _setup = setup;
            _coinFlipMatchRepoService = new CoinFlipMatchRepoService(_setup.FakedFactory);
        }

        [Fact]
        public async void InsertReturnsIdSuccess()
        {
            var entity = new DatabaseModel.CoinFlip
            {
                Created = DateTime.Now,
                CreatorUserId = _setup.User1.Id,
                CreatorIsHead = true,
                GameModeId = _setup.GameMode.Id,
                Hash = "HASh",
                Percentage = 10.654.ToString(CultureInfo.InvariantCulture),
                RoundId = Guid.NewGuid().ToString(),
                Salt = "SALT",
                SettingId = 0,
                Status = 0,
                TimerStarted = null,
                WinnerId = null,
            };

            using (var trans = new TransactionWrapperWrapper(_setup.Database))
            {
                Assert.True(entity.IsNew);
                var insertedCoinFlip = await _coinFlipMatchRepoService.InsertAsync(entity, trans);
                Assert.False(entity.IsNew);
                Assert.True(insertedCoinFlip.Id > 0);
                trans.Commit();
            }
        }

        [Fact]
        public async void UpdateWinnerSuccess()
        {
            var entity = new DatabaseModel.CoinFlip
            {
                Created = DateTime.Now,
                CreatorUserId = _setup.User1.Id,
                CreatorIsHead = true,
                GameModeId = _setup.GameMode.Id,
                Hash = "HASh",
                Percentage = 10.654.ToString(CultureInfo.InvariantCulture),
                RoundId = Guid.NewGuid().ToString(),
                Salt = "SALT",
                SettingId = 0,
                Status = 0,
                TimerStarted = null,
                WinnerId = null,
            };
            using (var trans = new TransactionWrapperWrapper(_setup.Database))
            {
                var insertedCoinFlip = await _coinFlipMatchRepoService.InsertAsync(entity, trans);
                insertedCoinFlip.WinnerId = _setup.User1.Id;
                trans.Commit();

                await _coinFlipMatchRepoService.UpdateAsync(insertedCoinFlip);
                var findEntiry = await _coinFlipMatchRepoService.FindAsync(entity.Id);

                Assert.Equal(_setup.User1.Id, findEntiry.WinnerId);
            }
        }

        [Fact]
        public async void InsertFailsAndRollBackIsDoneSuccess()
        {
            var entity = new DatabaseModel.CoinFlip
            {
                Created = DateTime.Now,
                CreatorUserId = _setup.User1.Id,
                CreatorIsHead = true,
                GameModeId = _setup.GameMode.Id,
                Hash = "HASh",
                Percentage = 10.654.ToString(CultureInfo.InvariantCulture),
                RoundId = Guid.NewGuid().ToString(),
                Salt = "SALT",
                SettingId = 0,
                Status = 0,
                TimerStarted = null,
                WinnerId = null,
            };

            using (var trans = new TransactionWrapperWrapper(_setup.Database))
            {
                Assert.True(entity.IsNew);
                var insertedCoinFlip = await _coinFlipMatchRepoService.InsertAsync(entity, trans);
                Assert.False(entity.IsNew);
                Assert.True(insertedCoinFlip.Id > 0);
                trans.Rollback();
                var res = await _coinFlipMatchRepoService.FindAsync(insertedCoinFlip.Id);
                Assert.Equal(null,res);
            }
        }
    }
}
using Betting.Models.Models;
using Betting.Repository.Services.Interfaces;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public sealed class GameModeRepoServiceIntegrationTestSetup : RepoServiceTestBase
    {
        protected override string DatabaseName => "GameModeTest";

        public GameModeRepoServiceIntegrationTestSetup()
        {
            InitTest().Wait();
        }
    }


    public class GameModeRepoServiceIntegrationTest : IClassFixture<GameModeRepoServiceIntegrationTestSetup>
    {
        private readonly GameModeRepoServiceIntegrationTestSetup _setup;

        public GameModeRepoServiceIntegrationTest(GameModeRepoServiceIntegrationTestSetup setup)
        {
            _setup = setup;
        }

        [Fact]
        public async void InsertReturnsId()
        {
            var gameMode = new DatabaseModel.GameMode("Jackpot", 1);
            Assert.True(gameMode.IsNew);
            var gameModeRepoService = new GameModeRepoService(_setup.FakedFactory);
            await gameModeRepoService.Insert(gameMode);
            Assert.True(gameMode.Id > 0);
            Assert.False(gameMode.IsNew);
        }

        [Fact]
        public async void Find()
        {
            var gameMode            = new DatabaseModel.GameMode("Jackpot1", 1);
            var gameModeRepoService = new GameModeRepoService(_setup.FakedFactory);
            await gameModeRepoService.Insert(gameMode);

            var res = await gameModeRepoService.Find(gameMode.Id);

            Assert.Equal("Jackpot1", res.Type);
        }

        [Fact]
        public async void Find1()
        {
            var gameMode            = new DatabaseModel.GameMode("Jackpot2", 1);
            var gameModeRepoService = new GameModeRepoService(_setup.FakedFactory);
            await gameModeRepoService.Insert(gameMode);

            var res = await gameModeRepoService.Find("Jackpot2");

            Assert.Equal("Jackpot2", res.Type);
            Assert.Equal(gameMode.Id, res.Id);
        }
    }
}
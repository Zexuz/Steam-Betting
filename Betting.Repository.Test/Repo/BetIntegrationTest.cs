using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Impl;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class BetRepoIntegrationTestSetup
    {
        public string              DatabaseName => "BettingTestBet";
        public string              ConnectionString;
        public IDatabaseConnection Database { get; private set; }

        public DatabaseModel.User     User1    { get; private set; }
        public DatabaseModel.User     User2    { get; private set; }
        public DatabaseModel.User     User3    { get; private set; }
        public DatabaseModel.User     User4    { get; private set; }
        public DatabaseModel.Match    Match1   { get; private set; }
        public DatabaseModel.Match    Match2   { get; private set; }
        public DatabaseModel.Match    Match3   { get; private set; }
        public DatabaseModel.CoinFlip CoinFlip1   { get; private set; }
        public DatabaseModel.CoinFlip CoinFlip2   { get; private set; }
        public DatabaseModel.CoinFlip CoinFlip3   { get; private set; }
        public DatabaseModel.GameMode GameMode { get; private set; }
        public DatabaseModel.GameMode GameMode1 { get; private set; }


        public BetRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();
            InitTest().Wait();
        }

        public async Task InitTest()
        {
            ConnectionString = new ConnectionStringsForTest().GetConnectionString(DatabaseName);
            Database = new DatabaseConnection(ConnectionString);

            var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(Database);

            var userRepo = new UserRepoService(fakedFactory, new UserQueries());
            var matchRepoService = new MatchRepoService(fakedFactory, new MatchQueries());
            var coinFlipMatchRepoService = new CoinFlipMatchRepoService(fakedFactory);

            var gameModeRepoService = new GameModeRepoService(fakedFactory);

            var gameMode = new DatabaseModel.GameMode(GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo), 1);
            var gameMode1 = new DatabaseModel.GameMode(GameModeHelper.GetStringFromType(GameModeType.CoinFlip), 1);
            GameMode = await gameModeRepoService.Insert(gameMode);
            GameMode1 = await gameModeRepoService.Insert(gameMode1);

            User1 = await userRepo.InsertAsync(new DatabaseModel.User(
                "987654321",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User2 = await userRepo.InsertAsync(new DatabaseModel.User(
                "987154321",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User3 = await userRepo.InsertAsync(new DatabaseModel.User(
                "983654321",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User4 = await userRepo.InsertAsync(new DatabaseModel.User(
                "983656584",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            Match1 = await matchRepoService.InsertAsync(new DatabaseModel.Match(
                11,
                "salt",
                "hash",
                47.5484.ToString(CultureInfo.InvariantCulture),
                1,
                null,
                null,
                1,
                GameMode.Id,
                DateTime.Now
            ));
            Match2 = await matchRepoService.InsertAsync(new DatabaseModel.Match(
                12,
                "salt",
                "hash",
                47.5484.ToString(CultureInfo.InvariantCulture),
                1,
                null,
                null,
                1,
                GameMode.Id,
                DateTime.Now
            ));
            Match3 = await matchRepoService.InsertAsync(new DatabaseModel.Match(
                13,
                "salt",
                "hash",
                47.5484.ToString(CultureInfo.InvariantCulture),
                1,
                null,
                null,
                1,
                GameMode.Id,
                DateTime.Now
            ));

            using (var transaction = new TransactionWrapperWrapper(Database))
            {
                CoinFlip1 = await coinFlipMatchRepoService.InsertAsync(new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Today,
                    CreatorIsHead = true,
                    CreatorUserId = User1.Id,
                    GameModeId = gameMode1.Id,
                    Hash = "",
                    Percentage = "",
                    Salt = "",
                    RoundId = "xF!",
                    SettingId = 0,
                    Status = 1,
                    TimerStarted = null,
                    WinnerId = null
                }, transaction);
                CoinFlip2 = await coinFlipMatchRepoService.InsertAsync(new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Today,
                    CreatorIsHead = true,
                    CreatorUserId = User1.Id,
                    GameModeId = gameMode1.Id,
                    Hash = "",
                    Percentage = "",
                    Salt = "",
                    RoundId = "xD1",
                    SettingId = 0,
                    Status = 1,
                    TimerStarted = null,
                    WinnerId = null
                }, transaction);
                CoinFlip3 = await coinFlipMatchRepoService.InsertAsync(new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Today,
                    CreatorIsHead = true,
                    CreatorUserId = User1.Id,
                    GameModeId = gameMode1.Id,
                    Hash = "",
                    Percentage = "",
                    Salt = "",
                    RoundId = "xD",
                    SettingId = 0,
                    Status = 1,
                    TimerStarted = null,
                    WinnerId = null
                }, transaction);
                transaction.Commit();
            }

        }
    }

    public class BetIntegrationTest : IClassFixture<BetRepoIntegrationTestSetup>
    {
        private readonly BetRepoIntegrationTestSetup _setup;
        private readonly IDatabaseConnection         _database;
        private          IDatabaseConnectionFactory  _fakedFactory;

        public BetIntegrationTest(BetRepoIntegrationTestSetup setup)
        {
            _setup = setup;
            _fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => _fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(setup.Database);
        }

        [Fact]
        public async void InsertBetSuccess()
        {
            var bet = new DatabaseModel.Bet(_setup.User1.Id, _setup.Match1.Id, _setup.GameMode.Id, DateTime.Now);

            var betRepo = new BetRepoService(_fakedFactory, new BetQueries());
            var res = await betRepo.InsertAsync(bet);

            Assert.True(res.Id > 0);
        }

        [Fact]
        public async void InsertSameBetThrows()
        {
            var bet = new DatabaseModel.Bet(_setup.User2.Id, _setup.Match1.Id, _setup.GameMode.Id, DateTime.Now);

            var betRepo = new BetRepoService(_fakedFactory, new BetQueries());
            await betRepo.InsertAsync(bet);
            var exception1 = await Record.ExceptionAsync(async () => { await betRepo.InsertAsync(bet); });
            Assert.IsType(typeof(SqlException), exception1);
        }

        [Fact]
        public async void GetBetForMatchIdAndUserId()
        {
            var user = _setup.User1;
            var match = _setup.Match2;
            var bet = new DatabaseModel.Bet(user.Id, match.Id, _setup.GameMode.Id, DateTime.Now);

            var betRepo = new BetRepoService(_fakedFactory, new BetQueries());
            await betRepo.InsertAsync(bet);

            var betResponse = await betRepo.FindAsync(match, user);

            Assert.Equal(user.Id, betResponse.UserId);
            Assert.Equal(match.Id, betResponse.MatchId);
        }

        [Fact]
        public async void GetBetForSpecificGameModeId()
        {
            var bet = new DatabaseModel.Bet(_setup.User2.Id, _setup.Match3.Id, _setup.GameMode.Id, DateTime.Now);
            var bet1 = new DatabaseModel.Bet(_setup.User2.Id, _setup.CoinFlip1.Id, _setup.GameMode1.Id, DateTime.Now);

            var betRepo = new BetRepoService(_fakedFactory, new BetQueries());
            await betRepo.InsertAsync(bet);
           var insertedBet1 =  await betRepo.InsertAsync(bet1);

            var res = await betRepo.FindAsync(new List<LookUpGameModeBet>
            {
                new LookUpGameModeBet
                {
                    GameMode = _setup.GameMode1,
                    MatchIds = new List<int>
                    {
                        _setup.CoinFlip1.Id
                    },
                    User = _setup.User2
                }
            });
            Assert.Equal(1,res.Count);
            Assert.Equal(insertedBet1.Id,res.First().Id);
        }
        
        [Fact]
        public async void GetBetForSpecificGameModeIdMultipleSuccess()
        {
            var bet1 = new DatabaseModel.Bet(_setup.User4.Id, _setup.Match1.Id, _setup.GameMode.Id, DateTime.Now);
            var bet2 = new DatabaseModel.Bet(_setup.User4.Id, _setup.Match2.Id, _setup.GameMode.Id, DateTime.Now);
            var bet3 = new DatabaseModel.Bet(_setup.User4.Id, _setup.Match3.Id, _setup.GameMode.Id, DateTime.Now);
            var bet4 = new DatabaseModel.Bet(_setup.User1.Id, _setup.Match3.Id, _setup.GameMode.Id, DateTime.Now);
            var bet5 = new DatabaseModel.Bet(_setup.User4.Id, _setup.CoinFlip1.Id, _setup.GameMode1.Id, DateTime.Now);
            var bet6 = new DatabaseModel.Bet(_setup.User4.Id, _setup.CoinFlip2.Id, _setup.GameMode1.Id, DateTime.Now);
            var bet7 = new DatabaseModel.Bet(_setup.User1.Id, _setup.CoinFlip3.Id, _setup.GameMode1.Id, DateTime.Now);
            var bet8 = new DatabaseModel.Bet(_setup.User4.Id, _setup.CoinFlip3.Id, _setup.GameMode1.Id, DateTime.Now);
            
            var betRepo = new BetRepoService(_fakedFactory, new BetQueries());

            await betRepo.InsertAsync(bet1);
            await betRepo.InsertAsync(bet2);
            await betRepo.InsertAsync(bet3);
            await betRepo.InsertAsync(bet4);
            await betRepo.InsertAsync(bet5);
            await betRepo.InsertAsync(bet6);
            await betRepo.InsertAsync(bet7);
            await betRepo.InsertAsync(bet8);
            
            var res = await betRepo.FindAsync(new List<LookUpGameModeBet>
            {
                new LookUpGameModeBet
                {
                    GameMode = _setup.GameMode1,
                    MatchIds = new List<int>
                    {
                        _setup.CoinFlip1.Id,
                        _setup.CoinFlip2.Id,
                        _setup.CoinFlip3.Id
                    },
                    User = _setup.User4
                },
                new LookUpGameModeBet
                {
                    GameMode = _setup.GameMode,
                    MatchIds = new List<int>
                    {
                        _setup.Match1.Id,
                        _setup.Match3.Id,
                    },
                    User = _setup.User4
                }
            });
            
            Assert.Equal(5,res.Count);
            
        }
    }
}
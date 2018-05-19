using System;
using System.Collections.Generic;
using System.Globalization;
using Betting.Backend.Interfaces;
using Betting.Backend.Managers.Impl;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;
using Item = Betting.Models.Models.Item;
using User = Betting.Models.Models.User;

namespace Betting.Backend.Test
{
    public class MatchManagerTest
    {
        private IMatchRepoService       _fakeMatchRepoServicey;
        private IRepoServiceFactory     _fakedRepoServiceFactory;
        private IHashService            _fakeHashService;
        private IRandomService          _fakeRandomService;
        private IMatchHubConnections    _fakedMatchHub;
        private IBetService             _fakeBetService;
        private IBetHubConnections      _fakedBetHub;
        private IGameModeSettingService _fakedGameModeSettingService;

        public MatchManagerTest()
        {
            _fakedRepoServiceFactory = A.Fake<IRepoServiceFactory>();
            _fakeMatchRepoServicey = A.Fake<IMatchRepoService>();
            _fakeHashService = A.Fake<IHashService>();
            _fakeRandomService = A.Fake<IRandomService>();
            _fakedMatchHub = A.Fake<IMatchHubConnections>();
            _fakedBetHub = A.Fake<IBetHubConnections>();
            _fakeBetService = A.Fake<IBetService>();
            _fakedGameModeSettingService = A.Fake<IGameModeSettingService>();

            A.CallTo(() => _fakedRepoServiceFactory.MatchRepoService).Returns(_fakeMatchRepoServicey);
        }

        [Fact]
        public async void GetCurrentMatchSuccess()
        {
            var matchToReturn = new DatabaseModel.Match(1, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 0,
                DateTime.Now, 1);


            A.CallTo(() => _fakeMatchRepoServicey.GetCurrentMatch()).Returns(matchToReturn);

            A.CallTo(() => _fakedGameModeSettingService.Find(A<int>._, A<GameModeType>._))
                .Returns(new DatabaseModel.JackpotSetting(0, 0, 50, 0, 0, 0, 0, 0, true, false, ""));


            A.CallTo(() => _fakeBetService.GetBettedItemsOnMatch(1,matchToReturn.GameModeId)).Returns(new List<Item>
            {
                new Item
                {
                    IconUrl = "imageUrl",
                    Name = "name1",
                    Value = 10,
                    Owner = new User
                    {
                        ImageUrl = "userImage",
                        Name = "userName",
                        SteamId = "123456789"
                    }
                },
                new Item
                {
                    IconUrl = "imageUrl",
                    Name = "name1",
                    Value = 10,
                    Owner = new User
                    {
                        ImageUrl = "userImage",
                        Name = "userName",
                        SteamId = "123456789"
                    }
                },
                new Item
                {
                    IconUrl = "imageUrl",
                    Name = "name1",
                    Value = 13,
                    Owner = new User
                    {
                        ImageUrl = "userImage",
                        Name = "userName",
                        SteamId = "123456789"
                    }
                },
            });

            var jackpotMatchManager = new JackpotMatchManager(
                _fakedRepoServiceFactory,
                _fakeBetService,
                _fakeHashService,
                _fakeRandomService,
                A.Dummy<IJackpotDraftService>(),
                A.Dummy<ILogServiceFactory>(),
                A.Dummy<IBetOrWithdrawQueueManager>(),
                _fakedGameModeSettingService,
                _fakedBetHub,
                _fakedMatchHub,
                A.Dummy<IDiscordService>()
            );
            var currentJackpotMatch = await jackpotMatchManager.GetCurrentMatch();


            Assert.Equal("hash", currentJackpotMatch.Hash);
            Assert.Equal(null, currentJackpotMatch.Salt);
            Assert.Equal(null, currentJackpotMatch.Percentage);
            Assert.Equal(1.ToString(), currentJackpotMatch.RoundId.ToString());
            Assert.Equal(MatchStatus.Open, currentJackpotMatch.Status);
            Assert.Equal("open", currentJackpotMatch.ReadableStatus.ToLower());
            Assert.Equal(3, currentJackpotMatch.ItemsInPool.Count);
            Assert.Equal(33, currentJackpotMatch.ValueInPool);
            Assert.Equal(1, currentJackpotMatch.Bets.Count);
            Assert.Equal(50, currentJackpotMatch.Setting.ItemLimit);

            A.CallTo(() => _fakedGameModeSettingService.Find(1, GameModeType.JackpotCsgo)).MustHaveHappened();
        }

        [Fact]
        public async void GetEmptyMatchSuccess()
        {
            var matchToReturn = new DatabaseModel.Match(1, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1,
                DateTime.Now, 1);

            A.CallTo(() => _fakeMatchRepoServicey.GetCurrentMatch()).Returns(matchToReturn);
            A.CallTo(() => _fakedGameModeSettingService.Find(A<int>._, A<GameModeType>._))
                .Returns(new DatabaseModel.JackpotSetting(0, 10000, 0, 0, 0, 0, 0, 0, true, false, ""));
            A.CallTo(() => _fakeBetService.GetBettedItemsOnMatch(1,matchToReturn.GameModeId)).Returns(new List<Item>());

            var jackpotMatchManager = new JackpotMatchManager(
                _fakedRepoServiceFactory,
                _fakeBetService,
                _fakeHashService,
                _fakeRandomService,
                A.Dummy<IJackpotDraftService>(),
                A.Dummy<ILogServiceFactory>(),
                A.Dummy<IBetOrWithdrawQueueManager>(),
                _fakedGameModeSettingService,
                _fakedBetHub,
                _fakedMatchHub,
                A.Dummy<IDiscordService>()
            );
            var currentJackpotMatch = await jackpotMatchManager.GetCurrentMatch();


            Assert.Equal("hash", currentJackpotMatch.Hash);
            Assert.Equal(null, currentJackpotMatch.Salt);
            Assert.Equal(null, currentJackpotMatch.Percentage);
            Assert.Equal(1.ToString(), currentJackpotMatch.RoundId);
            Assert.Equal(MatchStatus.Open, currentJackpotMatch.Status);
            Assert.Equal("open", currentJackpotMatch.ReadableStatus.ToLower());
            Assert.Equal(0, currentJackpotMatch.ItemsInPool.Count);
            Assert.Equal(0, currentJackpotMatch.ValueInPool);
            Assert.Equal(0, currentJackpotMatch.Bets.Count);
            Assert.Equal(10000, currentJackpotMatch.TimeLeft);

            A.CallTo(() => _fakedGameModeSettingService.Find(1, GameModeType.JackpotCsgo)).MustHaveHappened();
        }

        [Fact]
        public async void MatchIsOpenSaltAndPercentageIsNullSuccess()
        {
            var matchToReturn = new DatabaseModel.Match(1, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1,
                DateTime.Now, 1);

            A.CallTo(() => _fakeMatchRepoServicey.GetCurrentMatch()).Returns(matchToReturn);
            A.CallTo(() => _fakedGameModeSettingService.Find(A<int>._, A<GameModeType>._))
                .Returns(new DatabaseModel.JackpotSetting(0, 0, 0, 0, 0, 0, 0, 0, true, false, ""));
            A.CallTo(() => _fakeBetService.GetBettedItemsOnMatch(1,matchToReturn.GameModeId)).Returns(new List<Item>
            {
                new Item
                {
                    IconUrl = "imageUrl",
                    Name = "name1",
                    Value = 10,
                    Owner = new User
                    {
                        ImageUrl = "userImage",
                        Name = "userName",
                        SteamId = "123456789"
                    }
                }
            });

            var jackpotMatchManager = new JackpotMatchManager(
                _fakedRepoServiceFactory,
                _fakeBetService,
                _fakeHashService,
                _fakeRandomService,
                A.Dummy<IJackpotDraftService>(),
                A.Dummy<ILogServiceFactory>(),
                A.Dummy<IBetOrWithdrawQueueManager>(),
                _fakedGameModeSettingService,
                _fakedBetHub,
                _fakedMatchHub,
                A.Dummy<IDiscordService>()
            );
            var currentJackpotMatch = await jackpotMatchManager.GetCurrentMatch();


            Assert.Equal("hash", currentJackpotMatch.Hash);
            Assert.Equal(null, currentJackpotMatch.Salt);
            Assert.Equal(null, currentJackpotMatch.Percentage);
            Assert.Equal(MatchStatus.Open, currentJackpotMatch.Status);
            Assert.Equal("open", currentJackpotMatch.ReadableStatus.ToLower());
        }

        [Fact]
        public async void MatchIsClosedSaltAndPercentageIsValidSuccess()
        {
            var matchToReturn = new DatabaseModel.Match(1, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), 0, null, null, 1, 1,
                DateTime.Now, 1);

            A.CallTo(() => _fakeMatchRepoServicey.GetCurrentMatch()).Returns(matchToReturn);

            A.CallTo(() => _fakedGameModeSettingService.Find(A<int>._, A<GameModeType>._))
                .Returns(new DatabaseModel.JackpotSetting(0, 0, 0, 0, 0, 0, 0, 0, true, false, ""));

            A.CallTo(() => _fakeBetService.GetBettedItemsOnMatch(1,matchToReturn.GameModeId)).Returns(new List<Item>
            {
                new Item
                {
                    IconUrl = "imageUrl",
                    Name = "name1",
                    Value = 10,
                    Owner = new User
                    {
                        ImageUrl = "userImage",
                        Name = "userName",
                        SteamId = "123456789"
                    }
                }
            });

            var jackpotMatchManager = new JackpotMatchManager(
                _fakedRepoServiceFactory,
                _fakeBetService,
                _fakeHashService,
                _fakeRandomService,
                A.Dummy<IJackpotDraftService>(),
                A.Dummy<ILogServiceFactory>(),
                A.Dummy<IBetOrWithdrawQueueManager>(),
                _fakedGameModeSettingService,
                _fakedBetHub,
                _fakedMatchHub,
                A.Dummy<IDiscordService>()
            );
            var currentJackpotMatch = await jackpotMatchManager.GetCurrentMatch();


            Assert.Equal("hash", currentJackpotMatch.Hash);
            Assert.Equal("salt", currentJackpotMatch.Salt);
            Assert.Equal(10.ToString(CultureInfo.InvariantCulture), currentJackpotMatch.Percentage);
            Assert.Equal(MatchStatus.Closed, currentJackpotMatch.Status);
            Assert.Equal("closed", currentJackpotMatch.ReadableStatus.ToLower());
        }

        [Fact]
        public async void CreateNewMatchSuccess()
        {
            var matchInsert =
                new DatabaseModel.Match(1, "salt", "hash", "10", 1, null, null, 1, 1, DateTime.Now);
            var matchReturn = new DatabaseModel.Match(1, "salt", "hash", 10.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, 1,
                default(DateTime), 1);

            A.CallTo(() => _fakeMatchRepoServicey.InsertAsync(
                A<DatabaseModel.Match>.That.Matches(m =>
                    m.Hash       == matchInsert.Hash       &&
                    m.Salt       == matchInsert.Salt       &&
                    m.Percentage == matchInsert.Percentage &&
                    m.RoundId    == matchInsert.RoundId    &&
                    m.Status     == matchInsert.Status
                )
            )).Returns(matchReturn);

            A.CallTo(() => _fakeHashService.CreateBase64Sha512Hash(A<string>._, A<string>._)).Returns("hash");
            A.CallTo(() => _fakeRandomService.GeneratePercentage()).Returns("10");
            A.CallTo(() => _fakeRandomService.GenerateSalt()).Returns("salt");

            A.CallTo(() => _fakedGameModeSettingService.GetSettingForType(GameModeType.JackpotCsgo))
                .Returns(new DatabaseModel.JackpotSetting(0, 0, 0, 0, 0, 0, 0, 0, true, false, ""));
            A.CallTo(() => _fakedGameModeSettingService.Find(A<int>._, A<GameModeType>._))
                .Returns(new DatabaseModel.JackpotSetting(0, 0, 0, 0, 0, 0, 0, 0, true, false, ""));

            var jackpotMatchManager = new JackpotMatchManager(
                _fakedRepoServiceFactory,
                _fakeBetService,
                _fakeHashService,
                _fakeRandomService,
                A.Dummy<IJackpotDraftService>(),
                A.Dummy<ILogServiceFactory>(),
                A.Dummy<IBetOrWithdrawQueueManager>(),
                _fakedGameModeSettingService,
                _fakedBetHub,
                _fakedMatchHub,
                A.Dummy<IDiscordService>()
            );
            var match = await jackpotMatchManager.CreateNewMatchAsync(1);


            A.CallTo(() => _fakeMatchRepoServicey.InsertAsync(
                A<DatabaseModel.Match>.That.Matches(m =>
                    m.Hash       == matchInsert.Hash       &&
                    m.Salt       == matchInsert.Salt       &&
                    m.Percentage == matchInsert.Percentage &&
                    m.RoundId    == matchInsert.RoundId    &&
                    m.Status     == matchInsert.Status
                ))).MustHaveHappened();
            A.CallTo(() => _fakeHashService.CreateBase64Sha512Hash(A<string>._, A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakeRandomService.GeneratePercentage()).MustHaveHappened();
            A.CallTo(() => _fakeRandomService.GenerateSalt()).MustHaveHappened();
            Assert.Equal(1, match.Id);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Services;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Backend.Test
{
    public class CoinFlipServiceTest
    {
        private readonly IRandomService             _fakedRandomService;
        private readonly IHashService               _fakedHashService;
        private readonly CoinFlipService            _coinFlipService;
        private readonly ICoinFlipMatchRepoService  _fakedCoinFlipMatchRepoService;
        private readonly IGameModeRepoService       _fakedGameModeRepoSerivce;
        private readonly ITransactionFactory        _fakedTransactionFactory;
        private readonly ITransactionWrapper        _fakedTransactionWrapper;
        private          IBetService                _fakedBetService;
        private          IUserRepoService           _fakedUserRepoService;
        private          CreateCoinFlipSettingModel _defaultSetting;
        private          IItemService               _fakedItemService;
        private          IJackpotSettingRepo        _fakedJackpotSettingRepoService;
        private          IMongoJackpotRepoService   _fakedMongoDbJackpotRepoService;
        private          IMongoPreHashRepoService   _fakedMongoDbPreHashRepoService;

        public CoinFlipServiceTest()
        {
            _fakedHashService = A.Fake<IHashService>();
            _fakedRandomService = A.Fake<IRandomService>();
            var fakedRepoServcieFacotry = A.Fake<IRepoServiceFactory>();
            _fakedCoinFlipMatchRepoService = A.Fake<ICoinFlipMatchRepoService>();
            _fakedUserRepoService = A.Fake<IUserRepoService>();
            _fakedGameModeRepoSerivce = A.Fake<IGameModeRepoService>();
            _fakedTransactionFactory = A.Fake<ITransactionFactory>();
            _fakedTransactionWrapper = A.Fake<ITransactionWrapper>();
            _fakedBetService = A.Fake<IBetService>();
            _fakedItemService = A.Fake<IItemService>();
            _fakedJackpotSettingRepoService = A.Fake<IJackpotSettingRepo>();
            _fakedMongoDbJackpotRepoService = A.Fake<IMongoJackpotRepoService>();
            _fakedMongoDbPreHashRepoService = A.Fake<IMongoPreHashRepoService>();

            A.CallTo(() => _fakedTransactionFactory.BeginTransaction()).Returns(_fakedTransactionWrapper);
            A.CallTo(() => fakedRepoServcieFacotry.CoinFlipMatchRepoService).Returns(_fakedCoinFlipMatchRepoService);
            A.CallTo(() => fakedRepoServcieFacotry.GameModeRepoService).Returns(_fakedGameModeRepoSerivce);
            A.CallTo(() => fakedRepoServcieFacotry.UserRepoService).Returns(_fakedUserRepoService);
            A.CallTo(() => fakedRepoServcieFacotry.JackpotSettingRepo).Returns(_fakedJackpotSettingRepoService);

            _defaultSetting = new CreateCoinFlipSettingModel
            {
                AllowCsgo = true,
                AllowPubg = false,
                Diff = 1000,
                MaxItem = 10,
                MinItem = 0,
                PreHash = "ranomdHash"
            };


            _coinFlipService = new CoinFlipService
            (
                _fakedHashService,
                _fakedRandomService,
                fakedRepoServcieFacotry,
                _fakedTransactionFactory,
                _fakedBetService,
                _fakedItemService,
                _fakedMongoDbJackpotRepoService,
                A.Dummy<ICoinFlipHubConnections>(),
                _fakedMongoDbPreHashRepoService,
                A.Dummy<IDiscordService>()
            );
            //TODO CHECK ALL DUMMY TEST OBJECTS!
        }


        [Fact]
        public async Task CreateMatchFailDueToNoItemsPlaced()
        {
            var percentage = 12.4587;
            var percentageAsString = percentage.ToString(CultureInfo.InvariantCulture);

            var salt = "randomSalt";
            var hash = "ranomdHash";

            var creatorUser = new DatabaseModel.User("steamId", "name", "imageURl", null, DateTime.Today, DateTime.Today, false, null, 10);
            var gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                IsEnabled = true,
                Type = GameModeHelper.GetStringFromType(GameModeType.CoinFlip),
                Id = 2
            };

            A.CallTo(() => _fakedRandomService.GeneratePercentage()).Returns(percentageAsString);
            A.CallTo(() => _fakedRandomService.GenerateSalt()).Returns(salt);
            A.CallTo(() => _fakedHashService.CreateBase64Sha512Hash(percentageAsString, salt)).Returns(hash);
            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).Returns(creatorUser);

            A.CallTo(() => _fakedGameModeRepoSerivce.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip))).Returns(gameMode);

            var itemList = new List<AssetAndDescriptionId>();

            await Assert.ThrowsAsync<InvalidBetException>(async () =>
                await _coinFlipService.CreateMatch(creatorUser.SteamId, true, itemList, _defaultSetting));

            A.CallTo(() => _fakedCoinFlipMatchRepoService.InsertAsync(A<DatabaseModel.CoinFlip>._, A<ITransactionWrapper>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).MustHaveHappened();
        }


        [Fact]
        public async Task CreateMatchSuccess()
        {
            var percentage = 12.4587;
            var percentageAsString = percentage.ToString(CultureInfo.InvariantCulture);

            var salt = "randomSalt";
            var hash = "ranomdHash";

            var creatorUser = new DatabaseModel.User("steamId", "name", "imageURl", null, DateTime.Today, DateTime.Today, false, null, 10);
            var gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                IsEnabled = true,
                Type = GameModeHelper.GetStringFromType(GameModeType.CoinFlip),
                Id = 2
            };

            A.CallTo(() => _fakedMongoDbPreHashRepoService.Find(hash, creatorUser.SteamId)).Returns(new MongoDbModels.PreHash
            {
                Created = DateTime.Today,
                Hash = hash,
                Percentage = percentageAsString,
                Salt = salt,
                UserSteamId = creatorUser.SteamId
            });

            A.CallTo(() => _fakedRandomService.GenerateNewGuidAsString()).Returns("GUID");
            A.CallTo(() => _fakedJackpotSettingRepoService.InsertAsync(A<DatabaseModel.JackpotSetting>._, A<ITransactionWrapper>._))
                .Returns(new DatabaseModel.JackpotSetting {Id = 1});


            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).Returns(creatorUser);
            A.CallTo(() => _fakedGameModeRepoSerivce.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip))).Returns(gameMode);

            var itemList = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1}
            };


            await _coinFlipService.CreateMatch(creatorUser.SteamId, true, itemList, _defaultSetting);

            A.CallTo(() => _fakedCoinFlipMatchRepoService.InsertAsync(A<DatabaseModel.CoinFlip>.That.Matches
                (m =>
                    m.Created       > DateTime.Today          &&
                    m.CreatorUserId == 10                     &&
                    m.CreatorIsHead == true                   &&
                    m.Hash          == hash                   &&
                    m.Salt          == salt                   &&
                    m.Percentage    == percentageAsString     &&
                    m.RoundId       == "GUID"                 &&
                    m.Status        == (int) MatchStatus.Open &&
                    m.TimerStarted  == null                   &&
                    m.WinnerId      == null                   &&
                    m.GameModeId    == 2                      &&
                    m.SettingId     == 1
                ),
                A<ITransactionWrapper>._
            )).MustHaveHappened();

            A.CallTo(() => _fakedMongoDbPreHashRepoService.Find(hash,creatorUser.SteamId)).MustHaveHappened();
            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).MustHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustHaveHappened();
            A.CallTo(() => _fakedBetService.PlaceBetOnCoinFlipMatch
            (
                A<DatabaseModel.CoinFlip>._,
                A<JackpotMatchSetting>._,
                A<DatabaseModel.GameMode>._,
                A<int>._,
                A<List<DatabaseModel.Item>>._,
                A<DatabaseModel.User>._,
                A<List<DatabaseModel.ItemDescription>>._
            )).MustHaveHappened();
            A.CallTo(() => _fakedMongoDbJackpotRepoService.InsertAsync(A<MongoDbModels.JackpotMatch>._)).MustHaveHappened();
        }


        [Fact]
        public async Task CreateMatchRollBackWhenInsertMatchFailsSuccess()
        {
            var percentage = 12.4587;
            var percentageAsString = percentage.ToString(CultureInfo.InvariantCulture);

            var salt = "randomSalt";
            var hash = "ranomdHash";

            var creatorUser = new DatabaseModel.User("steamId", "name", "imageURl", null, DateTime.Today, DateTime.Today, false, null, 10);
            var gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                IsEnabled = true,
                Type = GameModeHelper.GetStringFromType(GameModeType.CoinFlip),
                Id = 2
            };

            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).Returns(creatorUser);
            A.CallTo(() => _fakedRandomService.GeneratePercentage()).Returns(percentageAsString);
            A.CallTo(() => _fakedRandomService.GenerateSalt()).Returns(salt);
            A.CallTo(() => _fakedHashService.CreateBase64Sha512Hash(percentageAsString, salt)).Returns(hash);
            A.CallTo(() => _fakedGameModeRepoSerivce.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip))).Returns(gameMode);
            A.CallTo(() => _fakedCoinFlipMatchRepoService.InsertAsync(A<DatabaseModel.CoinFlip>._, A<ITransactionWrapper>._)).Throws<Exception>();

            var itemList = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1}
            };

            bool didFail = false;
            try
            {
                await _coinFlipService.CreateMatch(creatorUser.SteamId, true, itemList, _defaultSetting);
            }
            catch (Exception)
            {
                didFail = true;
            }

            Assert.True(didFail, "The method did not throw!");


            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.Rollback()).MustHaveHappened();
            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).MustHaveHappened();
        }

        [Fact]
        public async Task CreateMatchRollBackWhenInsertMatchSettingFailsSuccess()
        {
            var percentage = 12.4587;
            var percentageAsString = percentage.ToString(CultureInfo.InvariantCulture);

            var salt = "randomSalt";
            var hash = "ranomdHash";

            var creatorUser = new DatabaseModel.User("steamId", "name", "imageURl", null, DateTime.Today, DateTime.Today, false, null, 10);
            var gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                IsEnabled = true,
                Type = GameModeHelper.GetStringFromType(GameModeType.CoinFlip),
                Id = 2
            };

            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).Returns(creatorUser);
            A.CallTo(() => _fakedRandomService.GeneratePercentage()).Returns(percentageAsString);
            A.CallTo(() => _fakedRandomService.GenerateSalt()).Returns(salt);
            A.CallTo(() => _fakedHashService.CreateBase64Sha512Hash(percentageAsString, salt)).Returns(hash);
            A.CallTo(() => _fakedGameModeRepoSerivce.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip))).Returns(gameMode);
            A.CallTo(() => _fakedJackpotSettingRepoService.InsertAsync(A<DatabaseModel.JackpotSetting>._, A<ITransactionWrapper>._))
                .Throws<Exception>();

            var itemList = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1}
            };

            bool didFail = false;
            try
            {
                await _coinFlipService.CreateMatch(creatorUser.SteamId, true, itemList, _defaultSetting);
            }
            catch (Exception)
            {
                didFail = true;
            }

            Assert.True(didFail, "The method did not throw!");

            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.Rollback()).MustHaveHappened();
            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).MustHaveHappened();
        }

        [Fact]
        public async Task CreateMatchRollBackWhenPlaceBetFailsSuccess()
        {
            var percentage = 12.4587;
            var percentageAsString = percentage.ToString(CultureInfo.InvariantCulture);

            var salt = "randomSalt";
            var hash = "ranomdHash";

            var creatorUser = new DatabaseModel.User("steamId", "name", "imageURl", null, DateTime.Today, DateTime.Today, false, null, 10);
            var gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                IsEnabled = true,
                Type = GameModeHelper.GetStringFromType(GameModeType.CoinFlip),
                Id = 2
            };

            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).Returns(creatorUser);
            A.CallTo(() => _fakedRandomService.GeneratePercentage()).Returns(percentageAsString);
            A.CallTo(() => _fakedRandomService.GenerateSalt()).Returns(salt);
            A.CallTo(() => _fakedHashService.CreateBase64Sha512Hash(percentageAsString, salt)).Returns(hash);
            A.CallTo(() => _fakedGameModeRepoSerivce.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip))).Returns(gameMode);

            A.CallTo(() => _fakedBetService.PlaceBetOnCoinFlipMatch
            (
                A<DatabaseModel.CoinFlip>._,
                A<JackpotMatchSetting>._,
                A<DatabaseModel.GameMode>._,
                A<int>._,
                A<List<DatabaseModel.Item>>._,
                A<DatabaseModel.User>._,
                A<List<DatabaseModel.ItemDescription>>._
            )).Throws<Exception>();

            var itemList = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1}
            };

            var didFail = false;
            try
            {
                await _coinFlipService.CreateMatch(creatorUser.SteamId, true, itemList, _defaultSetting);
            }
            catch (Exception)
            {
                didFail = true;
            }

            Assert.True(didFail, "The method did not throw!");

            A.CallTo(() => _fakedCoinFlipMatchRepoService.RemoveAsync(A<DatabaseModel.CoinFlip>._)).MustHaveHappened();
            A.CallTo(() => _fakedJackpotSettingRepoService.RemoveAsync(A<DatabaseModel.JackpotSetting>._)).MustHaveHappened();
            A.CallTo(() => _fakedUserRepoService.FindAsync(creatorUser.SteamId)).MustHaveHappened();
        }
    }
}
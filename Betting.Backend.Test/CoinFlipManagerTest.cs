using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using Betting.Repository.Helpers;
using Betting.Repository.Services;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Backend.Test
{
    public class CoinFlipManagerTest
    {
        private readonly IRepoServiceFactory        _fakedRepoServiceFactory;
        private readonly ICoinFlipService           _fakedCoinFlipService;
        private readonly ICoinFlipMatchRepoService  _fakedCoinFlipMatchRepoService;
        private readonly IBetRepoService            _fakedBetRepoService;
        private readonly IJackpotSettingRepo        _fakedJackpotSettingRepo;
        private readonly DatabaseModel.GameMode     _gameMode;
        private readonly IGameModeRepoService       _fakedGameModeRepoService;
        private readonly IJackpotDraftService       _fakedDraftingService;
        private readonly IUserRepoService           _fakedUserRepoService;
        private readonly ICoinFlipHubConnections    _coinFlipSocketSender;
        private          ILogServiceFactory         _dummyLogServiceFactory;
        private          IBetOrWithdrawQueueManager _dummyBetOrWithdrawQueueManager;
        private          IBetService                _dummyBetService;
        private          IBetHubConnections         _dummyBetHubConnection;
        private          IMongoJackpotRepoService   _dummyMongoJackpotRepoService;
        private          IHotStatusManager          _dummyHotStatusManager;
        private          IDiscordService            _dummyDiscordService;


        public CoinFlipManagerTest()
        {
            _fakedRepoServiceFactory = A.Fake<IRepoServiceFactory>();

            _fakedCoinFlipMatchRepoService = A.Fake<ICoinFlipMatchRepoService>();
            _fakedBetRepoService = A.Fake<IBetRepoService>();
            _fakedJackpotSettingRepo = A.Fake<IJackpotSettingRepo>();
            _fakedGameModeRepoService = A.Fake<IGameModeRepoService>();
            _fakedUserRepoService = A.Fake<IUserRepoService>();

            _fakedCoinFlipService = A.Fake<ICoinFlipService>();

            _fakedDraftingService = A.Fake<IJackpotDraftService>();

            _gameMode = new DatabaseModel.GameMode
            {
                CurrentSettingId = 1,
                Id = 2,
                IsEnabled = true,
                Type = "CoinFlip"
            };

            _coinFlipSocketSender = A.Dummy<ICoinFlipHubConnections>();

            A.CallTo(() => _fakedRepoServiceFactory.UserRepoService).Returns(_fakedUserRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.CoinFlipMatchRepoService).Returns(_fakedCoinFlipMatchRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.BetRepoService).Returns(_fakedBetRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.JackpotSettingRepo).Returns(_fakedJackpotSettingRepo);
            A.CallTo(() => _fakedRepoServiceFactory.GameModeRepoService).Returns(_fakedGameModeRepoService);

            A.CallTo(() => _fakedGameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip))).Returns(_gameMode);

            _dummyBetOrWithdrawQueueManager = A.Dummy<IBetOrWithdrawQueueManager>();
            _dummyBetHubConnection = A.Dummy<IBetHubConnections>();
            _dummyBetService = A.Dummy<IBetService>();
            _dummyLogServiceFactory = A.Dummy<ILogServiceFactory>();
            _dummyMongoJackpotRepoService = A.Dummy<IMongoJackpotRepoService>();
            _dummyHotStatusManager = A.Dummy<IHotStatusManager>();
            _dummyDiscordService = A.Dummy<IDiscordService>();
        }


        [Fact]
        public async Task MatchHasOneBetDoesNotingSuccess()
        {
            var manager = new CoinFlipManager
            (
                _fakedRepoServiceFactory,
                _fakedCoinFlipService,
                _fakedDraftingService,
                _coinFlipSocketSender,
                _dummyBetOrWithdrawQueueManager,
                _dummyBetHubConnection,
                _dummyLogServiceFactory,
                _dummyBetService,
                _dummyMongoJackpotRepoService,
                _dummyHotStatusManager,
                _dummyDiscordService
            );

            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).Returns(Task.FromResult(new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(10, 1, 2, DateTime.Today)
            }));


            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).Returns(new List<DatabaseModel.CoinFlip>
            {
                new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Today,
                    CreatorIsHead = true,
                    CreatorUserId = 10,
                    GameModeId = 2,
                    Hash = "hash",
                    Id = 1,
                    Percentage = "5",
                    RoundId = "",
                    Salt = "",
                    SettingId = 5,
                    Status = (int) MatchStatus.Open,
                    TimerStarted = null,
                    WinnerId = null
                }
            });

            await ExecuteStartStopManager(manager);

            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).MustHaveHappened();
            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).MustHaveHappened();
            A.CallTo(() => _fakedJackpotSettingRepo.Find(5)).MustHaveHappened();
            A.CallTo(() => _fakedCoinFlipMatchRepoService.UpdateAsync(A<DatabaseModel.CoinFlip>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task OpenMatchHasTwoBetsChangesStatusSuccess()
        {
            var manager = new CoinFlipManager
            (
                _fakedRepoServiceFactory,
                _fakedCoinFlipService,
                _fakedDraftingService,
                _coinFlipSocketSender,
                _dummyBetOrWithdrawQueueManager,
                _dummyBetHubConnection,
                _dummyLogServiceFactory,
                _dummyBetService,
                _dummyMongoJackpotRepoService,
                _dummyHotStatusManager,
                _dummyDiscordService
            );

            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).Returns(Task.FromResult(new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(10, 1, 2, DateTime.Today),
                new DatabaseModel.Bet(9, 1, 2, DateTime.Today),
            }));


            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).Returns(new List<DatabaseModel.CoinFlip>
            {
                new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Now,
                    CreatorIsHead = true,
                    CreatorUserId = 10,
                    GameModeId = 2,
                    Hash = "hash",
                    Id = 1,
                    Percentage = "5",
                    RoundId = "",
                    Salt = "",
                    SettingId = 5,
                    Status = (int) MatchStatus.Open,
                    TimerStarted = null,
                    WinnerId = null
                }
            });

            await ExecuteStartStopManager(manager);

            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).MustHaveHappened();
            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).MustHaveHappened();
            A.CallTo(() => _fakedJackpotSettingRepo.Find(5)).MustHaveHappened();

            A.CallTo(() => _fakedCoinFlipMatchRepoService.UpdateAsync(A<DatabaseModel.CoinFlip>.That.Matches(
                    m =>
                        m.Created       > DateTime.Today                    &&
                        m.CreatorIsHead == true                             &&
                        m.CreatorUserId == 10                               &&
                        m.GameModeId    == 2                                &&
                        m.Hash          == "hash"                           &&
                        m.Id            == 1                                &&
                        m.Percentage    == "5"                              &&
                        m.RoundId       == ""                               &&
                        m.Salt          == ""                               &&
                        m.SettingId     == 5                                &&
                        m.Status        == (int) MatchStatus.TimerCountdown &&
                        m.TimerStarted  > DateTime.Today                    &&
                        m.WinnerId      == null
                )
            )).MustHaveHappened();
        }

        [Fact]
        public async Task TimmerRanOutForMatchDraftsWinnerSuccess()
        {
            var manager = new CoinFlipManager
            (
                _fakedRepoServiceFactory,
                _fakedCoinFlipService,
                _fakedDraftingService,
                _coinFlipSocketSender,
                _dummyBetOrWithdrawQueueManager,
                _dummyBetHubConnection,
                _dummyLogServiceFactory,
                _dummyBetService,
                _dummyMongoJackpotRepoService,
                _dummyHotStatusManager,
                _dummyDiscordService
            );
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(10, 1, 2, DateTime.Today),
                new DatabaseModel.Bet(9, 1, 2, DateTime.Today),
            };

            var winningUser = new DatabaseModel.User("", "", "", "", DateTime.Now, DateTime.Now, false, null, 10);


            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).Returns(Task.FromResult(bets));


            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).Returns(new List<DatabaseModel.CoinFlip>
            {
                new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Now,
                    CreatorIsHead = true,
                    CreatorUserId = 10,
                    GameModeId = 2,
                    Hash = "hash",
                    Id = 1,
                    Percentage = "5",
                    RoundId = "",
                    Salt = "",
                    SettingId = 5,
                    Status = (int) MatchStatus.TimerCountdown,
                    TimerStarted = DateTime.Today,
                    WinnerId = null
                }
            });

            A.CallTo(() => _fakedDraftingService.GetWinningBet(5, bets, A<List<DatabaseModel.ItemBetted>>._)).Returns(new WinningBet
            {
                Bet = bets[0],
                WinningTicket = 5
            });

            A.CallTo(() => _fakedJackpotSettingRepo.Find(5)).Returns(new DatabaseModel.JackpotSetting
            {
                Rake = 10
            });

            A.CallTo(() => _fakedUserRepoService.FindAsync(10)).Returns(winningUser);

            await ExecuteStartStopManager(manager);

            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).MustHaveHappened();
            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).MustHaveHappened();
            A.CallTo(() => _fakedJackpotSettingRepo.Find(5)).MustHaveHappened();

            A.CallTo(() => _fakedCoinFlipMatchRepoService.UpdateAsync(A<DatabaseModel.CoinFlip>.That.Matches(
                    m =>
                        m.Created       > DateTime.Today                    &&
                        m.CreatorIsHead == true                             &&
                        m.CreatorUserId == 10                               &&
                        m.GameModeId    == 2                                &&
                        m.Hash          == "hash"                           &&
                        m.Id            == 1                                &&
                        m.Percentage    == "5"                              &&
                        m.RoundId       == ""                               &&
                        m.Salt          == ""                               &&
                        m.SettingId     == 5                                &&
                        m.Status        == (int) MatchStatus.TimerCountdown &&
                        m.TimerStarted  >= DateTime.Today                   &&
                        m.WinnerId      == null
                )
            )).MustNotHaveHappened();

            A.CallTo(() => _fakedCoinFlipMatchRepoService.UpdateAsync(A<DatabaseModel.CoinFlip>.That.Matches(
                    m =>
                        m.Created       > DateTime.Today              &&
                        m.CreatorIsHead == true                       &&
                        m.CreatorUserId == 10                         &&
                        m.GameModeId    == 2                          &&
                        m.Hash          == "hash"                     &&
                        m.Id            == 1                          &&
                        m.Percentage    == "5"                        &&
                        m.RoundId       == ""                         &&
                        m.Salt          == ""                         &&
                        m.SettingId     == 5                          &&
                        m.Status        == (int) MatchStatus.Drafting &&
                        m.TimerStarted  >= DateTime.Today             &&
                        m.WinnerId      == 10
                )
            )).MustHaveHappened();

            A.CallTo(() => _fakedDraftingService.GetWinningBet(5, bets, A<List<DatabaseModel.ItemBetted>>._)).MustHaveHappened();
            A.CallTo(() => _fakedUserRepoService.FindAsync(10)).MustHaveHappened();
            A.CallTo(() => _fakedDraftingService.ChangeOwnerOfItems(bets, A<List<DatabaseModel.ItemBetted>>._, winningUser, 1, 10, 2))
                .MustHaveHappened();
        }

        [Fact]
        public async Task DraftingIsDoneClosesMatchSuccess()
        {
            var manager = new CoinFlipManager
            (
                _fakedRepoServiceFactory,
                _fakedCoinFlipService,
                _fakedDraftingService,
                _coinFlipSocketSender,
                _dummyBetOrWithdrawQueueManager,
                _dummyBetHubConnection,
                _dummyLogServiceFactory,
                _dummyBetService,
                _dummyMongoJackpotRepoService,
                _dummyHotStatusManager,
                _dummyDiscordService
            );
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(10, 1, 2, DateTime.Today),
                new DatabaseModel.Bet(9, 1, 2, DateTime.Today),
            };

            A.CallTo(() => _fakedBetRepoService.FindAsync(1, 2)).Returns(Task.FromResult(bets));


            A.CallTo(() => _fakedCoinFlipMatchRepoService.FindAllNotClosedMatches()).Returns(new List<DatabaseModel.CoinFlip>
            {
                new DatabaseModel.CoinFlip
                {
                    Created = DateTime.Now,
                    CreatorIsHead = true,
                    CreatorUserId = 10,
                    GameModeId = 2,
                    Hash = "hash",
                    Id = 1,
                    Percentage = "5",
                    RoundId = "",
                    Salt = "",
                    SettingId = 5,
                    Status = (int) MatchStatus.Drafting,
                    TimerStarted = DateTime.Today,
                    WinnerId = 10
                }
            });

            await ExecuteStartStopManager(manager);

            A.CallTo(() => _fakedCoinFlipMatchRepoService.UpdateAsync(
                    A<DatabaseModel.CoinFlip>.That.Matches(flip => flip.Status == (int) MatchStatus.Closed)))
                .MustHaveHappened();
        }


        private static async Task ExecuteStartStopManager(CoinFlipManager manager)
        {
            manager.Start();
            await Task.Delay(100);
            await manager.Stop();
        }
    }
}
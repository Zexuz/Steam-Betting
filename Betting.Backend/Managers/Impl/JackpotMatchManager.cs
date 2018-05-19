using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Extension;
using Betting.Backend.Helpers;
using Betting.Backend.Interfaces;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Websockets.Models;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;

namespace Betting.Backend.Managers.Impl
{
    public interface IJackpotMatchManager
    {
        void                                 PlaceBetOnMatch(List<AssetAndDescriptionId> assetAndDescriptionIds, int matchId, string steamId);
        void                                 Start();
        void                                 Stop();
        Task<JackpotMatch>                   GetCurrentMatch();
        Task                                 InitGameModeWhenFreshDatabase();
        Task<List<JackpotMatchHistoryBasic>> GetMatchHistory(int fromMatchId, int count);
        Task<JackpotMatchHistoryDetailed>    GetMatchHistory(int roundId);
    }

    public class JackpotMatchManager : JackpotMatchService, IJackpotMatchManager
    {
        private readonly IRepoServiceFactory              _repoServiceFactory;
        private readonly IBetService                      _betService;
        private readonly IJackpotDraftService             _draftService;
        private readonly IMatchHubConnections             _matchHubConnections;
        private readonly IDiscordService _discordService;
        private readonly IBetHubConnections               _betHubConnections;
        private readonly IGameModeSettingService          _gameModeSettingService;
        private readonly IBetOrWithdrawQueueManager       _betOrWithdrawQueueManager;
        private readonly ILogService<JackpotMatchManager> _logService;
        private readonly BetQueue                         _betQueue;

        private bool                         _isRunning;
        private bool                         _isClosed;
        private Thread                       _loop;
        private DatabaseModel.JackpotSetting _currentMatchSettings;
        private DatabaseModel.GameMode       _gameMode;


        public bool IsRunning => _isRunning;

        public JackpotMatchManager
        (
            IRepoServiceFactory repoServiceFactory,
            IBetService betService,
            IHashService hashService,
            IRandomService randomService,
            IJackpotDraftService draftService,
            ILogServiceFactory logServiceFactory,
            IBetOrWithdrawQueueManager betOrWithdrawQueueManager,
            IGameModeSettingService gameModeSettingService,
            IBetHubConnections betHubConnections,
            IMatchHubConnections matchHubConnections,
            IDiscordService discordService
        ) : base(repoServiceFactory, betService, hashService, randomService)
        {
            _repoServiceFactory = repoServiceFactory;
            _betService = betService;
            _draftService = draftService;
            _matchHubConnections = matchHubConnections;
            _discordService = discordService;
            _betHubConnections = betHubConnections;
            _gameModeSettingService = gameModeSettingService;
            _betOrWithdrawQueueManager = betOrWithdrawQueueManager;
            _logService = logServiceFactory.CreateLogger<JackpotMatchManager>();

            _betQueue = new BetQueue();

            SetUpGameModeAndSetting().Wait();
        }

        public async Task<DatabaseModel.Match> CreateNewMatchAsync(int roundId)
        {
            var settingRes = await _gameModeSettingService.GetSettingForType(GameModeType.JackpotCsgo);

            _currentMatchSettings = (DatabaseModel.JackpotSetting) settingRes ?? throw new NullReferenceException("The match setting is NULL!");

            _gameMode = await _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo));

            var res = await base.CreateNewMatchAsync(roundId, _gameMode);

            var jackpotMatch = new JackpotMatch(
                roundId.ToString(),
                res.Hash,
                res.Salt,
                res.Percentage,
                MatchStatusHelper.GetMatchStatusFromInt(res.Status),
                new List<Bet>(),
                res.TimerStarted,
                new JackpotMatchSetting(
                    _currentMatchSettings.Rake,
                    TimeSpan.FromMilliseconds(_currentMatchSettings.TimmerInMilliSec),
                    _currentMatchSettings.ItemsLimit,
                    _currentMatchSettings.MaxItemAUserCanBet,
                    _currentMatchSettings.MinItemAUserCanBet,
                    _currentMatchSettings.MaxValueAUserCanBet,
                    _currentMatchSettings.MinValueAUserCanBet,
                    _currentMatchSettings.AllowCsgo,
                    _currentMatchSettings.AllowPubg,
                    TimeSpan.FromMilliseconds(_currentMatchSettings.DraftingTimeInMilliSec),
                    _currentMatchSettings.DraftingGraph
                )
            );

            await _matchHubConnections.NewMatchCreated(jackpotMatch);
            _isClosed = false;
            return res;
        }

        public void PlaceBetOnMatch(List<AssetAndDescriptionId> assetAndDescriptionIds, int roundId, string steamId)
        {
            if (_isClosed)
                throw new CanNotBetOnClosedMatchException($"Match with roundId {roundId} is closed and does not accept bet in this state.");

            var item = new BetQueue.Item
            {
                AssetAndDescriptionIds = assetAndDescriptionIds,
                RoundId = roundId,
                SteamId = steamId,
                GamMode = _gameMode
            };
            _betQueue.Add(item);
        }


        public void Start()
        {
            _isRunning = true;
            _loop = new Thread(() =>
            {
                try
                {
                    Tick();
                }
                catch (Exception e)
                {
                    _logService.Critical(e);
                    Thread.Sleep(100);
                    throw;
                }
            });
            _loop.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _loop.Join();
        }

        public new async Task<List<JackpotMatchHistoryBasic>> GetMatchHistory(int fromMatchId, int count)
        {
            return await base.GetMatchHistory(fromMatchId, 20);
        }

        public async Task<JackpotMatchHistoryDetailed> GetMatchHistory(int roundId)
        {
            var match = await _repoServiceFactory.MatchRepoService.FindAsync(roundId);
            if (MatchStatusHelper.GetMatchStatusFromInt(match.Status) == MatchStatus.Open)
                throw new Exception("Can't fetch history data on active round!");

            var rakeItems = await _repoServiceFactory.RakeItemRepoService.FindAsync(match);

            var itemsBetted = await _betService.GetBettedItemsOnMatch(match.Id, match.GameModeId);
            var basic = await GetHistoryBasic(match);
            var historyDetailed = new JackpotMatchHistoryDetailed
            {
                Bets = await GetBets(itemsBetted, match.Id, _gameMode),
                Created = basic.Created,
                Hash = basic.Hash,
                Percentage = basic.Percentage,
                RoundId = basic.RoundId,
                Salt = basic.Salt,
                UserWinnerImgUrl = basic.UserWinnerImgUrl,
                UserWinnerName = basic.UserWinnerName,
                UserWinnerQuote = basic.UserWinnerQuote,
                UserWinnerSteamId = basic.UserWinnerSteamId
            };
            foreach (var rakeItem in rakeItems)
            {
                foreach (var bet in historyDetailed.Bets)
                {
                    foreach (var betItem in bet.Items)
                    {
                        if (betItem.AssetId == rakeItem.AssetId && betItem.DescriptionId == rakeItem.DescriptionId)
                            betItem.TookAsRake = true;
                    }
                }
            }

            return historyDetailed;
        }

        public async Task<JackpotMatch> GetCurrentMatch()
        {
            return await base.GetCurrentMatch(_currentMatchSettings, _gameMode);
        }

        public async Task InitGameModeWhenFreshDatabase()
        {
            await CreateNewMatchAsync(1);
        }

        private async void Tick()
        {
            while (_isRunning)
            {
                await Task.Delay(200);
                if (_isClosed) continue;
                
                _gameMode = await _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo));


                var match = await _repoServiceFactory.MatchRepoService.GetCurrentMatch();
                var betsOnMatch = await _repoServiceFactory.BetRepoService.FindAsync(match);
                var itemBetsOnMatch = await _repoServiceFactory.ItemBettedRepoService.FindAsync(betsOnMatch);

                await ProcessQueueUntillTimeSpanIsMet(TimeSpan.Zero);

                if (ShouldStartTimmer(betsOnMatch, match))
                    await StartTimmer(match.RoundId);

                if (!ShouldCloseMatch(match, itemBetsOnMatch, _betQueue.Items.Count)) continue;


                _isClosed = true;
                await CloseMatch(match.RoundId);
                await ProcessQueueUntillDone();

                await DraftWinner(match);

                await Task.Delay(_currentMatchSettings.DraftingTimeInMilliSec);
                await CreateNewMatchAsync(match.RoundId + 1);
            }
        }

        private async Task ProcessQueueUntillTimeSpanIsMet(TimeSpan timeSpan)
        {
            int i = 0;
            while (timeSpan.TotalMilliseconds < 200)
            {
                if (_betQueue.IsEmpty())
                    break;

                var stopWatch = Stopwatch.StartNew();
                await ProcessItemInQueue();
                stopWatch.Stop();
                timeSpan.Add(stopWatch.Elapsed);
                i++;
            }

            if (i > 0)
                _logService.Info($"Prosecced {i} nr of bets in one tick");
        }

        private async Task ProcessQueueUntillDone()
        {
            while (true)
            {
                if (_betQueue.IsEmpty())
                    break;
                await ProcessItemInQueue();
            }
        }

        private async Task ProcessItemInQueue()
        {
            var item = _betQueue.Next();

            if (_betOrWithdrawQueueManager.DoesExist(item.SteamId))
            {
                await _betHubConnections.Error(item.SteamId, item.RoundId, "You alredy have a pending bet or withdraw.");
                return;
            }

            _betOrWithdrawQueueManager.Add(item.SteamId, QueueAction.Bet);
            try
            {
                var match = await _repoServiceFactory.MatchRepoService.GetCurrentMatch();
                if (item.RoundId != match.RoundId)
                    throw new MatchRoundIdNotSameException("The match roundId is not the same as the roundId that the user want to bet on");

                await _betService.PlaceBetOnJackpotMatch(match, _currentMatchSettings.ToJackpotMatchSetting(), item.GamMode,
                    item.AssetAndDescriptionIds,
                    item.SteamId);
            }
            catch (Exception ex)
            {
                _logService.Error(null, null, ex, new Dictionary<string, object>());

                if (
                    ex is GameModeIsNotEnabledException   ||
                    ex is ToManyItemsOnBetException       ||
                    ex is ToFewItemsOnBetException        ||
                    ex is NotAllowedAppIdOnMatchException ||
                    ex is ToMuchValueOnBetException       ||
                    ex is ToLittleValueOnBetException     ||
                    ex is InvalidItemException
                )
                {
                    await _betHubConnections.Error(item.SteamId, item.RoundId, ex.Message);
                    return;
                }
                
                await _betHubConnections.Error(item.SteamId, item.RoundId, "Something went wrong, please try again later.");
                return;
            }
            finally
            {
                _betOrWithdrawQueueManager.Remover(item.SteamId);
            }

            await _betHubConnections.Success(item.SteamId, item.RoundId);
        }

        private bool ShouldStartTimmer(List<DatabaseModel.Bet> betsOnMatch, DatabaseModel.Match match)
        {
            if (betsOnMatch.Count > 1 && match.TimerStarted == null)
                if (betsOnMatch.Count > 0 && match.TimerStarted == null)
                    return true;
            return false;
        }

        private async Task DraftWinner(DatabaseModel.Match match)
        {
            var betsOnMatch = await _repoServiceFactory.BetRepoService.FindAsync(match);
            var itemBetsOnMatch = await _repoServiceFactory.ItemBettedRepoService.FindAsync(betsOnMatch);

            var percentage = Convert.ToDouble(match.Percentage, CultureInfo.InvariantCulture);
            var winningBet = _draftService.GetWinningBet(percentage, betsOnMatch, itemBetsOnMatch);
            var winningUser = await _repoServiceFactory.UserRepoService.FindAsync(winningBet.Bet.UserId);

            await _repoServiceFactory.MatchRepoService.AddWinnerToMatch(winningUser, match.RoundId);

            var sum = itemBetsOnMatch.Sum(itemBet => itemBet.Value);
            await _matchHubConnections.Winner(new JackpotWinnerSelected
            {
                ImageUrl = winningUser.ImageUrl,
                Name = winningUser.Name,
                SteamId = winningUser.SteamId,
                Percentage = match.Percentage,
                PotValue = sum,
                Quote = winningUser.Quote,
                RoundId = match.RoundId,
                DraftingGraph = _currentMatchSettings.DraftingGraph,
                DraftingTimeInMs = _currentMatchSettings.DraftingTimeInMilliSec
            });

            await _draftService.ChangeOwnerOfItems(betsOnMatch, itemBetsOnMatch, winningUser, match.Id, _currentMatchSettings.Rake, _gameMode.Id);
            _discordService.JackpotWinnerAsync(match.RoundId,sum);
        }


        private async Task CloseMatch(int roundId)
        {
            await _repoServiceFactory.MatchRepoService.CloseMatch(roundId);
            await _matchHubConnections.MatchIsClosed(roundId);
        }

        private async Task StartTimmer(int matchId)
        {
            await _repoServiceFactory.MatchRepoService.StartTimerForMatch(matchId, DateTime.Now);
        }

        private bool ShouldCloseMatch(DatabaseModel.Match match, List<DatabaseModel.ItemBetted> itemBets, int currentBetCount)
        {
            if (MatchStatusHelper.GetMatchStatusFromInt(match.Status) == MatchStatus.Closed)
                return true;

            var itemLimit = _currentMatchSettings.ItemsLimit;
            if (itemBets.Count + currentBetCount >= itemLimit && itemBets.Select(i => i.BetId).Distinct().Count() > 1)
                return true;

            var jackPotTimespan = (TimeSpan.FromMilliseconds(_currentMatchSettings.TimmerInMilliSec));
            if (match.TimerStarted.HasValue && (DateTime.Now - (match.TimerStarted.Value + jackPotTimespan)).TotalMilliseconds > 0)
                return true;

            return false;
        }


        private async Task SetUpGameModeAndSetting()
        {
            var currentMatch = await _repoServiceFactory.MatchRepoService.GetCurrentMatch();
            if (currentMatch != null) //there is no matches, this is the first time we are starting the server.
                _currentMatchSettings =
                    (DatabaseModel.JackpotSetting) await _gameModeSettingService.Find(currentMatch.SettingId, GameModeType.JackpotCsgo);
            _gameMode = await _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo));
        }
    }
}
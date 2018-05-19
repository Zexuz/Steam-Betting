using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Extension;
using Betting.Backend.Helpers;
using Betting.Backend.Interfaces;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Websockets.Models;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Services;

namespace Betting.Backend.Managers.Impl
{
    public class CoinFlipManager : ICoinFlipManager
    {
        private readonly IRepoServiceFactory        _repoServiceFactory;
        private readonly ICoinFlipService           _coinFlipService;
        private readonly IJackpotDraftService       _draftService;
        private readonly ICoinFlipHubConnections    _coinFlipHubConnections;
        private readonly IBetOrWithdrawQueueManager _betOrWithdrawQueueManager;
        private readonly IBetHubConnections         _betHubConnections;
        private readonly IBetService                _betService;
        private readonly IMongoJackpotRepoService   _jackpotRepoService;
        private readonly IHotStatusManager _hotStatusManager;
        private readonly IDiscordService _discordService;

        private readonly BetQueue _betQueue;

        private bool                         _running;
        private Thread                       _tickLoopThread;
        private Thread                       _queueThread;
        private bool                         _didStopGracefully;
        private DatabaseModel.GameMode       _currentGamemode;
        private ILogService<CoinFlipManager> _logService;

        public CoinFlipManager
        (
            IRepoServiceFactory repoServiceFactory,
            ICoinFlipService coinFlipService,
            IJackpotDraftService draftService,
            ICoinFlipHubConnections coinFlipHubConnections,
            IBetOrWithdrawQueueManager betOrWithdrawQueueManager,
            IBetHubConnections betHubConnections,
            ILogServiceFactory logServiceFactory,
            IBetService betService,
            IMongoJackpotRepoService jackpotRepoService,
            IHotStatusManager hotStatusManager,
            IDiscordService discordService
        )
        {
            _repoServiceFactory = repoServiceFactory;
            _coinFlipService = coinFlipService;
            _draftService = draftService;
            _coinFlipHubConnections = coinFlipHubConnections;
            _betOrWithdrawQueueManager = betOrWithdrawQueueManager;
            _betHubConnections = betHubConnections;
            _betService = betService;
            _jackpotRepoService = jackpotRepoService;
            _hotStatusManager = hotStatusManager;
            _discordService = discordService;

            _logService = logServiceFactory.CreateLogger<CoinFlipManager>();
            _betQueue = new BetQueue();

            Setup().Wait();
        }


        public async Task PlaceBet(List<AssetAndDescriptionId> assetAndDescriptionIds, int lookUpId, string steamId)
        {
            _betQueue.Add(new BetQueue.Item
            {
                AssetAndDescriptionIds = assetAndDescriptionIds,
                GamMode = _currentGamemode,
                RoundId = lookUpId,
                SteamId = steamId
            });
        }

        public async Task<List<CoinFlipMatch>> GetAllOpenOrDratingCoinFlipMatches()
        {
            return await _coinFlipService.GetAllOpenOrDraftinMatchesFromMongoDb();
        }

        public void Start()
        {
            _running = true;
            _tickLoopThread = new Thread(() =>
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
            }) {Name = "CoinFlip manager Tick loop"};
            _tickLoopThread.Start();

            _queueThread = new Thread(() =>
            {
                try
                {
                    ProcessItemInQueue();
                }
                catch (Exception e)
                {
                    _logService.Critical(e);
                    Thread.Sleep(100);
                    throw;
                }
            }) {Name = "Queue loop"};
            _queueThread.Start();
        }

        public async Task Stop()
        {
            _running = false;

            while (!_didStopGracefully)
            {
                await Task.Delay(200);
            }

            _tickLoopThread.Join();
            _queueThread.Join();
        }

        private async void Tick()
        {
            while (_running)
            {
                await Task.Delay(200);
                
                _currentGamemode = await _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip));
                
                _hotStatusManager.RemoveExperiedHotStatuses();

                var matches = await _repoServiceFactory.CoinFlipMatchRepoService.FindAllNotClosedMatches();

                foreach (var coinFlip in matches)
                {
                    var currentMatchSetting = await _repoServiceFactory.JackpotSettingRepo.Find(coinFlip.SettingId);
                    var bets = await _repoServiceFactory.BetRepoService.FindAsync(coinFlip.Id, _currentGamemode.Id);

                    if (bets.Count > 2)
                    {
                        var e = new Exception("BETS SHOULD NEVER BE MORE THEN 2!");
                        e.Data.Add("Match", coinFlip);
                        throw e;
                    }

                    if (bets.Count == 1) continue; // we are still wating for someone to join.
                    if (bets.Count == 0) continue; // The create match and insert bet is done on two diffrent transactions sadly...

                    if (coinFlip.Status == (int) MatchStatus.Open)
                    {
                        var timer = DateTime.Now;
                        coinFlip.Status = (int) MatchStatus.TimerCountdown;
                        coinFlip.TimerStarted = timer;
                        await _repoServiceFactory.CoinFlipMatchRepoService.UpdateAsync(coinFlip);

                        await _jackpotRepoService.UpdateStatusForMatch(coinFlip, MatchStatus.TimerCountdown);
                        await _jackpotRepoService.UpdateTimmerStartedForMatch(coinFlip, timer);

                        await SendWebSocketEventPlayerJoinedAndInsertCoinFlipBetToMongoDb(coinFlip, currentMatchSetting, bets);

                        continue;
                    }

                    if (!coinFlip.TimerStarted.HasValue)
                    {
                        var e = new Exception("Timmer should always have a value here!");
                        e.Data.Add("match", coinFlip);
                        throw e;
                    }


                    if (coinFlip.Status == (int) MatchStatus.TimerCountdown)
                    {
                        if (!IsTimerDone(currentMatchSetting, coinFlip)) continue;

                        //Timer is done, start drafting! 
                        await DraftWinner(coinFlip, currentMatchSetting, bets);
                        continue;
                    }

                    if (coinFlip.Status == (int) MatchStatus.Drafting)
                    {
                        if (!IsDraftingDone(currentMatchSetting, coinFlip)) continue;
                        coinFlip.Status = (int) MatchStatus.Closed;
                        await _repoServiceFactory.CoinFlipMatchRepoService.UpdateAsync(coinFlip);
                        await _jackpotRepoService.UpdateStatusForMatch(coinFlip, MatchStatus.Closed);
                        await _coinFlipHubConnections.MatchClosed(coinFlip);
                        continue;
                    }
                }
            }

            _didStopGracefully = true;
        }

        private async Task SendWebSocketEventPlayerJoinedAndInsertCoinFlipBetToMongoDb
        (
            DatabaseModel.CoinFlip coinFlip,
            DatabaseModel.JackpotSetting currentMatchSetting,
            List<DatabaseModel.Bet> bets
        )
        {
            var creatorBet = bets.First(bet => bet.UserId == coinFlip.CreatorUserId);
            var creatorItemBetted = await _repoServiceFactory.ItemBettedRepoService.FindAsync(creatorBet);
            var creatorValueBet = creatorItemBetted.Sum(betted => betted.Value);

            var newBet = bets.First(bet => bet.UserId != coinFlip.CreatorUserId);
            var itemBetted = await _repoServiceFactory.ItemBettedRepoService.FindAsync(newBet);
            var valueBet = itemBetted.Sum(betted => betted.Value);

            var dbUser = await _repoServiceFactory.UserRepoService.FindAsync(newBet.UserId);

            var user = new User
            {
                ImageUrl = dbUser.ImageUrl,
                Name = dbUser.Name,
                SteamId = dbUser.SteamId
            };

            var unquieItemDescriptions = itemBetted.Select(item => item.DescriptionId).Distinct().ToList();

            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(unquieItemDescriptions);

            var items = new List<Item>();
            foreach (var itemDescription in itemDescriptions)
            {
                foreach (var betted in itemBetted)
                {
                    if (betted.DescriptionId != itemDescription.Id) continue;

                    items.Add(new Item
                    {
                        AssetId = betted.AssetId,
                        DescriptionId = itemDescription.Id,
                        IconUrl = itemDescription.ImageUrl,
                        Name = itemDescription.Name,
                        Value = itemDescription.Value,
                        TookAsRake = false,
                        Owner = user,
                    });
                }
            }

            var coinflipBet = new CoinFlipBet
            {
                IsHead = !coinFlip.CreatorIsHead,
                Items = items,
                User = user,
                DateTime = newBet.Created,
                Tickets = new RoundTicket
                {
                    Start = ((int) (creatorValueBet * 100)) + 1,
                    End = (int) ((valueBet + creatorValueBet) * 100)
                }
            };

            var websocketData = new TimerStartedWebsocketModel
            {
                RoundId = coinFlip.RoundId,
                Bet = coinflipBet,
                DraftingTimeInMilliSec = currentMatchSetting.DraftingTimeInMilliSec
            };
            await _jackpotRepoService.AddBetToMatch(coinFlip, coinflipBet);
            await _coinFlipHubConnections.MatchTimmerStarted(websocketData);
            _discordService.CoinFlipJoinAsync(creatorValueBet,coinFlip.RoundId,user.SteamId);
        }

        private async void ProcessItemInQueue()
        {
            while (true)
            {
                if (_betQueue.IsEmpty())
                {
                    await Task.Delay(200);
                    continue;
                }

                var item = _betQueue.Next();

                if (_betOrWithdrawQueueManager.DoesExist(item.SteamId))
                {
                    await _betHubConnections.Error(item.SteamId, item.RoundId, "You alredy have a pending bet or withdraw.");
                    continue;
                }

                DatabaseModel.CoinFlip match;
                DatabaseModel.JackpotSetting matchSetting;
                try
                {
                    match = await _repoServiceFactory.CoinFlipMatchRepoService.FindAsync(item.RoundId);
                    matchSetting = await _repoServiceFactory.JackpotSettingRepo.Find(match.SettingId);
                }
                catch (Exception e)
                {
                    _logService.Error(null, null, e, new Dictionary<string, object>());
                    await _betHubConnections.Error(item.SteamId, item.RoundId, "Something went wrong, please try again later.");
                    continue;
                }

                var bets = await _repoServiceFactory.BetRepoService.FindAsync(item.RoundId, _currentGamemode.Id);
                if (bets.Count == 2)
                {
                    await _betHubConnections.Error(item.SteamId, item.RoundId, "The match has no empty slots");
                    continue;
                }

                _betOrWithdrawQueueManager.Add(item.SteamId, QueueAction.Bet);
                try
                {
                    await _betService.PlaceBetOnCoinFlipMatch(match, matchSetting.ToJackpotMatchSetting(), item.GamMode, item.AssetAndDescriptionIds,
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
                        continue;
                    }
                    
                    await _betHubConnections.Error(item.SteamId, item.RoundId, "Something went wrong, please try again later.");
                    continue;
                }
                finally
                {
                    _betOrWithdrawQueueManager.Remover(item.SteamId);
                }

                await _betHubConnections.Success(item.SteamId, item.RoundId);
            }
        }

        private bool IsTimerDone(DatabaseModel.JackpotSetting currentMatchSetting, DatabaseModel.CoinFlip coinFlip)
        {
            var timeSpan = TimeSpan.FromMilliseconds(currentMatchSetting.TimmerInMilliSec);
            return ((DateTime.Now - (coinFlip.TimerStarted.Value + timeSpan)).TotalMilliseconds > 0);
        }

        private bool IsDraftingDone(DatabaseModel.JackpotSetting currentMatchSetting, DatabaseModel.CoinFlip coinFlip)
        {
            var countdownTimeSpan = TimeSpan.FromMilliseconds(currentMatchSetting.TimmerInMilliSec);
            var draftingTimeSpan = TimeSpan.FromMilliseconds(currentMatchSetting.DraftingTimeInMilliSec);
            return ((DateTime.Now - (coinFlip.TimerStarted.Value + countdownTimeSpan + draftingTimeSpan)).TotalMilliseconds > 0);
        }

        private async Task Setup()
        {
            _currentGamemode = await _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip));
        }

        private async Task DraftWinner(DatabaseModel.CoinFlip match, DatabaseModel.JackpotSetting matchSetting, List<DatabaseModel.Bet> betsOnMatch)
        {
            var itemBetsOnMatch = await _repoServiceFactory.ItemBettedRepoService.FindAsync(betsOnMatch);

            var percentage = Convert.ToDouble(match.Percentage, CultureInfo.InvariantCulture);
            var winningBet = _draftService.GetWinningBet(percentage, betsOnMatch, itemBetsOnMatch);
            var winningUser = await _repoServiceFactory.UserRepoService.FindAsync(winningBet.Bet.UserId);

            match.WinnerId = winningUser.Id;
            match.Status = (int) MatchStatus.Drafting;
            await _repoServiceFactory.CoinFlipMatchRepoService.UpdateAsync(match);

            await _jackpotRepoService.UpdateStatusForMatch(match, MatchStatus.Drafting);
            await _jackpotRepoService.SetWinnerForMatch(match, new UserWithQuote
            {
                ImageUrl = winningUser.ImageUrl,
                Name = winningUser.Name,
                SteamId = winningUser.SteamId,
                Quote = winningUser.Quote
            });
            await _jackpotRepoService.SetWinnerTicketForMatch(match, winningBet.WinningTicket + 1);

            var potValue = itemBetsOnMatch.Sum(itemBet => itemBet.Value);

            await _coinFlipHubConnections.MatchDrafting(new CoinFlipWinnerSelected
            {
                Winner = new UserWithQuote
                {
                    ImageUrl = winningUser.ImageUrl,
                    Name = winningUser.Name,
                    SteamId = winningUser.SteamId,
                    Quote = winningUser.Quote,
                },
                Percentage = match.Percentage,
                PotValue = potValue,
                RoundId = match.RoundId,
                WinningTicket = winningBet.WinningTicket + 1,
                Salt = match.Salt,
            });

            await _draftService.ChangeOwnerOfItems(betsOnMatch, itemBetsOnMatch, winningUser, match.Id, matchSetting.Rake, _currentGamemode.Id);
            _discordService.CoinFlipWinnerAsync(match.RoundId,potValue);
        }
    }
}
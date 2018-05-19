using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Extension;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Exceptions;
using Betting.Repository.Extensions;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Services;
using Betting.Repository.Services.Interfaces;
using Shared.Shared;

namespace Betting.Backend.Services.Impl
{
    public class CoinFlipService : ICoinFlipService
    {
        private readonly IHashService             _hashService;
        private readonly IRandomService           _randomService;
        private readonly IRepoServiceFactory      _repoServiceFactory;
        private readonly ITransactionFactory      _transactionFactory;
        private readonly IBetService              _betService;
        private readonly IItemService             _itemService;
        private readonly IMongoJackpotRepoService _mongoJackpotRepoService;
        private readonly ICoinFlipHubConnections  _coinFlipHubConnections;
        private readonly IMongoPreHashRepoService _preHashRepoService;
        private readonly IDiscordService _discordService;


        public CoinFlipService
        (
            IHashService hashService,
            IRandomService randomService,
            IRepoServiceFactory repoServiceFactory,
            ITransactionFactory transactionFactory,
            IBetService betService,
            IItemService itemService,
            IMongoJackpotRepoService mongoJackpotRepoService,
            ICoinFlipHubConnections coinFlipHubConnections,
            IMongoPreHashRepoService preHashRepoService,
            IDiscordService discordService
        )
        {
            _hashService = hashService;
            _randomService = randomService;
            _repoServiceFactory = repoServiceFactory;
            _transactionFactory = transactionFactory;
            _betService = betService;
            _itemService = itemService;
            _mongoJackpotRepoService = mongoJackpotRepoService;
            _coinFlipHubConnections = coinFlipHubConnections;
            _preHashRepoService = preHashRepoService;
            _discordService = discordService;
        }


        public async Task<CoinFlipMatch> CreateMatch
        (
            string creatorSteamId,
            bool creatorIsHead,
            List<AssetAndDescriptionId> assetAndDescriptionIds,
            CreateCoinFlipSettingModel setting
        )
        {
            var user = await _repoServiceFactory.UserRepoService.FindAsync(creatorSteamId);
            if (user == null)
                throw new UserDoesNotExistException($"The user does with steamId {creatorSteamId} not exist.");

            if (assetAndDescriptionIds.Count == 0)
                throw new InvalidBetException("No items selected");

            var items = await _repoServiceFactory.ItemRepoService.FindAsync(assetAndDescriptionIds);

            var totalValueOfItems = await _itemService.GetSumOfItems(items);

            var jackpotSetting = CreateJackpotSetting(setting, totalValueOfItems);
            var jackpotMatchSetting = jackpotSetting.ToJackpotMatchSetting();

            var itemDescsIds = assetAndDescriptionIds.Select(item => item.DescriptionId).Distinct().ToList();

            var itemDescriptionTask = _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescsIds);
            var gameModeTask = _repoServiceFactory.GameModeRepoService.Find(GameModeHelper.GetStringFromType(GameModeType.CoinFlip));
            var getPreHashTask = _preHashRepoService.Find(setting.PreHash, creatorSteamId);

            await Task.WhenAll(gameModeTask, itemDescriptionTask, getPreHashTask);

            if (!gameModeTask.Result.IsEnabled)
                throw new GameModeIsNotEnabledException("The current gamemode CoinFlip is not enabled at the moment.");

            var match = CreateCoinFlipMatch(creatorIsHead, user, gameModeTask.Result, getPreHashTask.Result);


            var insertedMatch = await InsertedMatchAndBetWithTransaction
            (
                assetAndDescriptionIds,
                match,
                jackpotSetting,
                gameModeTask.Result,
                items,
                user,
                itemDescriptionTask.Result,
                jackpotMatchSetting
            );

            var mongoDbMatch = await InsertIntoMongoDb(insertedMatch, items, itemDescriptionTask.Result, jackpotMatchSetting, user);

            var coinFlipMatch = new CoinFlipMatch(mongoDbMatch);

            await _coinFlipHubConnections.MatchCreated(coinFlipMatch);
            
            _discordService.CoinFlipCreateAsync(setting.AllowCsgo,setting.AllowPubg,totalValueOfItems,match.RoundId,user.SteamId);

            return coinFlipMatch;
        }

        public async Task<string> CreatePreHash(string steamId)
        {
            var percentage = _randomService.GeneratePercentage();
            var salt = _randomService.GenerateSalt();
            var hash = _hashService.CreateBase64Sha512Hash(percentage, salt);
            var preHash = new MongoDbModels.PreHash
            {
                Created = DateTime.Now,
                Hash = hash,
                Percentage = percentage,
                Salt = salt,
                UserSteamId = steamId
            };

            await _preHashRepoService.Insert(preHash);
            return preHash.Hash;
        }

        public Task<MongoDbModels.PreHash> FindPreHash(string hash, string steamId)
        {
            return _preHashRepoService.Find(hash, steamId);
        }

        public async Task<CoinFlipMatchHistory> GetMatchAsync(int lookUpId)
        {
            return await _mongoJackpotRepoService.FindMatchFromRoundId(lookUpId);
        }

        public async Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchs()
        {
            return await _repoServiceFactory.CoinFlipMatchRepoService.FindAllOpenMatchesAsync();
        }

        public async Task<List<DatabaseModel.CoinFlip>> FindAllOpenMatchs(DatabaseModel.User user)
        {
            return await _repoServiceFactory.CoinFlipMatchRepoService.FindAllOpenMatchesAsync(user);
        }

        public async Task<List<DatabaseModel.CoinFlip>> FindAllOpenOrDraftingMatchesMatchs(DatabaseModel.User user)
        {
            return await _repoServiceFactory.CoinFlipMatchRepoService.FindAllOpenMatchesAsync(user);
        }

        public async Task<List<CoinFlipMatch>> GetAllOpenOrDraftinMatchesFromMongoDb()
        {
            return await _mongoJackpotRepoService.GetAllOpenOrDraftinMatchesFromMongoDb();
        }
        
        public async Task<Result<CoinFlipMatchHistory>> GetGlobalHistory(int start, int count)
        {
            return await _mongoJackpotRepoService.GetHistory(start,count);
        }

        public async Task<Result<CoinFlipMatchHistory>> GetPersonalHistory(int start, int count, string steamId)
        {
            return await _mongoJackpotRepoService.GetPersonalHistory(start,count,steamId);
        }
        
        private DatabaseModel.CoinFlip CreateCoinFlipMatch
        (
            bool isHead,
            DatabaseModel.User creatorUser,
            DatabaseModel.GameMode gameMode,
            MongoDbModels.PreHash preHash
        )
        {
            var percentage = preHash.Percentage;
            var salt = preHash.Salt;
            var hash = preHash.Hash;

            var roundId = _randomService.GenerateNewGuidAsString();

            var match = new DatabaseModel.CoinFlip
            {
                Created = DateTime.Now,
                CreatorUserId = creatorUser.Id,
                CreatorIsHead = isHead,
                Hash = hash,
                Percentage = percentage,
                Salt = salt,
                RoundId = roundId,
                Status = (int) MatchStatus.Open,
                WinnerId = null,
                TimerStarted = null,
                SettingId = gameMode.CurrentSettingId,
                GameModeId = gameMode.Id,
            };
            return match;
        }

        private async Task<MongoDbModels.JackpotMatch> InsertIntoMongoDb
        (
            DatabaseModel.CoinFlip insertedMatch,
            List<DatabaseModel.Item> items,
            List<DatabaseModel.ItemDescription> itemDescription,
            JackpotMatchSetting jackpotMatchSetting,
            DatabaseModel.User user
        )
        {
            var mapedItems = items.MapItemsToItemsDescription(itemDescription);
            var coinFlipMatchModel = new MongoDbModels.JackpotMatch
            {
                LookUpId = insertedMatch.Id,
                RoundId = insertedMatch.RoundId,
                Hash = insertedMatch.Hash,
                Salt = insertedMatch.Salt,
                Percentage = insertedMatch.Percentage,
                Status = insertedMatch.Status,
                TimerStarted = insertedMatch.TimerStarted,
                Bets = new List<CoinFlipBet>
                {
                    new CoinFlipBet
                    {
                        User = new User
                        {
                            ImageUrl = user.ImageUrl,
                            Name = user.Name,
                            SteamId = user.SteamId,
                        },
                        DateTime = DateTime.Now,
                        Items = mapedItems,
                        Tickets = new RoundTicket
                        {
                            Start = 1,
                            End = (int) (mapedItems.Sum(item => item.Value) * 100)
                        },
                        IsHead = insertedMatch.CreatorIsHead
                    }
                },
                Setting = jackpotMatchSetting
            };

            await _mongoJackpotRepoService.InsertAsync(coinFlipMatchModel);
            return coinFlipMatchModel;
        }

        private async Task<DatabaseModel.CoinFlip> InsertedMatchAndBetWithTransaction
        (
            List<AssetAndDescriptionId> assetAndDescriptionIds,
            DatabaseModel.CoinFlip match,
            DatabaseModel.JackpotSetting jackpotSetting,
            DatabaseModel.GameMode gameMode,
            List<DatabaseModel.Item> items,
            DatabaseModel.User user,
            List<DatabaseModel.ItemDescription> itemDescription,
            JackpotMatchSetting jackpotMatchSetting
        )
        {
            DatabaseModel.CoinFlip insertedMatch;
            DatabaseModel.JackpotSetting insertedJackpotSetting;

            using (var transaction = _transactionFactory.BeginTransaction())
            {
                try
                {
                    insertedJackpotSetting = await _repoServiceFactory.JackpotSettingRepo.InsertAsync(jackpotSetting, transaction);
                    match.SettingId = insertedJackpotSetting.Id;
                    insertedMatch = await _repoServiceFactory.CoinFlipMatchRepoService.InsertAsync(match, transaction);
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }

                transaction.Commit();
            }

            try
            {
                var count = assetAndDescriptionIds.Count;
                await _betService.PlaceBetOnCoinFlipMatch(insertedMatch, jackpotMatchSetting, gameMode, count, items, user, itemDescription);
            }
            catch (Exception)
            {
                await _repoServiceFactory.CoinFlipMatchRepoService.RemoveAsync(insertedMatch);
                await _repoServiceFactory.JackpotSettingRepo.RemoveAsync(insertedJackpotSetting);
                throw;
            }

            return insertedMatch;
        }

        private DatabaseModel.JackpotSetting CreateJackpotSetting(CreateCoinFlipSettingModel setting, decimal totalValueOfItems )
        {
            var factor = ((setting.Diff / new decimal(100.0)) + 1);
            var maxDiff = (totalValueOfItems * factor);
            var minDiff = (totalValueOfItems / factor);
            var jackpotSetting = new DatabaseModel.JackpotSetting
            (
                5,
                10              * 1000,
                setting.MaxItem * 2,
                setting.MaxItem,
                setting.MinItem,
                decimal.Round(maxDiff, 2),
                decimal.Round(minDiff, 2),
                10 * 1000,
                setting.AllowCsgo,
                setting.AllowPubg,
                "drafting graph"
            );
            return jackpotSetting;
        }
    }
}
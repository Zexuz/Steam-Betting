using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Cache;
using Betting.Backend.Exceptions;
using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using RpcCommunication;
using Item = RpcCommunication.Item;
using User = RpcCommunication.User;

namespace Betting.Backend.Services.Impl
{
    public class SteamService : ISteamService
    {
        private readonly IRepoServiceFactory        _repoServiceFactory;
        private readonly IBotService                _botService;
        private readonly IOfferService              _offerService;
        private readonly ISteamServiceClientWrapper _steamServiceClient;
        private          ILogService<SteamService>  _logService;

        public SteamService
        (
            IGrpcServiceFactory grpcServiceFactory,
            IRepoServiceFactory repoServiceFactory,
            IBotService botService,
            ILogServiceFactory factory,
            IOfferService offerService,
            ISteamInventoryCacheManager cacheManager
        )
        {
            _logService = factory.CreateLogger<SteamService>();
            _repoServiceFactory = repoServiceFactory;
            _botService = botService;
            _offerService = offerService;
            _steamServiceClient = grpcServiceFactory.GetSteamServiceClient(cacheManager);
        }

        public async Task<GetPlayerSteamInventoryResponse> GetPlayerSteamInventoryAsync(string steamId, int appId, string contextId)
        {
            var request = new GetPlayerSteamInventoryRequest
            {
                SteamId = steamId,
                InventoryToFetch = new Inventory
                {
                    AppId = appId,
                    ContextId = contextId
                }
            };
            return await _steamServiceClient.GetPlayerSteamInventoryAsync(request);
        }

        public async Task<GetPlayerInfoResponse> GetPlayerInfoAsync(string steamId)
        {
            var req = new GetPlayerInfoRequest
            {
                SteamId = steamId
            };
            return await _steamServiceClient.GetPlayerInfoAsync(req);
        }

        public async Task<List<MakeOfferResponse>> MakeWithdrawOfferAsync(string steamId, List<AssetAndDescriptionId> ids)
        {
            var list = new List<MakeOfferResponse>();
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);

            if (string.IsNullOrEmpty(user.TradeLink))
                throw new TradeLinkNotSetExeption("The tradelink is not set");

            var offers = await _botService.GetBotsWithWithdrawItems(ids);

            if (offers.Values.Any(items => items.Any(item => item.OwnerId != user.Id)))
            {
                throw new UserDoesNotOwnThisItemsException(
                    $"The user with id {user.Id} does not own all items in the AssetAndDescriptionId array {string.Join(",", ids)}");
            }

            foreach (var offer in offers)
            {
                var itemsInOffer = (await GetItemsInOffer(offer.Value)).ToList();
                var request = MakeOfferRequest(user, offer.Key, itemsInOffer, false);
                var steamOfferResponse = await _steamServiceClient.MakeOfferAsync(request);

                if (steamOfferResponse.DataCase == MakeOfferResponse.DataOneofCase.Error &&
                    steamOfferResponse.Error.Message.ToLower().Contains("not logged in"))
                {
                    await _steamServiceClient.StopAllBotsAsync(new StopAllBotsRequest());
                    await Task.Delay(1000);
                    await _steamServiceClient.StartAllBotsAsync(new StartAllBotsRequest());
                    await Task.Delay(1000);
                    steamOfferResponse = await _steamServiceClient.MakeOfferAsync(request);
                }

                steamOfferResponse.Bot = new Bot();
                list.Add(steamOfferResponse);
                await AddPrepareWithdrawOfferToDatabase(itemsInOffer, offer.Key, user, steamOfferResponse);
            }

            return list;
        }

        public async Task<List<MakeOfferResponse>> MakeDepositOfferAsync(string steamId, List<Item> unsafeItemsWithoutMarketHashName)
        {
            var list = new List<MakeOfferResponse>();
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);
            if (string.IsNullOrEmpty(user.TradeLink))
                throw new TradeLinkNotSetExeption("The tradelink is not set");

            var playerSteamInvenotry = await GetPlayerInventory(steamId);

            var items = ConvertUnsafeItemsToSafeItems(unsafeItemsWithoutMarketHashName, playerSteamInvenotry);

            if (items.Count != unsafeItemsWithoutMarketHashName.Count)
                throw new HaxxorKidIsInTheHouseExeption("We have a H4x0r in the house! Watch out!");

            var usersItems = await GetUsersItemsAndValidateOfferData(items, user);

            var bots = await _botService.GetAvalibleBotsForDeposit(user, usersItems);

            var bot = bots.Pop();
            var request = MakeOfferRequest(user, bot, items, true);
            var response = await _steamServiceClient.MakeOfferAsync(request);

            if (response.DataCase == MakeOfferResponse.DataOneofCase.Error && response.Error.Message.ToLower().Contains("not logged in"))
            {
                await _steamServiceClient.StopAllBotsAsync(new StopAllBotsRequest());
                await Task.Delay(1000);
                await _steamServiceClient.StartAllBotsAsync(new StartAllBotsRequest());
                await Task.Delay(1000);
                response = await _steamServiceClient.MakeOfferAsync(request);
            }

            response.Bot = new Bot();
            list.Add(response);
            return list;
        }

        public async Task<SellItemsFromOpskinsBotResponse> SellItemsAsync(SellItemsFromOpskinsBotRequest request)
        {
            return await _steamServiceClient.SellItemsAsync(request);
        }

        public async Task<WithdrawBtcOpskinsResponse> WithdrawBtcOpskinsAsync(WithdrawBtcOpskinsRequest request)
        {
            return await _steamServiceClient.WithdrawBtcOpskinsAsync(request);
        }

        public async Task<AccountBalanceOpskinsResponse> AccountBalanceOpskinsAsync(AccountBalanceOpskinsRequest request)
        {
            return await _steamServiceClient.AccountBalanceOpskinsAsync(request);
        }

        public async Task<StartAllBotsResponse> StartAllBotsAsync(StartAllBotsRequest request)
        {
            return await _steamServiceClient.StartAllBotsAsync(request);
        }

        public async Task<StopAllBotsResponse> StopAllBotsAsync(StopAllBotsRequest request)
        {
            return await _steamServiceClient.StopAllBotsAsync(request);
        }

        public async Task<GetOfferLoggResponse> GetOfferLogg(GetOfferLoggRequest request)
        {
            return await _steamServiceClient.GetOfferLoggAsync(request);
        }

        public async Task<GetOpskinsLoggResponse> GetOpskinsLogg(GetOpskinsLoggRequest request)
        {
            return await _steamServiceClient.GetOpskinsLoggAsync(request);
        }

        public async Task<GetExceptionLoggResponse> GetExceptionLog(GetExceptionLoggRequest request)
        {
            return await _steamServiceClient.GetExceptionLoggAsync(request);
        }

        public async Task<GetBotLoggResponse> GetBotLogg(GetBotLoggRequest request)
        {
            return await _steamServiceClient.GetBotLoggAsync(request);
        }

        public async Task<GetBotLoginInfoResponse> GetBotLoginInfo(GetBotLoginInfoRequest request)
        {
            return await _steamServiceClient.GetBotLoginInfoAsync(request);
        }

        public async Task<MakeOfferResponse> MakeOfferAsync(MakeOfferRequest request)
        {
            return await _steamServiceClient.MakeOfferAsync(request);
        }

        private async Task AddPrepareWithdrawOfferToDatabase
        (
            List<Item> itemsInOffer,
            DatabaseModel.Bot bot,
            DatabaseModel.User user,
            MakeOfferResponse offerResponse
        )
        {
            var offerTransaction = await _offerService.PrepareWithdrawlSteamOffer(itemsInOffer.ToList(), bot, user);
            if (offerResponse.DataCase == MakeOfferResponse.DataOneofCase.Error || offerResponse.DataCase == MakeOfferResponse.DataOneofCase.None)
            {
                await _repoServiceFactory.ItemInOfferTransactionRepoService.Remove(offerTransaction.Id);
                await _repoServiceFactory.OfferTranascrionRepoService.Remove(offerTransaction.Id);
                return;
            }

            await _repoServiceFactory.OfferTranascrionRepoService.AddSteamIdToOffer(offerTransaction.Id, offerResponse.Offer.SteamOffer.Id);
        }


        private async Task<List<DatabaseModel.Item>> GetUsersItemsAndValidateOfferData
        (
            List<Item> items,
            DatabaseModel.User user
        )
        {
            var itemNames = items
                .Select(item => item.MarketHashName)
                .GroupBy(name => name)
                .Select(grp => grp.First())
                .ToList();

            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemNames);

            if (itemDescriptions.Count != itemNames.Count)
                throw new ItemDescriptionNotInDatabase("We does not have the item description for atleast one item");

            var settings = await _repoServiceFactory.SettingRepoService.GetSettingsAsync();

            if (itemDescriptions.Any(itemDesc => itemDesc.Value < settings.ItemValueLimit))
                throw new ItemInOfferNotMatchingLowestValueRuleExecption(
                    $"Atleast one item in the offer does not match the value limit. All items need a value more than {settings.ItemValueLimit}.");

            var usersItems = await _repoServiceFactory.ItemRepoService.FindAsync(user);

            if (usersItems.Count + items.Count > settings.InventoryLimit)
                throw new InventoryLimitExceeded(
                    $"Accepting this offer would put your inventory on {usersItems.Count + items.Count} item, the limit is {settings.InventoryLimit}");
            return usersItems;
        }

        private async Task<IEnumerable<Item>> GetItemsInOffer(IReadOnlyCollection<DatabaseModel.Item> databaseItems)
        {
            var itemDescIds = databaseItems.Select(item => item.DescriptionId).ToList();
            var itemDescs = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescIds);
            var itemsInOffer = new List<Item>();
            foreach (var item in databaseItems)
            {
                foreach (var itemDescription in itemDescs)
                {
                    if (item.DescriptionId != itemDescription.Id) continue;


                    itemsInOffer.Add(new Item
                    {
                        AppId = Convert.ToInt32(itemDescription.AppId),
                        ContextId = itemDescription.ContextId,
                        AssetId = item.AssetId,
                        MarketHashName = itemDescription.Name
                    });
                    break;
                }
            }

            return itemsInOffer;
        }

        private MakeOfferRequest MakeOfferRequest(DatabaseModel.User user, DatabaseModel.Bot bot, IEnumerable<Item> itemsInOffer, bool isDeposit)
        {
            var request = new MakeOfferRequest
            {
                SendItems = !isDeposit,
                User = new User
                {
                    SteamId = user.SteamId,
                    TradeLink = user.TradeLink
                },
                BotName = bot.Name,
                Items = {itemsInOffer}
            };
            return request;
        }

        //they are unsafe beacuse they are raw user input...
        private static List<Item> ConvertUnsafeItemsToSafeItems(List<Item> unsafeItems, List<Item> playerItems)
        {
            var items = new List<Item>();
            foreach (var item in unsafeItems)
            {
                foreach (var inventoryItem in playerItems)
                {
                    if (item.AssetId      != inventoryItem.AssetId
                        || item.AppId     != inventoryItem.AppId
                        || item.ContextId != inventoryItem.ContextId) continue;
                    items.Add(inventoryItem);
                    break;
                }
            }

            return items;
        }

        private async Task<List<Item>> GetPlayerInventory(string steamId)
        {
            var csgoInventoryTask = GetPlayerSteamInventory(steamId,730,"2");
            var pubgInventoryTask = GetPlayerSteamInventory(steamId,578080,"2");

            await Task.WhenAll(csgoInventoryTask, pubgInventoryTask);

            var items = new List<Item>();
            
            items.AddRange(csgoInventoryTask.Result.PlayerInventory.Items);
            items.AddRange(pubgInventoryTask.Result.PlayerInventory.Items);
            
            return items;
        }

        
        private async Task<GetPlayerSteamInventoryResponse> GetPlayerSteamInventory(string steamId, int appId, string contextId)
        {
            var req = new GetPlayerSteamInventoryRequest
            {
                SteamId = steamId,
                InventoryToFetch = new Inventory
                {
                    AppId = appId,
                    ContextId = contextId
                }
            };
            var playerSteamInvenotry = await _steamServiceClient.GetPlayerSteamInventoryAsync(req);

            if (playerSteamInvenotry.DataCase == GetPlayerSteamInventoryResponse.DataOneofCase.Error)
                throw new System.Exception(playerSteamInvenotry.Error.Message);
            return playerSteamInvenotry;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Cache;
using Betting.Backend.Exceptions;
using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using RpcCommunication;
using Xunit;
using Item = RpcCommunication.Item;
using SteamService = Betting.Backend.Services.Impl.SteamService;

namespace Betting.Backend.Test
{
    public class SteamServiceTest
    {
        private IGrpcServiceFactory        _fakedGrpcServiceFactory;
        private IBotService                _fakedBotService;
        private IRepoServiceFactory        _fakeRepoServiceFacotry;
        private SteamService               _steamService;
        private ISteamServiceClientWrapper _fakedSteamServiceClient;

        private DatabaseModel.User _realUser;
        private DatabaseModel.User _scammerUser;
        private IUserRepoService   _fakedUserRepoService;
        private IItemRepoService   _fakedItemRepoService;

        private List<DatabaseModel.Item>     _realUsersItems;
        private List<DatabaseModel.Item>     _scammerUsersItems;
        private IItemDescriptionRepoService  _fakedItemDescRepoService;
        private ISettingRepoService          _fakedSettingsRepoService;
        private ISteamInventoryCacheManager  _fakedSteamCacheManager;
        private IOfferService                _fakedOfferService;
        private IOfferTranascrionRepoService _fakedOfferTransactionRepoService;

        public SteamServiceTest()
        {
            var host = "testHost";
            var port = 8080;

            _realUser = new DatabaseModel.User("realUserSteamud", "realUser", "realImg", "myTradelink", DateTime.Now, DateTime.Now, false, null, 10);
            _scammerUser = new DatabaseModel.User("scammerUserSteamud", "scamerUser", "fakeImg", "myTradelink1", DateTime.Now, DateTime.Now, false,
                null, 20);

            _realUsersItems = new List<DatabaseModel.Item>();
            _scammerUsersItems = new List<DatabaseModel.Item>();

            _fakedGrpcServiceFactory = A.Fake<IGrpcServiceFactory>();
            _fakedBotService = A.Fake<IBotService>();
            _fakeRepoServiceFacotry = A.Fake<IRepoServiceFactory>();
            _fakedSteamServiceClient = A.Fake<ISteamServiceClientWrapper>();

            _fakedUserRepoService = A.Fake<IUserRepoService>();
            _fakedItemRepoService = A.Fake<IItemRepoService>();
            _fakedItemDescRepoService = A.Fake<IItemDescriptionRepoService>();
            _fakedSettingsRepoService = A.Fake<ISettingRepoService>();
            _fakedOfferTransactionRepoService = A.Fake<IOfferTranascrionRepoService>();


            _fakedOfferService = A.Fake<IOfferService>();

            A.CallTo(() => _fakeRepoServiceFacotry.UserRepoService).Returns(_fakedUserRepoService);
            A.CallTo(() => _fakeRepoServiceFacotry.ItemRepoService).Returns(_fakedItemRepoService);
            A.CallTo(() => _fakeRepoServiceFacotry.SettingRepoService).Returns(_fakedSettingsRepoService);
            A.CallTo(() => _fakeRepoServiceFacotry.ItemDescriptionRepoService).Returns(_fakedItemDescRepoService);
            A.CallTo(() => _fakeRepoServiceFacotry.OfferTranascrionRepoService).Returns(_fakedOfferTransactionRepoService);


            A.CallTo(() => _fakedUserRepoService.FindAsync("scammerUserSteamud")).Returns(_scammerUser);
            A.CallTo(() => _fakedUserRepoService.FindAsync("realUserSteamud")).Returns(_realUser);


            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>.That.Matches(user => user.Id == _realUser.Id)))
                .Returns(_realUsersItems);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>.That.Matches(user => user.Id == _scammerUser.Id)))
                .Returns(_scammerUsersItems);

            A.CallTo(() => _fakedGrpcServiceFactory.GetSteamServiceClient(A<ISteamInventoryCacheManager>._)).Returns(_fakedSteamServiceClient);

            _steamService = new SteamService(
                _fakedGrpcServiceFactory,
                _fakeRepoServiceFacotry,
                _fakedBotService,
                A.Dummy<ILogServiceFactory>(),
                _fakedOfferService,
                A.Dummy<ISteamInventoryCacheManager>()
            );
        }

        [Fact]
        public async void UserDepositSuccess()
        {
            var listOfDepositItems = new List<Item>
            {
                new Item {AssetId = "assetId1", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                new Item {AssetId = "assetId2", AppId = 730, ContextId = "2", MarketHashName = "weapon2"},
                new Item {AssetId = "assetId3", AppId = 730, ContextId = "2", MarketHashName = "weapon3"},
                new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
            };

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("weapon1", new decimal(1), "730", "2", "img",true, 1),
                new DatabaseModel.ItemDescription("weapon2", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon3", new decimal(3), "730", "2", "img",true, 3),
            };

            var botStack = new Stack<DatabaseModel.Bot>();
            botStack.Push(new DatabaseModel.Bot("botSteamId", "botName"));

            var steamInventory = new GetPlayerSteamInventoryResponse
            {
                PlayerInventory = new PlayerResponseItems
                {
                    Items = {listOfDepositItems}
                }
            };
            A.CallTo(() => _fakedSteamServiceClient.GetPlayerSteamInventoryAsync(A<GetPlayerSteamInventoryRequest>._)).Returns(steamInventory);


            A.CallTo(() => _fakedSettingsRepoService.GetSettingsAsync()).Returns(new DatabaseModel.Settings(10, 0, 0, DateTime.Today, 20));
            A.CallTo(() => _fakedBotService.GetAvalibleBotsForDeposit(A<DatabaseModel.User>._, A<List<DatabaseModel.Item>>._)).Returns(botStack);
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).Returns(itemDescriptions);

            var respose = await _steamService.MakeDepositOfferAsync(_realUser.SteamId, listOfDepositItems);

            Assert.Equal(1, respose.Count);

            A.CallTo(() => _fakedSteamServiceClient.MakeOfferAsync(A<MakeOfferRequest>.That.Matches(
                    offer => offer.SendItems       == false
                             && offer.Items.Count  == 4
                             && offer.User.SteamId == _realUser.SteamId
                             && offer.BotName      == "botName"
                )))
                .MustHaveHappened();
        }

        [Fact]
        public async void UserWithdrawSuccess()
        {
            var listOfAssetAndDescriptionId = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId2", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 2},
            };

            _realUsersItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 2, 5, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 2, 5, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 2, 5, _realUser.Id, DateTimeOffset.Now),
            };

            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc5", new decimal(5.55), "730", "2", "imgUlr",true, 2),
                new DatabaseModel.ItemDescription("itemDesc5", new decimal(5.55), "730", "2", "imgUlr",true, 2),
                new DatabaseModel.ItemDescription("itemDesc5", new decimal(5.55), "730", "2", "imgUlr",true, 2),
            });

            A.CallTo(() => _fakedSteamServiceClient.MakeOfferAsync(A<MakeOfferRequest>._)).Returns(new MakeOfferResponse
            {
                Offer = new Offer
                {
                    SteamOffer = new SteamOffer
                    {
                        Id = "staemOfferId"
                    }
                }
            });

            A.CallTo(() => _fakedOfferService.PrepareWithdrawlSteamOffer(
                A<List<Item>>.That.Matches(list => list.Count           == 3),
                A<DatabaseModel.Bot>.That.Matches(bot => bot.SteamId    == "botSteamId"),
                A<DatabaseModel.User>.That.Matches(user => user.SteamId == _realUser.SteamId)
            )).Returns(new DatabaseModel.OfferTransaction(0, 0, Decimal.One, false, null, null, 10));

            var offers = new Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>();
            offers.Add(new DatabaseModel.Bot("botSteamId", "botName", 5), _realUsersItems);

            A.CallTo(() => _fakedBotService.GetBotsWithWithdrawItems(listOfAssetAndDescriptionId)).Returns(offers);

            var respose = await _steamService.MakeWithdrawOfferAsync(_realUser.SteamId, listOfAssetAndDescriptionId);

            Assert.Equal(1, respose.Count);

            A.CallTo(() => _fakedSteamServiceClient.MakeOfferAsync(A<MakeOfferRequest>.That.Matches(
                    offer => offer.SendItems
                             && offer.Items.Count  == 3
                             && offer.User.SteamId == _realUser.SteamId
                             && offer.BotName == "botName"
                             )))
                .MustHaveHappened();

            A.CallTo(() => _fakedOfferTransactionRepoService.AddSteamIdToOffer(10, "staemOfferId")).MustHaveHappened();

            A.CallTo(() => _fakedOfferService.PrepareWithdrawlSteamOffer(
                A<List<Item>>.That.Matches(list => list.Count           == 3),
                A<DatabaseModel.Bot>.That.Matches(bot => bot.SteamId    == "botSteamId"),
                A<DatabaseModel.User>.That.Matches(user => user.SteamId == _realUser.SteamId)
            )).MustHaveHappened();
        }

        [Fact]
        public async void UserWithdrawsSuccessButSteamBotReturnsError()
        {
            var listOfAssetAndDescriptionId = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId2", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 2},
            };

            _realUsersItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 2, 5, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 2, 5, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 2, 5, _realUser.Id, DateTimeOffset.Now),
            };

            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<int>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("itemDesc5", new decimal(5.55), "730", "2", "imgUlr",true, 2),
                new DatabaseModel.ItemDescription("itemDesc5", new decimal(5.55), "730", "2", "imgUlr",true, 2),
                new DatabaseModel.ItemDescription("itemDesc5", new decimal(5.55), "730", "2", "imgUlr",true, 2),
            });

            A.CallTo(() => _fakedSteamServiceClient.MakeOfferAsync(A<MakeOfferRequest>._)).Returns(new MakeOfferResponse
            {
                Error = new Error
                {
                    Message = "Error!!"
                }
            });

            A.CallTo(() => _fakedOfferService.PrepareWithdrawlSteamOffer(
                A<List<Item>>.That.Matches(list => list.Count           == 3),
                A<DatabaseModel.Bot>.That.Matches(bot => bot.SteamId    == "botSteamId"),
                A<DatabaseModel.User>.That.Matches(user => user.SteamId == _realUser.SteamId)
            )).Returns(new DatabaseModel.OfferTransaction(0, 0, Decimal.One, false, null, null, 10));


            var offers = new Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>();
            offers.Add(new DatabaseModel.Bot("botSteamId", "botName", 5), _realUsersItems);

            A.CallTo(() => _fakedBotService.GetBotsWithWithdrawItems(listOfAssetAndDescriptionId)).Returns(offers);

            var respose = await _steamService.MakeWithdrawOfferAsync(_realUser.SteamId, listOfAssetAndDescriptionId);

            Assert.Equal(1, respose.Count);

            A.CallTo(() => _fakedSteamServiceClient.MakeOfferAsync(A<MakeOfferRequest>.That.Matches(
                    offer => offer.SendItems
                             && offer.Items.Count  == 3
                             && offer.User.SteamId == _realUser.SteamId
                             && offer.BotName == "botName"
                             )))
                .MustHaveHappened();

            A.CallTo(() => _fakedOfferTransactionRepoService.Remove(10)).MustHaveHappened();
            A.CallTo(() => _fakedOfferService.PrepareWithdrawlSteamOffer(
                A<List<Item>>.That.Matches(list => list.Count           == 3),
                A<DatabaseModel.Bot>.That.Matches(bot => bot.SteamId    == "botSteamId"),
                A<DatabaseModel.User>.That.Matches(user => user.SteamId == _realUser.SteamId)
            )).MustHaveHappened();
        }

        [Fact]
        public async void UserSendsWithdrawOnItemsThatHeDoesNotOwnThrows()
        {
            var listOfAssetAndDescriptionId = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId2", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId3", DescriptionId = 2},
            };

            _realUsersItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 2, 5, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 2, 5, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 2, 5, _realUser.Id, DateTimeOffset.Now),
            };

            var offers = new Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>();
            offers.Add(new DatabaseModel.Bot("botSteamId", "botName", 5), _realUsersItems);

            A.CallTo(() => _fakedBotService.GetBotsWithWithdrawItems(listOfAssetAndDescriptionId)).Returns(offers);

            await Assert.ThrowsAsync<UserDoesNotOwnThisItemsException>(async () =>
                await _steamService.MakeWithdrawOfferAsync(_scammerUser.SteamId, listOfAssetAndDescriptionId));

            A.CallTo(() => _fakedSteamServiceClient.MakeOfferAsync(A<MakeOfferRequest>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedOfferService.PrepareWithdrawlSteamOffer(A<List<Item>>._, A<DatabaseModel.Bot>._, A<DatabaseModel.User>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async void UserDepositsItemThatIsNotInItemDescriotionThrows()
        {
            var repositItems = new List<Item>
            {
                new Item {AssetId = "assetId1", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                new Item {AssetId = "assetId2", AppId = 730, ContextId = "2", MarketHashName = "weapon2"},
                new Item {AssetId = "assetId3", AppId = 730, ContextId = "2", MarketHashName = "weapon3"},
                new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
            };

            var steamInventory = new GetPlayerSteamInventoryResponse
            {
                PlayerInventory = new PlayerResponseItems
                {
                    Items = {repositItems}
                }
            };
            A.CallTo(() => _fakedSteamServiceClient.GetPlayerSteamInventoryAsync(A<GetPlayerSteamInventoryRequest>._)).Returns(steamInventory);

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("weapon2", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon3", new decimal(3), "730", "2", "img",true, 3),
            };

            var botStack = new Stack<DatabaseModel.Bot>();
            botStack.Push(new DatabaseModel.Bot("botSteamId", "botName"));

            A.CallTo(() => _fakedBotService.GetAvalibleBotsForDeposit(A<DatabaseModel.User>._, A<List<DatabaseModel.Item>>._)).Returns(botStack);
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<ItemDescriptionNotInDatabase>(async () =>
                await _steamService.MakeDepositOfferAsync(_realUser.SteamId, repositItems));
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).MustHaveHappened();
        }

        [Fact]
        public async void UserTriesToManipulateDepositOfferThrows()
        {
            var repositItems = new List<Item>
            {
                new Item {AssetId = "assetId1", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                new Item {AssetId = "assetId2", AppId = 730, ContextId = "2", MarketHashName = "weapon2"},
                new Item {AssetId = "assetId3", AppId = 730, ContextId = "2", MarketHashName = "weapon3"},
                new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
            };

            var steamInventory = new GetPlayerSteamInventoryResponse
            {
                PlayerInventory = new PlayerResponseItems
                {
                    Items =
                    {
                        new Item {AssetId = "assetId11", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                        new Item {AssetId = "assetId22", AppId = 730, ContextId = "2", MarketHashName = "weapon22"},
                        new Item {AssetId = "assetId33", AppId = 730, ContextId = "2", MarketHashName = "weapon33"},
                        new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon11"}
                    }
                }
            };
            A.CallTo(() => _fakedSteamServiceClient.GetPlayerSteamInventoryAsync(A<GetPlayerSteamInventoryRequest>._)).Returns(steamInventory);

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("weapon1", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon11", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon2", new decimal(3), "730", "2", "img",true, 3),
                new DatabaseModel.ItemDescription("weapon22", new decimal(3), "730", "2", "img",true, 3),
                new DatabaseModel.ItemDescription("weapon3", new decimal(3), "730", "2", "img",true, 3),
                new DatabaseModel.ItemDescription("weapon33", new decimal(3), "730", "2", "img",true, 3),
            };

            var botStack = new Stack<DatabaseModel.Bot>();
            botStack.Push(new DatabaseModel.Bot("botSteamId", "botName"));

            A.CallTo(() => _fakedBotService.GetAvalibleBotsForDeposit(A<DatabaseModel.User>._, A<List<DatabaseModel.Item>>._)).Returns(botStack);
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<HaxxorKidIsInTheHouseExeption>(async () =>
                await _steamService.MakeDepositOfferAsync(_realUser.SteamId, repositItems));
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).MustNotHaveHappened();
        }

        [Fact]
        public async void UserDepositValueLessThenAcceptedLimitThrows()
        {
            var listOfDepositItems = new List<Item>
            {
                new Item {AssetId = "assetId1", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                new Item {AssetId = "assetId2", AppId = 730, ContextId = "2", MarketHashName = "weapon2"},
                new Item {AssetId = "assetId3", AppId = 730, ContextId = "2", MarketHashName = "weapon3"},
                new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
            };

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("weapon1", new decimal(1), "730", "2", "img",true, 1),
                new DatabaseModel.ItemDescription("weapon2", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon3", new decimal(3), "730", "2", "img",true, 3),
            };

            var steamInventory = new GetPlayerSteamInventoryResponse
            {
                PlayerInventory = new PlayerResponseItems
                {
                    Items = {listOfDepositItems}
                }
            };
            A.CallTo(() => _fakedSteamServiceClient.GetPlayerSteamInventoryAsync(A<GetPlayerSteamInventoryRequest>._)).Returns(steamInventory);

            var botStack = new Stack<DatabaseModel.Bot>();
            botStack.Push(new DatabaseModel.Bot("botSteamId", "botName"));
            A.CallTo(() => _fakedSettingsRepoService.GetSettingsAsync())
                .Returns(new DatabaseModel.Settings(0, new decimal(2), 0, DateTime.Today, 20));

            A.CallTo(() => _fakedBotService.GetAvalibleBotsForDeposit(A<DatabaseModel.User>._, A<List<DatabaseModel.Item>>._)).Returns(botStack);
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<ItemInOfferNotMatchingLowestValueRuleExecption>(async () =>
                await _steamService.MakeDepositOfferAsync(_realUser.SteamId, listOfDepositItems));
        }

        [Fact]
        public async Task UserHasToManyItemInOurBotInventoryThrows()
        {
            var listOfDepositItems = new List<Item>
            {
                new Item {AssetId = "assetId1", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                new Item {AssetId = "assetId2", AppId = 730, ContextId = "2", MarketHashName = "weapon2"},
                new Item {AssetId = "assetId3", AppId = 730, ContextId = "2", MarketHashName = "weapon3"},
                new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
            };

            var steamInventory = new GetPlayerSteamInventoryResponse
            {
                PlayerInventory = new PlayerResponseItems
                {
                    Items = {listOfDepositItems}
                }
            };
            A.CallTo(() => _fakedSteamServiceClient.GetPlayerSteamInventoryAsync(A<GetPlayerSteamInventoryRequest>._)).Returns(steamInventory);

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("weapon1", new decimal(1), "730", "2", "img",true, 1),
                new DatabaseModel.ItemDescription("weapon2", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon3", new decimal(3), "730", "2", "img",true, 3),
            };

            var botStack = new Stack<DatabaseModel.Bot>();
            botStack.Push(new DatabaseModel.Bot("botSteamId", "botName"));
            A.CallTo(() => _fakedSettingsRepoService.GetSettingsAsync())
                .Returns(new DatabaseModel.Settings(2, new decimal(0), 0, DateTime.Today, 20));

            A.CallTo(() => _fakedBotService.GetAvalibleBotsForDeposit(A<DatabaseModel.User>._, A<List<DatabaseModel.Item>>._)).Returns(botStack);
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<InventoryLimitExceeded>(async () =>
                await _steamService.MakeDepositOfferAsync(_realUser.SteamId, listOfDepositItems));
        }

        [Fact]
        public async Task UserHasToManyItemInOurBotInventoryAndDepositThrows()
        {
            _realUsersItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId11", 2, 1, _realUser.Id, DateTimeOffset.Now),
            };
            var listOfDepositItems = new List<Item>
            {
                new Item {AssetId = "assetId1", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
                new Item {AssetId = "assetId2", AppId = 730, ContextId = "2", MarketHashName = "weapon2"},
                new Item {AssetId = "assetId3", AppId = 730, ContextId = "2", MarketHashName = "weapon3"},
                new Item {AssetId = "assetId4", AppId = 730, ContextId = "2", MarketHashName = "weapon1"},
            };

            var itemDescriptions = new List<DatabaseModel.ItemDescription>
            {
                new DatabaseModel.ItemDescription("weapon1", new decimal(1), "730", "2", "img",true, 1),
                new DatabaseModel.ItemDescription("weapon2", new decimal(2), "730", "2", "img",true, 2),
                new DatabaseModel.ItemDescription("weapon3", new decimal(3), "730", "2", "img",true, 3),
            };

            var steamInventory = new GetPlayerSteamInventoryResponse
            {
                PlayerInventory = new PlayerResponseItems
                {
                    Items = {listOfDepositItems}
                }
            };
            A.CallTo(() => _fakedSteamServiceClient.GetPlayerSteamInventoryAsync(A<GetPlayerSteamInventoryRequest>._)).Returns(steamInventory);


            var botStack = new Stack<DatabaseModel.Bot>();
            botStack.Push(new DatabaseModel.Bot("botSteamId", "botName"));
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>.That.Matches(user => user.Id == _realUser.Id)))
                .Returns(_realUsersItems);
            A.CallTo(() => _fakedSettingsRepoService.GetSettingsAsync())
                .Returns(new DatabaseModel.Settings(10, new decimal(0), 0, DateTime.Today, 20));

            A.CallTo(() => _fakedBotService.GetAvalibleBotsForDeposit(A<DatabaseModel.User>._, A<List<DatabaseModel.Item>>._)).Returns(botStack);
            A.CallTo(() => _fakedItemDescRepoService.FindAsync(A<List<string>>._)).Returns(itemDescriptions);

            await Assert.ThrowsAsync<InventoryLimitExceeded>(async () =>
                await _steamService.MakeDepositOfferAsync(_realUser.SteamId, listOfDepositItems));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using RpcCommunication;
using Xunit;
using Item = RpcCommunication.Item;

namespace Betting.Backend.Test
{
    public class OfferTransactionServiceTest
    {
        private readonly IItemInOfferTransactionRepoService _fakedItemInOfferTransactionRepoService;
        private readonly IOfferTranascrionRepoService       _fakedOfferTranascrionRepoService;
        private readonly IUserRepoService                   _fakedUserRepoService;
        private readonly IBotRepoService                    _fakedBotRepoService;
        private readonly ITransactionWrapper                       _fakedTransactionWrapper;
        private readonly OfferService                       _offerService;
        private readonly OfferStatusRequest                 _offerMinmalInfo;

        private IItemRepoService _fakedItemRepoService;


        public OfferTransactionServiceTest()
        {
            var fakedItemDescriptionRepoService = A.Fake<IItemDescriptionRepoService>();
            var fakedTransactionFactory         = A.Fake<ITransactionFactory>();
            var fakedRepoServiceFactory         = A.Fake<IRepoServiceFactory>();

            _fakedItemInOfferTransactionRepoService = A.Fake<IItemInOfferTransactionRepoService>();
            _fakedOfferTranascrionRepoService       = A.Fake<IOfferTranascrionRepoService>();
            _fakedUserRepoService                   = A.Fake<IUserRepoService>();
            _fakedBotRepoService                    = A.Fake<IBotRepoService>();
            _fakedItemRepoService                   = A.Fake<IItemRepoService>();

            _fakedTransactionWrapper = A.Fake<ITransactionWrapper>();

            A.CallTo(() => fakedRepoServiceFactory.ItemInOfferTransactionRepoService).Returns(_fakedItemInOfferTransactionRepoService);
            A.CallTo(() => fakedRepoServiceFactory.OfferTranascrionRepoService).Returns(_fakedOfferTranascrionRepoService);
            A.CallTo(() => fakedRepoServiceFactory.UserRepoService).Returns(_fakedUserRepoService);
            A.CallTo(() => fakedRepoServiceFactory.BotRepoService).Returns(_fakedBotRepoService);
            A.CallTo(() => fakedRepoServiceFactory.ItemDescriptionRepoService).Returns(fakedItemDescriptionRepoService);
            A.CallTo(() => fakedRepoServiceFactory.ItemRepoService).Returns(_fakedItemRepoService);
            A.CallTo(() => fakedTransactionFactory.BeginTransaction()).Returns(_fakedTransactionWrapper);

            _offerMinmalInfo = new OfferStatusRequest
            {
                Bot = new Bot
                {
                    Username = "botName",
                    SteamId  = "botSteamId"
                },
                SteamId       = "userSteamId",
                StatusCode    = int.MinValue,
                StatusMessage = "",
                OfferSend     = new OfferStatusOffer
                {
                    SteamOffer = new SteamOffer
                    {
                        ItemsToGive =
                        {
                            new Item {AppId = 730, ContextId = "2", AssetId = "1", MarketHashName = "SomeWeapon1"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "2", MarketHashName = "SomeWeapon1"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "3", MarketHashName = "SomeWeapon2"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "4", MarketHashName = "SomeWeapon3"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "5", MarketHashName = "SomeWeapon4"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "6", MarketHashName = "SomeWeapon2"},
                        },
                        ItemsToReceive =
                        {
                            new Item {AppId = 730, ContextId = "2", AssetId = "11", MarketHashName = "SomeWeapon1"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "12", MarketHashName = "SomeWeapon1"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "13", MarketHashName = "SomeWeapon2"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "14", MarketHashName = "SomeWeapon3"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "15", MarketHashName = "SomeWeapon4"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "16", MarketHashName = "SomeWeapon2"},
                        },
                    }
                }
            };


            var someWeapon1 = new DatabaseModel.ItemDescription("SomeWeapon1", new decimal(11.22), "720", "2", "imgUrl",true, 1);
            var someWeapon2 = new DatabaseModel.ItemDescription("SomeWeapon2", new decimal(45.5), "720", "2", "imgUrl",true, 2);
            var someWeapon3 = new DatabaseModel.ItemDescription("SomeWeapon3", new decimal(78.00), "720", "2", "imgUrl",true, 3);
            var someWeapon4 = new DatabaseModel.ItemDescription("SomeWeapon4", new decimal(5.47), "720", "2", "imgUrl",true, 4);

            A.CallTo(() => fakedItemDescriptionRepoService.FindAsync("SomeWeapon1")).Returns(someWeapon1);
            A.CallTo(() => fakedItemDescriptionRepoService.FindAsync("SomeWeapon2")).Returns(someWeapon2);
            A.CallTo(() => fakedItemDescriptionRepoService.FindAsync("SomeWeapon3")).Returns(someWeapon3);
            A.CallTo(() => fakedItemDescriptionRepoService.FindAsync("SomeWeapon4")).Returns(someWeapon4);

            A.CallTo(() => fakedItemDescriptionRepoService.FindAsync(A<List<string>>._)).Returns(new List<DatabaseModel.ItemDescription>
            {
                someWeapon1,
                someWeapon2,
                someWeapon3,
                someWeapon4,
            });


            _offerService = new OfferService(fakedRepoServiceFactory, fakedTransactionFactory, A.Dummy<ILogServiceFactory>());
        }


        [Fact]
        public async Task UserDepositItemsSuccess()
        {
            _offerMinmalInfo.OfferSend.SteamOffer.ItemsToGive.Clear();

            var offerTransInsertWithId = new DatabaseModel.OfferTransaction(1, 14, new decimal(196.91), true, "12345", DateTime.Today, 75);

            var userInsertRes = new DatabaseModel.User("userSteamId", "name", "img", "tradeLink", DateTime.Today, DateTime.Today, false,null, 1);
            A.CallTo(() => _fakedUserRepoService.FindAsync("userSteamId")).Returns(userInsertRes);

            var botLookRes = Task.FromResult(new DatabaseModel.Bot("botSteamId", "botName", 14));
            A.CallTo(() => _fakedBotRepoService.FindAsync("botSteamId")).Returns(botLookRes);

            A.CallTo(() => _fakedOfferTranascrionRepoService.InsertAsync(A<DatabaseModel.OfferTransaction>.That.Matches(o =>
                o.IsDeposit && o.Accepted > DateTime.Today && o.BotId == 14 && o.UserId == 1 && o.Id == 0 && o.TotalValue == new decimal(196.91)
            ), A<ITransactionWrapper>._)).Returns(offerTransInsertWithId);

            await _offerService.DepositSteamOfferAsync(_offerMinmalInfo);

            A.CallTo(() => _fakedOfferTranascrionRepoService.InsertAsync(A<DatabaseModel.OfferTransaction>.That.Matches(o =>
                o.IsDeposit && o.Accepted > DateTime.Today && o.BotId == 14 && o.UserId == 1 && o.Id == 0 && o.TotalValue == new decimal(196.91)
            ), A<ITransactionWrapper>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemInOfferTransactionRepoService.InsertAsync(A<List<DatabaseModel.ItemInOfferTransaction>>.That.Matches(i =>
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 1 && o.Value == new decimal(11.22)).ToList().Count == 2 &&
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 3 && o.Value == new decimal(78.00)).ToList().Count == 1 &&
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 4 && o.Value == new decimal(5.47)).ToList().Count  == 1 &&
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 2 && o.Value == new decimal(45.5)).ToList().Count  == 2
            ), A<ITransactionWrapper>._)).MustHaveHappened();

            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustHaveHappened();
        }

        [Fact]
        public async Task UserDepositItemsTransactionThrows()
        {
            _offerMinmalInfo.OfferSend.SteamOffer.ItemsToGive.Clear();

            var offerTransInsertWithId = new DatabaseModel.OfferTransaction(1, 14, new decimal(196.91), true, "12347", DateTime.Today, 75);

            var userInsertRes = new DatabaseModel.User("userSteamId", "name", "img", "tradeLink", DateTime.Today, DateTime.Today, false,null, 1);
            A.CallTo(() => _fakedUserRepoService.FindAsync("userSteamId")).Returns(userInsertRes);

            var botLookRes = Task.FromResult(new DatabaseModel.Bot("botSteamId", "botName", 14));
            A.CallTo(() => _fakedBotRepoService.FindAsync("botSteamId")).Returns(botLookRes);

            A.CallTo(() => _fakedOfferTranascrionRepoService.InsertAsync(A<DatabaseModel.OfferTransaction>._, A<ITransactionWrapper>._))
                .Throws(new CantCompleteTransaction(new System.Exception()));

            await Assert.ThrowsAsync<CantCompleteSteamDeposit>(async () => await _offerService.DepositSteamOfferAsync(_offerMinmalInfo));

            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustNotHaveHappened();
            A.CallTo(() => _fakedTransactionWrapper.Rollback()).MustHaveHappened();
        }

        [Fact]
        public async Task UserPrepareWithdrawalItemsSuccess()
        {
            _offerMinmalInfo.OfferSend.SteamOffer.ItemsToReceive.Clear();
            _offerMinmalInfo.OfferSend.SteamOffer.Id = "12346";

            var offerTransInsertWithId = new DatabaseModel.OfferTransaction(1, 14, new decimal(196.91), false, "12346", DateTime.Today, 75);
            var bot                    = new DatabaseModel.Bot("botSteamId", "botName", 14);

            var userInsertRes = new DatabaseModel.User("userSteamId", "name", "img", "tradeLink", DateTime.Today, DateTime.Today, false,null, 1);
            A.CallTo(() => _fakedUserRepoService.FindAsync("userSteamId")).Returns(userInsertRes);

            var botLookRes = Task.FromResult(bot);
            A.CallTo(() => _fakedBotRepoService.FindAsync("botSteamId")).Returns(botLookRes);

            A.CallTo(() => _fakedOfferTranascrionRepoService.InsertAsync(A<DatabaseModel.OfferTransaction>.That.Matches(o =>
                !o.IsDeposit && o.Accepted == null && o.BotId == 14 && o.UserId == 1 && o.Id == 0 && o.TotalValue == new decimal(196.91)
            ), A<ITransactionWrapper>._)).Returns(offerTransInsertWithId);

            await _offerService.PrepareWithdrawlSteamOffer(_offerMinmalInfo.OfferSend.SteamOffer.ItemsToGive.ToList(), bot, userInsertRes);

            A.CallTo(() => _fakedOfferTranascrionRepoService.InsertAsync(A<DatabaseModel.OfferTransaction>.That.Matches(o =>
                !o.IsDeposit && o.Accepted == null && o.BotId == 14 && o.UserId == 1 && o.Id == 0 && o.TotalValue == new decimal(196.91)
            ), A<ITransactionWrapper>._)).MustHaveHappened();
            A.CallTo(() => _fakedItemInOfferTransactionRepoService.InsertAsync(A<List<DatabaseModel.ItemInOfferTransaction>>.That.Matches(i =>
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 1 && o.Value == new decimal(11.22)).ToList().Count == 2 &&
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 3 && o.Value == new decimal(78.00)).ToList().Count == 1 &&
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 4 && o.Value == new decimal(5.47)).ToList().Count  == 1 &&
                i.Where(o => o.OfferTransactionId == 75 && o.ItemDescriptionId == 2 && o.Value == new decimal(45.5)).ToList().Count  == 2
            ), A<ITransactionWrapper>._)).MustHaveHappened();

            A.CallTo(() => _fakedTransactionWrapper.Commit()).MustHaveHappened();
        }

        [Fact]
        public async Task UserWithdrawlOfferWasCanceled()
        {
            _offerMinmalInfo.OfferSend.SteamOffer.ItemsToReceive.Clear();
            _offerMinmalInfo.OfferSend.SteamOffer.Id = "12346";

            A.CallTo(() => _fakedOfferTranascrionRepoService.FindAsync("12346"))
                .Returns(new DatabaseModel.OfferTransaction(0, 0, 0, false, "123456", null, 10));
            await _offerService.RemoveCanceledWithdrawalSteamOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedOfferTranascrionRepoService.Remove(10)).MustHaveHappened();
            A.CallTo(() => _fakedItemInOfferTransactionRepoService.Remove(10)).MustHaveHappened();
            A.CallTo(() => _fakedOfferTranascrionRepoService.FindAsync("12346")).MustHaveHappened();
        }

        [Fact]
        public async Task UserWithdrawalItemsSuccess()
        {
            _offerMinmalInfo.OfferSend.SteamOffer.ItemsToReceive.Clear();
            _offerMinmalInfo.OfferSend.SteamOffer.Id = "12346";

            var offerTransInsertWithId = new DatabaseModel.OfferTransaction(1, 14, new decimal(196.91), false, "12346", DateTime.Today, 75);

            var userInsertRes = new DatabaseModel.User("userSteamId", "name", "img", "tradeLink", DateTime.Today, DateTime.Today, false,null, 1);
            A.CallTo(() => _fakedUserRepoService.FindAsync("userSteamId")).Returns(userInsertRes);

            var botLookRes = Task.FromResult(new DatabaseModel.Bot("botSteamId", "botName", 14));
            A.CallTo(() => _fakedBotRepoService.FindAsync("botSteamId")).Returns(botLookRes);

            A.CallTo(() => _fakedOfferTranascrionRepoService.FindAsync("12346")).Returns(offerTransInsertWithId);

            await _offerService.WithdrawalSteamOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedOfferTranascrionRepoService.AddAcceptedTimesptampToOffer(A<DateTime>._, 75)).MustHaveHappened();

            A.CallTo(() => _fakedItemRepoService.DeleteAsync(A<List<DatabaseModel.Item>>._)).MustHaveHappened();
        }
    }
}
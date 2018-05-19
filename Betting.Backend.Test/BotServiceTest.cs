using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using FakeItEasy.Core;
using Xunit;

namespace Betting.Backend.Test
{
    public class BotServiceTest
    {
        private readonly IBotRepoService     _fakedBotRepoService;
        private readonly IRepoServiceFactory _fakedRepoServiceFactory;
        private readonly IItemRepoService    _fakedItemRepoService;
        private          BotService          _botService;

        public BotServiceTest()
        {
            _fakedBotRepoService     = A.Fake<IBotRepoService>();
            _fakedItemRepoService    = A.Fake<IItemRepoService>();
            _fakedRepoServiceFactory = A.Fake<IRepoServiceFactory>();

            A.CallTo(() => _fakedRepoServiceFactory.ItemRepoService).Returns(_fakedItemRepoService);
            A.CallTo(() => _fakedRepoServiceFactory.BotRepoService).Returns(_fakedBotRepoService);

            _botService = new BotService(_fakedRepoServiceFactory);
        }


        [Fact]
        public async Task GetBotsAndItemsForWithdrawSuccess()
        {
            //this will lookup where his items is and create a new offer for every bot that the item is in.

            var assetAndDescIds = new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 1},
                new AssetAndDescriptionId {AssetId = "assetId2", DescriptionId = 3},
                new AssetAndDescriptionId {AssetId = "assetId1", DescriptionId = 2},
                new AssetAndDescriptionId {AssetId = "assetId5", DescriptionId = 1},
            };

            var databaseItem = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 1, -1, DateTimeOffset.Now, -1),
                new DatabaseModel.Item("assetId2", 3, 2, -1, DateTimeOffset.Now, -1),
                new DatabaseModel.Item("assetId1", 2, 3, -1, DateTimeOffset.Now, -1),
                new DatabaseModel.Item("assetId5", 1, 1, -1, DateTimeOffset.Now, -1),
            };

            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(databaseItem);
            A.CallTo(() => _fakedBotRepoService.FindAsync(A<List<int>>._)).ReturnsLazily(CreateBots);

            var dict = await _botService.GetBotsWithWithdrawItems(assetAndDescIds);

            Assert.Equal(3, dict.Count);
            Assert.Equal(2, dict.First(kvp => kvp.Key.Id == 1).Value.Count);
            Assert.Equal(1, dict.First(kvp => kvp.Key.Id == 2).Value.Count);
            Assert.Equal(1, dict.First(kvp => kvp.Key.Id == 3).Value.Count);
        }

        [Fact]
        public async Task GetAvalibleBotsForDepositSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imgUrl", "tade", DateTime.Now, DateTime.Now, false, null, 10);

            var allItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 2, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 1, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId5", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId6", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId7", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId8", 1, 1, 4, DateTimeOffset.Now),
            };

            var usersItem = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 2, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 1, user.Id, DateTimeOffset.Now),
            };

            A.CallTo(() => _fakedItemRepoService.GetAll()).Returns(allItems);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>._))
                .Returns(allItems.Where(item => item.OwnerId == user.Id).ToList());
            A.CallTo(() => _fakedBotRepoService.FindAsync(2)).ReturnsLazily(call => Task.FromResult(CreateBot((int) call.Arguments[0])));
            A.CallTo(() => _fakedBotRepoService.FindAsync(A<List<int>>._)).ReturnsLazily(CreateBots);

            var bots = await _botService.GetAvalibleBotsForDeposit(user, usersItem);

            Assert.Equal(4, bots.Count);
            Assert.Equal(3, bots.Pop().Id);
            var botIdTemp = bots.Pop().Id;
            Assert.True(botIdTemp == 2 || botIdTemp == 1);
            var botIdTemp1 = bots.Pop().Id;
            Assert.True(botIdTemp1 == 2 || botIdTemp1 == 1);
            Assert.Equal(2, bots.Pop().Id);

            A.CallTo(() => _fakedBotRepoService.FindAsync(2)).MustHaveHappened();
        }

        [Fact]
        public async Task GetAvalibleBotsForDepositOtherInfoSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imgUrl", "tade", DateTime.Now, DateTime.Now, false, null, 10);

            var allItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 7, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId5", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId6", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId7", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId8", 1, 1, 4, DateTimeOffset.Now),
            };

            var usersItem = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 3, user.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 3, user.Id, DateTimeOffset.Now),
            };

            A.CallTo(() => _fakedItemRepoService.GetAll()).Returns(allItems);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>._))
                .Returns(allItems.Where(item => item.OwnerId == user.Id).ToList());
            A.CallTo(() => _fakedBotRepoService.FindAsync(7)).ReturnsLazily(call => Task.FromResult(CreateBot((int) call.Arguments[0])));
            A.CallTo(() => _fakedBotRepoService.FindAsync(A<List<int>>._)).ReturnsLazily(CreateBots);

            var bots = await _botService.GetAvalibleBotsForDeposit(user, usersItem);

            Assert.Equal(2, bots.Count);
            Assert.Equal(3, bots.Pop().Id);
            Assert.Equal(7, bots.Pop().Id);

            A.CallTo(() => _fakedBotRepoService.FindAsync(7)).MustHaveHappened();
        }
        
        [Fact]
        public async Task GetAvalibleBotsForDepositUserHasNoPrevItemsSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imgUrl", "tade", DateTime.Now, DateTime.Now, false, null, 10);

            var allItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 3, 6, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 3, 6, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 3, 6, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 7, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId5", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId6", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId7", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId8", 1, 1, 4, DateTimeOffset.Now),
            };

            A.CallTo(() => _fakedItemRepoService.GetAll()).Returns(allItems);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>._))
                .Returns(allItems.Where(item => item.OwnerId == user.Id).ToList());
            A.CallTo(() => _fakedBotRepoService.FindAsync(7)).ReturnsLazily(call => Task.FromResult(CreateBot((int) call.Arguments[0])));
            A.CallTo(() => _fakedBotRepoService.FindAsync(A<List<int>>._)).ReturnsLazily(CreateBots);

            var bots = await _botService.GetAvalibleBotsForDeposit(user, new List<DatabaseModel.Item>());

            Assert.Equal(1, bots.Count);
            Assert.Equal(7, bots.Pop().Id);

            A.CallTo(() => _fakedBotRepoService.FindAsync(7)).MustHaveHappened();
        }
        
        [Fact]
        public async Task GetAvalibleBotsForDepositUserBotHasNoSkinsSuccess()
        {
            var user = new DatabaseModel.User("steamId", "name", "imgUrl", "tade", DateTime.Now, DateTime.Now, false, null, 10);

            var allItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId1", 1, 3, 6, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId2", 1, 3, 6, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId3", 1, 3, 6, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId4", 1, 7, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId5", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId6", 1, 1, 5, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId7", 1, 1, 1, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId8", 1, 1, 4, DateTimeOffset.Now),
            };

            A.CallTo(() => _fakedItemRepoService.GetAll()).Returns(allItems);
            A.CallTo(() => _fakedItemRepoService.FindAsync(A<DatabaseModel.User>._))
                .Returns(allItems.Where(item => item.OwnerId == user.Id).ToList());
            A.CallTo(() => _fakedBotRepoService.FindAsync(154)).ReturnsLazily(call => Task.FromResult(CreateBot((int) call.Arguments[0])));
            A.CallTo(() => _fakedBotRepoService.FindAsync(A<List<int>>._)).ReturnsLazily(CreateBots);
            A.CallTo(() => _fakedBotRepoService.GetAll()).Returns(new List<DatabaseModel.Bot>
            {
                CreateBot(1),
                CreateBot(154),
                CreateBot(3),
                CreateBot(7),
            });

            var bots = await _botService.GetAvalibleBotsForDeposit(user, new List<DatabaseModel.Item>());

            Assert.Equal(1, bots.Count);
            Assert.Equal(154, bots.Pop().Id);

            A.CallTo(() => _fakedBotRepoService.FindAsync(154)).MustHaveHappened();
        }

        private Task<List<DatabaseModel.Bot>> CreateBots(IFakeObjectCall call)
        {
            var res = call.Arguments
                .SelectMany(arugment => (List<int>) arugment)
                .Select(CreateBot)
                .ToList();
            return Task.FromResult(res);
        }

        private DatabaseModel.Bot CreateBot(int id)
        {
            return new DatabaseModel.Bot("steamId", "name", id);
        }
    }
}
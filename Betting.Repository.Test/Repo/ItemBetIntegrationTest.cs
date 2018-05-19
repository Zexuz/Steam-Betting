using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Impl;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class ItemBettedRepoIntegrationTestSetup
    {
        public string              DatabaseName => "BettingTestItemBetted";
        public string              ConnectionString;
        public IDatabaseConnection Database { get; private set; }

        public DatabaseModel.User            User1                     { get; private set; }
        public DatabaseModel.User            User2                     { get; private set; }
        public DatabaseModel.User            User3                     { get; private set; }
        public DatabaseModel.Match           Match1                    { get; private set; }
        public DatabaseModel.Match           Match2                    { get; private set; }
        public DatabaseModel.Bot             BotId                     { get; private set; }
        public DatabaseModel.ItemDescription ItemDescription1          { get; private set; }
        public DatabaseModel.ItemDescription ItemDescription2          { get; private set; }
        public DatabaseModel.Item            Item1                     { get; private set; }
        public DatabaseModel.Item            Item2                     { get; private set; }
        public DatabaseModel.Item            Item3                     { get; private set; }
        public DatabaseModel.Item            Item4                     { get; private set; }
        public DatabaseModel.Item            Item5                     { get; private set; }
        public DatabaseModel.Item            Item6                     { get; private set; }
        public DatabaseModel.Item            Item7                     { get; private set; }
        public DatabaseModel.Item            Item8                     { get; private set; }
        public DatabaseModel.Item            Item9                     { get; private set; }
        public DatabaseModel.Item            Item10                    { get; private set; }
        public DatabaseModel.Item            Item11                    { get; private set; }
        public DatabaseModel.Item            Item12                    { get; private set; }
        public DatabaseModel.Item            Item13                    { get; private set; }
        public DatabaseModel.Item            Item14                    { get; private set; }
        public DatabaseModel.Item            Item15                    { get; private set; }
        public DatabaseModel.Bet             Bet1                      { get; private set; }
        public DatabaseModel.Bet             Bet2                      { get; private set; }
        public DatabaseModel.Bet             Bet3                      { get; private set; }
        public DatabaseModel.Bet             Bet4                      { get; private set; }
        public DatabaseModel.Bet             Bet5                      { get; private set; }
        public IDatabaseConnectionFactory    DatabaseConnectionFactory { get; }
        public DatabaseModel.GameMode        GameMode                  { get; set; }


        public ItemBettedRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();

            ConnectionString = new ConnectionStringsForTest().GetConnectionString(DatabaseName);
            Database         = new DatabaseConnection(ConnectionString);

            var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(Database);
            DatabaseConnectionFactory = fakedFactory;
            InitTest().Wait();
        }

        public async Task InitTest()
        {
            var userRepoService  = new UserRepoService(DatabaseConnectionFactory, new UserQueries());
            var matchRepoService = new MatchRepoService(DatabaseConnectionFactory, new MatchQueries());
            var betRepoService   = new BetRepoService(DatabaseConnectionFactory, new BetQueries());
            var itemRepoService  = new ItemRepoService(DatabaseConnectionFactory, new ItemQueries());

            GameMode = new DatabaseModel.GameMode("Jackpot", 1);
            await new GameModeRepoService(DatabaseConnectionFactory).Insert(GameMode);

            User1 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "987654321",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User2 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "4477654321",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User3 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "444784",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            Match1 = await matchRepoService.InsertAsync(new DatabaseModel.Match(
                11,
                "salt",
                "hash",
                47.5484.ToString(CultureInfo.InvariantCulture),
                1,
                null,
                null,
                1,
                GameMode.Id,
                DateTime.Now
            ));
            Match2 = await matchRepoService.InsertAsync(new DatabaseModel.Match(
                101,
                "salt",
                "hash",
                47.5484.ToString(CultureInfo.InvariantCulture),
                1,
                null,
                null,
                1,
                GameMode.Id,
                DateTime.Now
            ));

            var model1 =
                new DatabaseModel.ItemDescription("itemDescription itembet 2", new decimal(13.37), "730", "2", "imageUrl",true);
            var model2 =
                new DatabaseModel.ItemDescription("itemDescription itembet 1", new decimal(13.37), "730", "2", "imageUrl",true);
            var itemDescriptionRepoService = new ItemDescriptionRepoService(DatabaseConnectionFactory, new ItemDescriptionQueries());

            BotId =
                await new BotRepoService(DatabaseConnectionFactory, new BotQueries()).InsertAsync(new DatabaseModel.Bot("47489", "Bot ItemBet"));
            ItemDescription1 = await itemDescriptionRepoService.InsertAsync(model2);
            ItemDescription2 = await itemDescriptionRepoService.InsertAsync(model1);
            Item1            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 1", ItemDescription1.Id, BotId.Id, User1.Id, DateTimeOffset.Now));
            Item2            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 2", ItemDescription1.Id, BotId.Id, User1.Id, DateTimeOffset.Now));
            Item3            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 3", ItemDescription2.Id, BotId.Id, User1.Id, DateTimeOffset.Now));
            Item4            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 4", ItemDescription1.Id, BotId.Id, User1.Id, DateTimeOffset.Now));
            Item5            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 5", ItemDescription2.Id, BotId.Id, User1.Id, DateTimeOffset.Now));
            Item6            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 6", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item7            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 7", ItemDescription2.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item8            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 8", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item9            = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 9", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item10           = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 10", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item11           = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 11", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item12           = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 12", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item13           = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 13", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item14           = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 14", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Item15           = await itemRepoService.InsertAsync(new DatabaseModel.Item("itemBet 15", ItemDescription1.Id, BotId.Id, User2.Id, DateTimeOffset.Now));
            Bet1             = await betRepoService.InsertAsync(new DatabaseModel.Bet(User1.Id, Match1.Id,GameMode.Id, DateTime.Now));
            Bet2             = await betRepoService.InsertAsync(new DatabaseModel.Bet(User2.Id, Match1.Id,GameMode.Id, DateTime.Now));
            Bet3             = await betRepoService.InsertAsync(new DatabaseModel.Bet(User3.Id, Match1.Id,GameMode.Id, DateTime.Now));
            Bet4             = await betRepoService.InsertAsync(new DatabaseModel.Bet(User3.Id, Match2.Id,GameMode.Id, DateTime.Now));
            Bet5             = await betRepoService.InsertAsync(new DatabaseModel.Bet(User1.Id, Match2.Id,GameMode.Id, DateTime.Now));
        }
    }

    public class ItemBetIntegrationTest : IClassFixture<ItemBettedRepoIntegrationTestSetup>
    {
        private readonly ItemBettedRepoIntegrationTestSetup _setup;
        private readonly IDatabaseConnectionFactory         _databaseFactory;

        public ItemBetIntegrationTest(ItemBettedRepoIntegrationTestSetup setup)
        {
            _setup           = setup;
            _databaseFactory = setup.DatabaseConnectionFactory;
        }

        [Fact]
        public async void InsertItemBetSuccess()
        {
            var itemBet = new DatabaseModel.ItemBetted(_setup.Bet1.Id, _setup.Item1.DescriptionId, _setup.Item1.AssetId, new decimal(1.0));

            var res = await new ItemBettedRepoService(_databaseFactory, new ItemBetQueries()).InsertAsync(itemBet);

            Assert.Equal(itemBet.BetId, res.BetId);
            Assert.Equal(itemBet.AssetId, res.AssetId);
            Assert.Equal(itemBet.DescriptionId, res.DescriptionId);
        }

        [Fact]
        public async void InsertSameItemBetThrows()
        {
            var itemBet = new DatabaseModel.ItemBetted(_setup.Bet1.Id, _setup.Item5.DescriptionId, _setup.Item5.AssetId, new decimal(1.0));

            var itemBetRepo = new ItemBettedRepoService(_databaseFactory, new ItemBetQueries());
            await itemBetRepo.InsertAsync(itemBet);
            var exception1 = await Record.ExceptionAsync(async () => { await itemBetRepo.InsertAsync(itemBet); });
            Assert.IsType(typeof(SqlException), exception1);
        }

        [Fact]
        public async void GetItemsForBet()
        {
            var itemBetRepo = new ItemBettedRepoService(_databaseFactory, new ItemBetQueries());

            await itemBetRepo.InsertAsync(new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(_setup.Bet2.Id, _setup.Item6.DescriptionId, _setup.Item6.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet2.Id, _setup.Item7.DescriptionId, _setup.Item7.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet2.Id, _setup.Item8.DescriptionId, _setup.Item8.AssetId, new decimal(1.0))
            });

            var items = await itemBetRepo.FindAsync(_setup.Bet2);
            Assert.Equal(3, items.Count);
        }

        [Fact]
        public async void GetItemsFromBets()
        {
            var itemBetRepo = new ItemBettedRepoService(_databaseFactory, new ItemBetQueries());

            await itemBetRepo.InsertAsync(new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(_setup.Bet4.Id, _setup.Item9.DescriptionId, _setup.Item9.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet4.Id, _setup.Item10.DescriptionId, _setup.Item10.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet4.Id, _setup.Item11.DescriptionId, _setup.Item11.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet4.Id, _setup.Item12.DescriptionId, _setup.Item12.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet4.Id, _setup.Item13.DescriptionId, _setup.Item13.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet5.Id, _setup.Item14.DescriptionId, _setup.Item14.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet5.Id, _setup.Item15.DescriptionId, _setup.Item15.AssetId, new decimal(1.0))
            });

            var ids = new List<DatabaseModel.Bet>();
            ids.Add(_setup.Bet4);
            ids.Add(_setup.Bet5);
            var res = await itemBetRepo.FindAsync(ids);

            Assert.Equal(7, res.Count);
        }

        [Fact]
        public async void GetValueOfItemBetSuccess()
        {
            var itemBetRepo = new ItemBettedRepoService(_databaseFactory, new ItemBetQueries());

            await itemBetRepo.InsertAsync(new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(_setup.Bet3.Id, _setup.Item1.DescriptionId, _setup.Item1.AssetId, new decimal(1.0)),
                new DatabaseModel.ItemBetted(_setup.Bet3.Id, _setup.Item2.DescriptionId, _setup.Item2.AssetId, new decimal(3.0)),
                new DatabaseModel.ItemBetted(_setup.Bet3.Id, _setup.Item3.DescriptionId, _setup.Item3.AssetId, new decimal(5.54)),
                new DatabaseModel.ItemBetted(_setup.Bet3.Id, _setup.Item4.DescriptionId, _setup.Item4.AssetId, new decimal(154.68)),
            });

            var items = await itemBetRepo.FindAsync(_setup.Bet3);
            Assert.Equal(4, items.Count);
            Assert.Equal(new decimal(164.22), items.Sum(item => item.Value));
        }
    }
}
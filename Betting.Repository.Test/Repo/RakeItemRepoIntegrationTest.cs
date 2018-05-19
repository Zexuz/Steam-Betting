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
    public class RakeItemRepoIntegrationTestSetup
    {
        public string              DatabaseName => "BettingTestRakeItem";
        public string              ConnectionString;
        public IDatabaseConnection Database { get; private set; }

        public DatabaseModel.ItemDescription ItemDescription1 { get; private set; }
        public DatabaseModel.ItemDescription ItemDescription2 { get; private set; }
        public DatabaseModel.Bot             Bot1             { get; private set; }
        public DatabaseModel.Match           Match1           { get; set; }
        public DatabaseModel.Match           Match2           { get; set; }
        public DatabaseModel.GameMode        GameMode         { get; private set; }
        public DatabaseModel.GameMode        GameMode2        { get; private set; }
        public DatabaseModel.GameMode        GameMode3        { get; private set; }
        public DatabaseModel.GameMode        GameMode4        { get; private set; }

        public IDatabaseConnectionFactory DatabaseConnectionFactory { get; private set; }

        public RakeItemRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");

            ConnectionString = new ConnectionStringsForTest().GetConnectionString(DatabaseName);
            Database = new DatabaseConnection(ConnectionString);
            var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(Database);
            DatabaseConnectionFactory = fakedFactory;

            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();

            InitTest().Wait();
        }

        public async Task InitTest()
        {
            var itemDesc1 = new DatabaseModel.ItemDescription("item1", new decimal(10.20), "730", "2", "imageUrl",true);
            var itemDesc2 = new DatabaseModel.ItemDescription("item2", new decimal(11.40), "730", "2", "imageUrl",true);
            ItemDescription1 = await new ItemDescriptionRepoService(DatabaseConnectionFactory, new ItemDescriptionQueries()).InsertAsync(itemDesc1);
            ItemDescription2 = await new ItemDescriptionRepoService(DatabaseConnectionFactory, new ItemDescriptionQueries()).InsertAsync(itemDesc2);

            var gameMode = new DatabaseModel.GameMode("Jackpot", 1);
            var gameMode2 = new DatabaseModel.GameMode("Jackpot1", 2);
            var gameMode3 = new DatabaseModel.GameMode("Jackpot2", 3);
            var gameMode4 = new DatabaseModel.GameMode("Jackpot3", 4);
            GameMode = await new GameModeRepoService(DatabaseConnectionFactory).Insert(gameMode);
            GameMode2 = await new GameModeRepoService(DatabaseConnectionFactory).Insert(gameMode2);
            GameMode3 = await new GameModeRepoService(DatabaseConnectionFactory).Insert(gameMode3);
            GameMode4 = await new GameModeRepoService(DatabaseConnectionFactory).Insert(gameMode4);

            var bot = new DatabaseModel.Bot(",anotherBot", "Bot 2");
            Bot1 = await new BotRepoService(DatabaseConnectionFactory, new BotQueries()).InsertAsync(bot);
            var match = new DatabaseModel.Match(1, "salt", "hash", 55.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, GameMode.Id,
                DateTime.Now);
            Match1 = await new MatchRepoService(DatabaseConnectionFactory, new MatchQueries()).InsertAsync(match);
            var match1 = new DatabaseModel.Match(2, "salt", "hash", 55.ToString(CultureInfo.InvariantCulture), 1, null, null, 1, GameMode.Id,
                DateTime.Now);
            Match2 = await new MatchRepoService(DatabaseConnectionFactory, new MatchQueries()).InsertAsync(match1);
        }
    }

    public class RakeItemRepoIntegrationTest : IClassFixture<RakeItemRepoIntegrationTestSetup>
    {
        private readonly RakeItemRepoIntegrationTestSetup _setup;
        private readonly IRakeItemRepoService             _rakeItemRepoService;

        public RakeItemRepoIntegrationTest(RakeItemRepoIntegrationTestSetup setup)
        {
            _setup = setup;
            _rakeItemRepoService = new RakeItemRepoService(setup.DatabaseConnectionFactory, new RakeItemQueries());
        }

        [Fact]
        public async void AddItemsSuccess()
        {
            var item = new DatabaseModel.RakeItem("assetId1", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var itemId = await _rakeItemRepoService.InsertAsync(item);

            var item2 = new DatabaseModel.RakeItem("assetId2", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var itemId2 = await _rakeItemRepoService.InsertAsync(item2);
            Assert.NotEqual(itemId.Id, itemId2.Id);
        }

        [Fact]
        public async void AddAndFindSuccess()
        {
            var item =
                new DatabaseModel.RakeItem("assetId78789", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id);
            var itemInsertRes = await _rakeItemRepoService.InsertAsync(item);
            var findRes = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "assetId78789",
                DescriptionId = _setup.ItemDescription2.Id,
            });

            Assert.True(itemInsertRes.Id > 0);
            Assert.Equal("assetId78789", findRes.AssetId);
            Assert.Equal(_setup.ItemDescription2.Id, findRes.DescriptionId);
            Assert.Equal(_setup.Match1.Id, findRes.MatchId);
            Assert.Equal(DateTime.Today, findRes.Received);
        }

        [Fact]
        public async void AddItemWithSameAssetIdMultipleTimesThrows()
        {
            var exceptionAsync = Record.ExceptionAsync(async () =>
            {
                var item = new DatabaseModel.RakeItem("assetId3", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id);
                await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
                {
                    item,
                    item
                });
            });
            if (exceptionAsync == null) Assert.True(false, "No exception was thrown");
            var exception1 = await exceptionAsync;
            Assert.IsType(typeof(SqlException), exception1);
        }

        [Fact]
        public async void GetItemsFromAssetIdSuccess()
        {
            var listOfItems = new List<DatabaseModel.RakeItem>
            {
                new DatabaseModel.RakeItem("assetId40", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id),
                new DatabaseModel.RakeItem("assetId41", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id),
                new DatabaseModel.RakeItem("assetId42", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id),
                new DatabaseModel.RakeItem("assetId43", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id),
                new DatabaseModel.RakeItem("assetId44", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id),
                new DatabaseModel.RakeItem("assetId45", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                    _setup.GameMode.Id),
            };
            await _rakeItemRepoService.InsertAsync(listOfItems);

            var items = await _rakeItemRepoService.FindAsync(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId
                {
                    AssetId = "assetId40",
                    DescriptionId = _setup.ItemDescription2.Id
                },
                new AssetAndDescriptionId
                {
                    AssetId = "assetId41",
                    DescriptionId = _setup.ItemDescription2.Id
                },
                new AssetAndDescriptionId
                {
                    AssetId = "assetId43",
                    DescriptionId = _setup.ItemDescription1.Id
                },
                new AssetAndDescriptionId
                {
                    AssetId = "assetId45",
                    DescriptionId = _setup.ItemDescription1.Id
                },
            });
            Assert.Equal(4, items.Count);
        }

        [Fact]
        public async void GetItemsFromIdSuccess()
        {
            var id1 = (await _rakeItemRepoService.InsertAsync(new DatabaseModel.RakeItem("assetId50", _setup.ItemDescription2.Id, _setup.Bot1.Id,
                DateTime.Today, _setup.Match1.Id, _setup.GameMode.Id))).Id;
            var id2 = (await _rakeItemRepoService.InsertAsync(new DatabaseModel.RakeItem("assetId51", _setup.ItemDescription2.Id, _setup.Bot1.Id,
                DateTime.Today, _setup.Match1.Id, _setup.GameMode.Id))).Id;
            var id3 = (await _rakeItemRepoService.InsertAsync(new DatabaseModel.RakeItem("assetId52", _setup.ItemDescription1.Id, _setup.Bot1.Id,
                DateTime.Today, _setup.Match1.Id, _setup.GameMode.Id))).Id;
            var id4 = (await _rakeItemRepoService.InsertAsync(new DatabaseModel.RakeItem("assetId53", _setup.ItemDescription1.Id, _setup.Bot1.Id,
                DateTime.Today, _setup.Match1.Id, _setup.GameMode.Id))).Id;
            var id5 = (await _rakeItemRepoService.InsertAsync(new DatabaseModel.RakeItem("assetId54", _setup.ItemDescription2.Id, _setup.Bot1.Id,
                DateTime.Today, _setup.Match1.Id, _setup.GameMode.Id))).Id;
            var id6 = (await _rakeItemRepoService.InsertAsync(new DatabaseModel.RakeItem("assetId55", _setup.ItemDescription1.Id, _setup.Bot1.Id,
                DateTime.Today, _setup.Match1.Id, _setup.GameMode.Id))).Id;

            var items = await _rakeItemRepoService.FindAsync(new List<int>
            {
                id1,
                id2,
                id6,
                id4,
                id5,
            });
            Assert.Equal(5, items.Count);
        }

        [Fact]
        public async void InsertSameAssetIdButDiffrentDescriptionIdSuccess()
        {
            var item = new DatabaseModel.RakeItem("assetId100", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var item2 = new DatabaseModel.RakeItem("assetId100", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
            {
                item,
                item2
            });

            var itemResult1 = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "AssetId100",
                DescriptionId = _setup.ItemDescription1.Id,
            });
            var itemResult2 = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "AssetId100",
                DescriptionId = _setup.ItemDescription2.Id,
            });

            Assert.False(itemResult1.Id == itemResult2.Id);
            Assert.NotNull(itemResult1);
            Assert.NotNull(itemResult2);
        }

        [Fact]
        public async void SetRakeItemAsSold()
        {
            var item = new DatabaseModel.RakeItem("randAssetId123", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var item2 = new DatabaseModel.RakeItem("randAssetId124", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);

            Assert.False(item.IsSold);
            Assert.False(item2.IsSold);

            await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
            {
                item,
                item2
            });

            await _rakeItemRepoService.SetAsSold(new List<string>
            {
                "randAssetId123"
            });

            var item1Res = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "randAssetId123",
                DescriptionId = _setup.ItemDescription2.Id,
            });
            var item2Res = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "randAssetId124",
                DescriptionId = _setup.ItemDescription1.Id,
            });

            Assert.True(item1Res.IsSold);
            Assert.False(item2Res.IsSold);
        }

        [Fact]
        public async void SetRakeItemsAsSold()
        {
            var item = new DatabaseModel.RakeItem("randAssetId125", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var item2 = new DatabaseModel.RakeItem("randAssetId126", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var item7 = new DatabaseModel.RakeItem("randAssetId127", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);

            Assert.False(item.IsSold);
            Assert.False(item2.IsSold);

            await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
            {
                item,
                item2,
                item7
            });

            await _rakeItemRepoService.SetAsSold(new List<string>
            {
                "randAssetId126",
                "randAssetId125",
            });

            var item1Res = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "randAssetId126",
                DescriptionId = _setup.ItemDescription1.Id,
            });
            var item2Res = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "randAssetId125",
                DescriptionId = _setup.ItemDescription2.Id,
            });
            var item7Res = await _rakeItemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId = "randAssetId127",
                DescriptionId = _setup.ItemDescription1.Id,
            });

            Assert.True(item1Res.IsSold);
            Assert.True(item2Res.IsSold);
            Assert.False(item7Res.IsSold);
        }

        [Fact]
        public async void FindRakeItemFromGameModeId()
        {
            var item = new DatabaseModel.RakeItem("randAssetId225", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode4.Id);
            var item2 = new DatabaseModel.RakeItem("randAssetId326", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode4.Id);
            var item7 = new DatabaseModel.RakeItem("randAssetI3127", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode4.Id);
            var item8 = new DatabaseModel.RakeItem("randAssetI5427", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode2.Id);
            var item9 = new DatabaseModel.RakeItem("455465427", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode3.Id);

            await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
            {
                item,
                item2,
                item7,
                item8,
                item9,
            });


            var item1Res = await _rakeItemRepoService.FindFromGameModeIdAsync(new List<int>
            {
                _setup.GameMode4.Id,
                _setup.GameMode2.Id,
            });

            Assert.Equal(4,item1Res.Count);
        }

//        [Fact]
//        public async void DeletItemsOnWithdrawSuccess()
//        {
//            var item  = new DatabaseModel.RakeItem("assetId123", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id);
//            var item2 = new DatabaseModel.RakeItem("assetId123", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id);
//            await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
//            {
//                item,
//                item2
//            });
//
//            var itemResult1 =
//                await _rakeItemRepoService.DeleteAsync(new AssetAndDescriptionId
//                {
//                    AssetId       = "assetId123",
//                    DescriptionId = _setup.ItemDescription2.Id
//                });
//            var itemResult2 =
//                await _rakeItemRepoService.DeleteAsync(new AssetAndDescriptionId
//                {
//                    AssetId       = "assetId123",
//                    DescriptionId = _setup.ItemDescription1.Id
//                });
//
//            Assert.Equal(1, itemResult1);
//            Assert.Equal(1, itemResult2);
//        }
//
//        [Fact]
//        public async void DeletItemRangeOnWithdrawSuccess()
//        {
//            var item  = new DatabaseModel.RakeItem("assetId1231", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id);
//            var item2 = new DatabaseModel.RakeItem("assetId1234", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id);
//            var item3 = new DatabaseModel.RakeItem("assetId1233", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id);
//            await _rakeItemRepoService.InsertAsync(new List<DatabaseModel.RakeItem>
//            {
//                item,
//                item2,
//                item3
//            });
//
//            var itemResult1 = await _rakeItemRepoService.DeleteAsync(new List<AssetAndDescriptionId>
//            {
//                new AssetAndDescriptionId {AssetId = "assetId1231", DescriptionId = _setup.ItemDescription2.Id},
//                new AssetAndDescriptionId {AssetId = "assetId1234", DescriptionId = _setup.ItemDescription1.Id}
//            });
//
//            Assert.Equal(2, itemResult1);
//        }

        [Fact]
        public async void GetAllItemsSuccess()
        {
            var item = new DatabaseModel.RakeItem("assetId12311", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var item2 = new DatabaseModel.RakeItem("assetId12344", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);
            var item3 = new DatabaseModel.RakeItem("assetId12333", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);

            var list = new List<DatabaseModel.RakeItem>
            {
                item,
                item2,
                item3
            };

            await _rakeItemRepoService.InsertAsync(list);
            var res = await _rakeItemRepoService.GetAll();

            int matches = 0;
            foreach (var t in res)
            {
                matches += list.Count(t1 => t1.AssetId == t.AssetId);
            }

            Assert.Equal(3, matches);
        }

        [Fact]
        public async void GetRakeItemsForSpecificMatch()
        {
            var item = new DatabaseModel.RakeItem("assetId15311", _setup.ItemDescription2.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match2.Id,
                _setup.GameMode.Id);
            var item2 = new DatabaseModel.RakeItem("assetId15344", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match2.Id,
                _setup.GameMode.Id);
            var item3 = new DatabaseModel.RakeItem("assetId15333", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match2.Id,
                _setup.GameMode.Id);
            var item4 = new DatabaseModel.RakeItem("assetId1233489", _setup.ItemDescription1.Id, _setup.Bot1.Id, DateTime.Today, _setup.Match1.Id,
                _setup.GameMode.Id);

            var list = new List<DatabaseModel.RakeItem>
            {
                item,
                item2,
                item3,
                item4
            };

            await _rakeItemRepoService.InsertAsync(list);
            var res = await _rakeItemRepoService.FindAsync(_setup.Match2);
            Assert.Equal(3, res.Count);
        }
    }
}
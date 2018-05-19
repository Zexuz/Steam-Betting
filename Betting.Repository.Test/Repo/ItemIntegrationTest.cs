using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class ItemRepoIntegrationTestSetup
    {
        public string              DatabaseName => "BettingTestItem";
        public string              ConnectionString;
        public IDatabaseConnection Database { get; private set; }

        public DatabaseModel.ItemDescription ItemDescription1 { get; private set; }
        public DatabaseModel.ItemDescription ItemDescription2 { get; private set; }
        public DatabaseModel.Bot             Bot1             { get; private set; }
        public DatabaseModel.User            User1            { get; private set; }
        public DatabaseModel.User            User2            { get; private set; }
        public DatabaseModel.User            User3            { get; private set; }
        public DatabaseModel.User            User4            { get; private set; }
        public DatabaseModel.User            User5            { get; private set; }
        public DatabaseModel.User            User6            { get; set; }

        public IDatabaseConnectionFactory DatabaseConnectionFactory { get; private set; }

        public ItemRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");

            ConnectionString = new ConnectionStringsForTest().GetConnectionString(DatabaseName);
            Database         = new DatabaseConnection(ConnectionString);
            var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(Database);
            DatabaseConnectionFactory = fakedFactory;

            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();

            InitTest().Wait();
        }

        public async Task InitTest()
        {
            var itemDesc1    = new DatabaseModel.ItemDescription("item1", new decimal(10.20), "730", "2", "imageUrl",true);
            var itemDesc2    = new DatabaseModel.ItemDescription("item2", new decimal(11.40), "730", "2", "imageUrl",true);
            ItemDescription1 = await new ItemDescriptionRepoService(DatabaseConnectionFactory, new ItemDescriptionQueries()).InsertAsync(itemDesc1);
            ItemDescription2 = await new ItemDescriptionRepoService(DatabaseConnectionFactory, new ItemDescriptionQueries()).InsertAsync(itemDesc2);

            var bot = new DatabaseModel.Bot(",anotherBot", "Bot 2");
            Bot1    = await new BotRepoService(DatabaseConnectionFactory, new BotQueries()).InsertAsync(bot);

            var userRepo        = new QueryFactory().UserQueries;
            var userRepoService = new UserRepoService(DatabaseConnectionFactory, userRepo);

            User1 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "987456131549",
                "Kalle",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User2 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "987456131548",
                "Kalle1",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User3 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "789456321159",
                "Kalle1",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User4 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "4571248613587",
                "Kalle1",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User5 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "2745613477",
                "Kalle1",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
            User6 = await userRepoService.InsertAsync(new DatabaseModel.User(
                "27456134hjadshjgs77",
                "Kalle1",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            ));
        }
    }

    public class ItemRepoIntegrationTest : IClassFixture<ItemRepoIntegrationTestSetup>
    {
        private readonly ItemRepoIntegrationTestSetup _setup;
        private readonly ItemRepoService              _itemRepoService;

        public ItemRepoIntegrationTest(ItemRepoIntegrationTestSetup setup)
        {
            _setup           = setup;
            _itemRepoService = new ItemRepoService(setup.DatabaseConnectionFactory, new ItemQueries());
        }

        [Fact]
        public async void AddItemsSuccess()
        {
            var item   = new DatabaseModel.Item("assetId1", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var itemId = await _itemRepoService.InsertAsync(item);

            var item2   = new DatabaseModel.Item("assetId2", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var itemId2 = await _itemRepoService.InsertAsync(item2);
            Assert.NotEqual(itemId, itemId2);
        }

        [Fact]
        public async void ChangeOwnerOfItemsSuccess()
        {
            var item    = new DatabaseModel.Item("assetId4", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var retItem = await _itemRepoService.InsertAsync(item);
            Assert.True(retItem.Id > 0);

            var updateRes = await _itemRepoService.ChangeOwner(retItem, _setup.User2);
            Assert.Equal(1, updateRes);

            var getRes = await _itemRepoService.FindAsync(retItem.Id);
            Assert.Equal(_setup.User2.Id, getRes.OwnerId);
            Assert.Equal(_setup.Bot1.Id, getRes.LocationId);
        }

        [Fact]
        public async void AddItemWithSameAssetIdMultipleTimesThrows()
        {
            var item = new DatabaseModel.Item("assetId3", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            await Assert.ThrowsAsync<SqlException>(async () => await _itemRepoService.InsertAsync(new List<DatabaseModel.Item>
            {
                item,
                item
            }));
        }

        [Fact]
        public async void GetItemsThatUserOwnsSuccess()
        {
            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId30", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User3.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId31", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User3.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId32", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User3.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId33", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User3.Id, DateTimeOffset.Now),
            };
            await _itemRepoService.InsertAsync(listOfItems);

            var items = await _itemRepoService.FindAsync(_setup.User3);
            Assert.Equal(4, items.Count);
        }

        [Fact]
        public async void GetItemsThatUserOwnsReturnsEmptyListSuccess()
        {
            var items = await _itemRepoService.FindAsync(_setup.User4);
            Assert.Equal(0, items.Count);
        }

        [Fact]
        public async void GetItemsFromAssetIdSuccess()
        {
            var listOfItems = new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("assetId40", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId41", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId42", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId43", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId44", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now),
                new DatabaseModel.Item("assetId45", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now),
            };
            await _itemRepoService.InsertAsync(listOfItems);

            var items = await _itemRepoService.FindAsync(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId
                {
                    AssetId       = "assetId40",
                    DescriptionId = _setup.ItemDescription2.Id
                },
                new AssetAndDescriptionId
                {
                    AssetId       = "assetId41",
                    DescriptionId = _setup.ItemDescription2.Id
                },
                new AssetAndDescriptionId
                {
                    AssetId       = "assetId43",
                    DescriptionId = _setup.ItemDescription1.Id
                },
                new AssetAndDescriptionId
                {
                    AssetId       = "assetId45",
                    DescriptionId = _setup.ItemDescription1.Id
                },
            });
            Assert.Equal(4, items.Count);
        }

        [Fact]
        public async void GetItemsFromIdSuccess()
        {
            var id1 = (await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId50", _setup.ItemDescription2.Id, _setup.Bot1.Id,
                _setup.User5.Id, DateTimeOffset.Now))).Id;
            var id2 = (await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId51", _setup.ItemDescription2.Id, _setup.Bot1.Id,
                _setup.User5.Id, DateTimeOffset.Now))).Id;
            var id3 = (await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId52", _setup.ItemDescription1.Id, _setup.Bot1.Id,
                _setup.User5.Id, DateTimeOffset.Now))).Id;
            var id4 = (await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId53", _setup.ItemDescription1.Id, _setup.Bot1.Id,
                _setup.User5.Id, DateTimeOffset.Now))).Id;
            var id5 = (await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId54", _setup.ItemDescription2.Id, _setup.Bot1.Id,
                _setup.User5.Id, DateTimeOffset.Now))).Id;
            var id6 = (await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId55", _setup.ItemDescription1.Id, _setup.Bot1.Id,
                _setup.User5.Id, DateTimeOffset.Now))).Id;

            var items = await _itemRepoService.FindAsync(new List<int>
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
            var item  = new DatabaseModel.Item("assetId100", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item2 = new DatabaseModel.Item("assetId100", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            await _itemRepoService.InsertAsync(new List<DatabaseModel.Item>
            {
                item,
                item2
            });

            var itemResult1 = await _itemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId       = "AssetId100",
                DescriptionId = _setup.ItemDescription1.Id,
            });
            var itemResult2 = await _itemRepoService.FindAsync(new AssetAndDescriptionId
            {
                AssetId       = "AssetId100",
                DescriptionId = _setup.ItemDescription2.Id,
            });

            Assert.False(itemResult1.Id == itemResult2.Id);
            Assert.NotNull(itemResult1);
            Assert.NotNull(itemResult2);
        }

        [Fact]
        public async void DeletItemsOnWithdrawSuccess()
        {
            var item  = new DatabaseModel.Item("assetId123", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item2 = new DatabaseModel.Item("assetId123", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            await _itemRepoService.InsertAsync(new List<DatabaseModel.Item>
            {
                item,
                item2
            });

            var itemResult1                                                           =
                await _itemRepoService.DeleteAsync(new AssetAndDescriptionId {AssetId = "assetId123", DescriptionId = _setup.ItemDescription2.Id});
            var itemResult2                                                           =
                await _itemRepoService.DeleteAsync(new AssetAndDescriptionId {AssetId = "assetId123", DescriptionId = _setup.ItemDescription1.Id});

            Assert.Equal(1, itemResult1);
            Assert.Equal(1, itemResult2);
        }

        [Fact]
        public async void DeletItemsSuccess()
        {
            var item        = new DatabaseModel.Item("assetId1231", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item2       = new DatabaseModel.Item("assetId1231", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item3       = new DatabaseModel.Item("assetId1211", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var insertItem1 = await _itemRepoService.InsertAsync(item);
            var insertItem2 = await _itemRepoService.InsertAsync(item2);
            var insertItem3 = await _itemRepoService.InsertAsync(item3);

            var res = await _itemRepoService.DeleteAsync(new List<int>
            {
                insertItem1.Id,
                insertItem3.Id
            });


            Assert.Equal(2, res);
        }

        [Fact]
        public async void DeletItemRangeOnWithdrawSuccess()
        {
            var item  = new DatabaseModel.Item("assetId1231", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item2 = new DatabaseModel.Item("assetId1234", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item3 = new DatabaseModel.Item("assetId1233", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            await _itemRepoService.InsertAsync(new List<DatabaseModel.Item>
            {
                item,
                item2,
                item3
            });

            var itemResult1 = await _itemRepoService.DeleteAsync(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId1231", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId1234", DescriptionId = _setup.ItemDescription1.Id}
            });

            Assert.Equal(2, itemResult1);
        }

        [Fact]
        public async void GetAllItemsSuccess()
        {
            var item  = new DatabaseModel.Item("assetId12311", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item2 = new DatabaseModel.Item("assetId12344", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item3 = new DatabaseModel.Item("assetId12333", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);

            var list = new List<DatabaseModel.Item>
            {
                item,
                item2,
                item3
            };

            await _itemRepoService.InsertAsync(list);
            var res = await _itemRepoService.GetAll();

            int matches = 0;
            for (int i = 0; i < res.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].AssetId == res[i].AssetId)
                        matches++;
                }
            }

            Assert.Equal(3, matches);
        }

        [Fact]
        public async void ChangeOwnerOfRange()
        {
            var item1    = new DatabaseModel.Item("assetId987894", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item2    = new DatabaseModel.Item("assetId987895", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item3    = new DatabaseModel.Item("assetId987896", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var item4    = new DatabaseModel.Item("assetId987897", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User1.Id, DateTimeOffset.Now);
            var retItem1 = await _itemRepoService.InsertAsync(item1);
            var retItem2 = await _itemRepoService.InsertAsync(item2);
            var retItem3 = await _itemRepoService.InsertAsync(item3);
            var retItem4 = await _itemRepoService.InsertAsync(item4);

            var updateRes = await _itemRepoService.ChangeOwner(new List<int>
            {
                retItem1.Id,
                retItem2.Id,
                retItem3.Id,
                retItem4.Id,
            }, _setup.User6);

            var getRes = await _itemRepoService.FindAsync(_setup.User6);
            Assert.Equal(4, getRes.Count);
        }

        [Fact]
        public async void ChangeOwnerWithAssetAndDescriptionId()
        {
            await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId7874011", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now));
            await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId7874012", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now));
            await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId7874013", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now));
            await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId7874014", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now));
            await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId7874015", _setup.ItemDescription2.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now));
            await _itemRepoService.InsertAsync(new DatabaseModel.Item("assetId7874016", _setup.ItemDescription1.Id, _setup.Bot1.Id, _setup.User5.Id, DateTimeOffset.Now));

            await _itemRepoService.ChangeOwner(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId7874011", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874012", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874013", DescriptionId = _setup.ItemDescription1.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874014", DescriptionId = _setup.ItemDescription1.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874015", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874016", DescriptionId = _setup.ItemDescription1.Id}
            }, _setup.User6);

            var res = await _itemRepoService.FindAsync(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = "assetId7874011", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874012", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874013", DescriptionId = _setup.ItemDescription1.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874014", DescriptionId = _setup.ItemDescription1.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874015", DescriptionId = _setup.ItemDescription2.Id},
                new AssetAndDescriptionId {AssetId = "assetId7874016", DescriptionId = _setup.ItemDescription1.Id}
            });


            Assert.True(res.All(item => item.OwnerId == _setup.User6.Id));
        }
    }
}
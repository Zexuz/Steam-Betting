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
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class ItemInOfferTransactionIntegrationTestSetup
    {
        public string              DatabaseName => "BettingTestItemsInOfferTransaction";
        public string              ConnectionString;
        public IDatabaseConnection Database { get; private set; }

        public ItemInOfferTransactionIntegrationTestSetup()
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

        public DatabaseModel.OfferTransaction OfferTransaction1 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction2 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction3 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction4 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction5 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction6 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction7 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction8 { get; set; }
        public DatabaseModel.OfferTransaction OfferTransaction9 { get; set; }

        public IDatabaseConnectionFactory DatabaseConnectionFactory { get; set; }

        public DatabaseModel.ItemDescription ItemDescription1 { get; set; }
        public DatabaseModel.ItemDescription ItemDescription2 { get; set; }

        private DatabaseModel.User _user1;
        private DatabaseModel.User _user2;

        private DatabaseModel.Bot _bot1;

        public async Task InitTest()
        {
            var offerTransactionServiceRepo = new OfferTransactionRepoService(DatabaseConnectionFactory, new OfferTransationQueries());
            var botServiceRepo              = new BotRepoService(DatabaseConnectionFactory, new BotQueries());
            var userServiceRepo             = new UserRepoService(DatabaseConnectionFactory, new UserQueries());
            var itemDescServiceRepo         = new ItemDescriptionRepoService(DatabaseConnectionFactory, new ItemDescriptionQueries());

            _user1 = await userServiceRepo.InsertAsync(new DatabaseModel.User("steamId1", "name1", "imgUrl", "tradelink", DateTime.Now,
                DateTime.Now,false));
            _user2 = await userServiceRepo.InsertAsync(new DatabaseModel.User("steamId2", "name2", "imgUrl", "tradelink", DateTime.Now,
                DateTime.Now,false));
            _bot1 = await botServiceRepo.InsertAsync(new DatabaseModel.Bot("botSteamId1", "botName1"));


            var toInsert1     = new DatabaseModel.OfferTransaction(_user1.Id, _bot1.Id, new decimal(10.45), true, "456232", DateTime.Now);
            var toInsert2     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(54.45), false, "456332", DateTime.Now);
            var toInsert3     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(666.66), false, "454132", DateTime.Now);
            var toInsert4     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(544.75), false, "455132", null);
            var toInsert5     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(10.75), true, "456162", DateTime.Today);
            var toInsert6     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(10.75), true, "44864748654", DateTime.Today);
            var toInsert7     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(10.75), true, "548", DateTime.Today);
            var toInsert8     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(10.75), true, "555", DateTime.Today);
            var toInsert9     = new DatabaseModel.OfferTransaction(_user2.Id, _bot1.Id, new decimal(10.75), true, "5646555", DateTime.Today);
            OfferTransaction1 = await offerTransactionServiceRepo.InsertAsync(toInsert1);
            OfferTransaction2 = await offerTransactionServiceRepo.InsertAsync(toInsert2);
            OfferTransaction3 = await offerTransactionServiceRepo.InsertAsync(toInsert3);
            OfferTransaction4 = await offerTransactionServiceRepo.InsertAsync(toInsert4);
            OfferTransaction5 = await offerTransactionServiceRepo.InsertAsync(toInsert5);
            OfferTransaction6 = await offerTransactionServiceRepo.InsertAsync(toInsert6);
            OfferTransaction7 = await offerTransactionServiceRepo.InsertAsync(toInsert7);
            OfferTransaction8 = await offerTransactionServiceRepo.InsertAsync(toInsert8);
            OfferTransaction9 = await offerTransactionServiceRepo.InsertAsync(toInsert9);

            ItemDescription1 =
                await itemDescServiceRepo.InsertAsync(new DatabaseModel.ItemDescription("weapon1", new decimal(1.40), "720", "2", "imgUrl1",true));
            ItemDescription2 =
                await itemDescServiceRepo.InsertAsync(new DatabaseModel.ItemDescription("weapon2", new decimal(8.75), "720", "2", "imgUrl2",true));
        }
    }

    public class ItemInOfferTransactionIntegrationTest : IClassFixture<ItemInOfferTransactionIntegrationTestSetup>
    {
        private readonly ItemInOfferTransactionIntegrationTestSetup _setup;
        private readonly IDatabaseConnection                        _database;
        private readonly IItemInOfferTransactionRepoService         _itemInOfferTransactionRepoService;

        public ItemInOfferTransactionIntegrationTest(ItemInOfferTransactionIntegrationTestSetup setup)
        {
            _setup                             = setup;
            _database                          = setup.Database;
            _itemInOfferTransactionRepoService =
                new ItemInOfferTransactionRepoService(_setup.DatabaseConnectionFactory, new ItemInOfferTransactionQueries());
        }

        [Fact]
        public async Task InsertItemsInOfferSuccess()
        {
            var toInsert = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction1.Id, _setup.ItemDescription1.Id, "assetId4474",
                new decimal(14.45));
            var res = await _itemInOfferTransactionRepoService.InsertAsync(toInsert);

            Assert.True(res.Id > 0);
        }

        [Fact]
        public async Task InsertItemsRangeInOfferSuccess()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction2.Id, _setup.ItemDescription1.Id, "assetId4471", new decimal(14.45)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction2.Id, _setup.ItemDescription2.Id, "assetId4472", new decimal(17.78)),
            };
            await _itemInOfferTransactionRepoService.InsertAsync(list);
        }

        [Fact]
        public async Task InsertItemsInOfferInvalidOfferTransactionIdThrows()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(54845, _setup.ItemDescription1.Id, "assetId4473", new decimal(14.45)),
                new DatabaseModel.ItemInOfferTransaction(54845, _setup.ItemDescription2.Id, "assetId4475", new decimal(17.78)),
            };

            await Assert.ThrowsAsync<SqlException>(async () => await _itemInOfferTransactionRepoService.InsertAsync(list));
        }


        [Fact]
        public async Task FindAllItemsInOfferFromOfferIdSuccess()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction3.Id, _setup.ItemDescription1.Id, "assetId4476", new decimal(14.45)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction3.Id, _setup.ItemDescription2.Id, "assetId4477", new decimal(17.78)),
            };
            await _itemInOfferTransactionRepoService.InsertAsync(list);

            var findRes = await _itemInOfferTransactionRepoService.FindAsync(_setup.OfferTransaction3);
            Assert.Equal(2, findRes.Count);
            Assert.Equal(new decimal(32.23), findRes.Sum(item => item.Value));
        }

        [Fact]
        public async Task FindAllItemsInOfferFromOfferIdsSuccess()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction4.Id, _setup.ItemDescription1.Id, "assetId5474", new decimal(14.45)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction4.Id, _setup.ItemDescription2.Id, "assetId5473", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction4.Id, _setup.ItemDescription2.Id, "assetId5472", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction5.Id, _setup.ItemDescription2.Id, "assetId5471", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction5.Id, _setup.ItemDescription2.Id, "assetId5476", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction5.Id, _setup.ItemDescription2.Id, "assetId5477", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction5.Id, _setup.ItemDescription2.Id, "assetId52134", new decimal(17.78)),
            };
            await _itemInOfferTransactionRepoService.InsertAsync(list);

            var findRes = await _itemInOfferTransactionRepoService.FindAsync(new List<DatabaseModel.OfferTransaction>
            {
                _setup.OfferTransaction4,
                _setup.OfferTransaction5
            });
            Assert.Equal(7, findRes.Count);
            Assert.Equal(new decimal(121.13), findRes.Sum(item => item.Value));
        }

        [Fact]
        public async Task AddSameAssetIdWithDiffrentDescriptionIdSuccess()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction6.Id, _setup.ItemDescription1.Id, "assetId54178", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction6.Id, _setup.ItemDescription2.Id, "assetId54178", new decimal(17.78)),
            };
            await _itemInOfferTransactionRepoService.InsertAsync(list);

            var findRes = await _itemInOfferTransactionRepoService.FindAsync(_setup.OfferTransaction6);
            Assert.Equal(2, findRes.Count);
            Assert.Equal(new decimal((17.78) * 2), findRes.Sum(item => item.Value));
        }

        [Fact]
        public async Task AddSameAssetIdAndDescriptionButDirrentOfferIdIdSuccess()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction1.Id, _setup.ItemDescription2.Id, "assetId5478", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction2.Id, _setup.ItemDescription2.Id, "assetId5478", new decimal(17.78)),
            };
            await _itemInOfferTransactionRepoService.InsertAsync(list);
        }

        [Fact]
        public async Task AddSameAssetIdAndDescriptionAndSameOfferIdIdThrows()
        {
            var list = new List<DatabaseModel.ItemInOfferTransaction>
            {
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction7.Id, _setup.ItemDescription2.Id, "assetId5478", new decimal(17.78)),
                new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction7.Id, _setup.ItemDescription2.Id, "assetId5478", new decimal(17.78)),
            };
            await Assert.ThrowsAsync<SqlException>(async () => await _itemInOfferTransactionRepoService.InsertAsync(list));
        }

        [Fact]
        public async Task RemovedAllItemsWithOfferId()
        {
            var toInsert1 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction8.Id, _setup.ItemDescription1.Id, "assetId4471",
                new decimal(14.45));
            var toInsert2 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction8.Id, _setup.ItemDescription1.Id, "assetId4472",
                new decimal(14.45));
            var toInsert3 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction8.Id, _setup.ItemDescription1.Id, "assetId4473",
                new decimal(14.45));
            var toInsert4 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction8.Id, _setup.ItemDescription1.Id, "assetId4474",
                new decimal(14.45));
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert1);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert2);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert3);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert4);

            var nr = await _itemInOfferTransactionRepoService.Remove(_setup.OfferTransaction8.Id);

            Assert.Equal(4, nr);
        }

        [Fact]
        public async Task GetItemCount()
        {
            var toInsert1 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction9.Id, _setup.ItemDescription1.Id, "assetId4871",
                new decimal(14.45));
            var toInsert2 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction9.Id, _setup.ItemDescription1.Id, "assetId4872",
                new decimal(14.45));
            var toInsert3 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction9.Id, _setup.ItemDescription1.Id, "assetId4873",
                new decimal(14.45));
            var toInsert4 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction9.Id, _setup.ItemDescription1.Id, "assetId4874",
                new decimal(14.45));
            var toInsert5 = new DatabaseModel.ItemInOfferTransaction(_setup.OfferTransaction9.Id, _setup.ItemDescription1.Id, "assetId8464",
                new decimal(14.45));
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert1);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert2);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert3);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert4);
            await _itemInOfferTransactionRepoService.InsertAsync(toInsert5);

            var nr = await _itemInOfferTransactionRepoService.GetItemCountInOffer(_setup.OfferTransaction9.Id);

            Assert.Equal(5, nr);
        }
    }
}
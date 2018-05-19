using System;
using System.Data.SqlClient;
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
    public class OfferTransactionIntegrationTestSetup
    {
        public string DatabaseName => "BettingTestOfferTransaction";
        public string ConnectionString;

        public IDatabaseConnection        Database                  { get; private set; }
        public UserRepoService            UserRepoService           { get; private set; }
        public BotRepoService             BotRepoService            { get; private set; }
        public IDatabaseConnectionFactory DatabaseConnectionFactory { get; private set; }

        public OfferTransactionIntegrationTestSetup()
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

        public DatabaseModel.User User1 { get; set; }
        public DatabaseModel.User User2 { get; set; }
        public DatabaseModel.User User3 { get; set; }
        public DatabaseModel.User User4 { get; set; }

        public DatabaseModel.Bot Bot1 { get; set; }
        public DatabaseModel.Bot Bot2 { get; set; }
        public DatabaseModel.Bot Bot3 { get; set; }


        public async Task InitTest()
        {
            UserRepoService = new UserRepoService(DatabaseConnectionFactory, new UserQueries());
            BotRepoService  = new BotRepoService(DatabaseConnectionFactory, new BotQueries());

            User1 = await UserRepoService.InsertAsync(new DatabaseModel.User("steamid1", "name1", "imgUrl1", null, DateTime.Now, DateTime.Now,false));
            User2 = await UserRepoService.InsertAsync(new DatabaseModel.User("steamid2", "name1", "imgUrl1", "tradelin", DateTime.Now, DateTime.Now,false));
            User3 = await UserRepoService.InsertAsync(new DatabaseModel.User("steamid3", "name1", "imgUrl1", "tradelin", DateTime.Now, DateTime.Now,false));
            User4 = await UserRepoService.InsertAsync(new DatabaseModel.User("steamid4", "name1", "imgUrl1", "tradelin", DateTime.Now, DateTime.Now,false));

            Bot1 = await BotRepoService.InsertAsync(new DatabaseModel.Bot("botSteamId1", "botname1"));
            Bot2 = await BotRepoService.InsertAsync(new DatabaseModel.Bot("botSteamId2", "botname1"));
            Bot3 = await BotRepoService.InsertAsync(new DatabaseModel.Bot("botSteamId3", "botname3"));
        }
    }

    public class OfferTransactionIntegrationTest : IClassFixture<OfferTransactionIntegrationTestSetup>
    {
        private readonly OfferTransactionIntegrationTestSetup _setup;
        private readonly IDatabaseConnection                  _database;
        private          OfferTransactionRepoService          _offerTransactionRepoService;

        public OfferTransactionIntegrationTest(OfferTransactionIntegrationTestSetup setup)
        {
            _setup                       = setup;
            _database                    = setup.Database;
            _offerTransactionRepoService = new OfferTransactionRepoService(_setup.DatabaseConnectionFactory, new OfferTransationQueries());
        }


        [Fact]
        public async Task FindActiveOffersForUser()
        {
            var offerTransaction1 =
                new DatabaseModel.OfferTransaction(_setup.User3.Id, _setup.Bot3.Id, new decimal(11.54), false, "455612", DateTime.Today);
            var offerTransaction2 = new DatabaseModel.OfferTransaction(_setup.User3.Id, _setup.Bot3.Id, new decimal(11.54), false, null,
                DateTime.Today.Subtract(TimeSpan.FromDays(1)));
            var offerTransaction3 = new DatabaseModel.OfferTransaction(_setup.User3.Id, _setup.Bot3.Id, new decimal(54.78), false, "456142", null);
            await _offerTransactionRepoService.InsertAsync(offerTransaction1);
            await _offerTransactionRepoService.InsertAsync(offerTransaction2);
            await _offerTransactionRepoService.InsertAsync(offerTransaction3);


            var res = await _offerTransactionRepoService.FindActiveAsync(_setup.User3);
            Assert.Equal(1, res.Count);
            Assert.Equal(new decimal(54.78), res[0].TotalValue);
            Assert.Null(res[0].Accepted);
        }

        [Fact]
        public async Task InsertWithdrawReturnsIdSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot1.Id, new decimal(11.54), false, "78", DateTime.Now);
            var returnObj        = await _offerTransactionRepoService.InsertAsync(offerTransaction);

            Assert.True(returnObj.Id > 0);
            Assert.Equal(false, returnObj.IsDeposit);
            Assert.Equal(new decimal(11.54), returnObj.TotalValue);
        }


        [Fact]
        public async Task UpdateSteamOfferIdSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot1.Id, new decimal(45.54), false, null, DateTime.Now);
            var insertRes        = await _offerTransactionRepoService.InsertAsync(offerTransaction);
            await _offerTransactionRepoService.AddSteamIdToOffer(insertRes.Id, "1337");

            var returnObj = await _offerTransactionRepoService.FindAsync("1337");

            Assert.True(returnObj.Id > 0);
            Assert.Equal(false, returnObj.IsDeposit);
            Assert.Equal(new decimal(45.54), returnObj.TotalValue);
        }

        [Fact]
        public async Task UpdateAcceptedTimeSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User4.Id, _setup.Bot1.Id, new decimal(45.54), false, "1336", null);
            var insertRes        = await _offerTransactionRepoService.InsertAsync(offerTransaction);
            await _offerTransactionRepoService.AddAcceptedTimesptampToOffer(DateTime.Today, insertRes.Id);

            var returnObj = await _offerTransactionRepoService.FindAsync("1336");

            Assert.True(returnObj.Id > 0);
            Assert.Equal(false, returnObj.IsDeposit);
            Assert.Equal(new decimal(45.54), returnObj.TotalValue);
            Assert.Equal(DateTime.Today, returnObj.Accepted);
        }

        [Fact]
        public async Task DeleteSteamOfferIdSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot1.Id, new decimal(45.54), false, null, DateTime.Now);
            var insertRes        = await _offerTransactionRepoService.InsertAsync(offerTransaction);
            await _offerTransactionRepoService.Remove(insertRes.Id);

            var res = await _offerTransactionRepoService.FindAsync(insertRes.Id);
            Assert.Null(res);
        }

        [Fact]
        public async Task FindWithSteamOfferIdSuccess()
        {
            var offerTransaction =
                new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot1.Id, new decimal(11.54), false, "4758", DateTime.Now);
            await _offerTransactionRepoService.InsertAsync(offerTransaction);
            var returnObj = await _offerTransactionRepoService.FindAsync("4758");

            Assert.True(returnObj.Id > 0);
            Assert.Equal(false, returnObj.IsDeposit);
            Assert.Equal(new decimal(11.54), returnObj.TotalValue);
        }

        [Fact]
        public async Task InsertSameSteamOfferIdThrows()
        {
            var offerTransaction1 =
                new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot1.Id, new decimal(11.54), false, "50", DateTime.Now);
            var offerTransaction2 =
                new DatabaseModel.OfferTransaction(_setup.User3.Id, _setup.Bot2.Id, new decimal(11.54), false, "50", DateTime.Now);
            await _offerTransactionRepoService.InsertAsync(offerTransaction1);

            await Assert.ThrowsAsync<SqlException>(async () => await _offerTransactionRepoService.InsertAsync(offerTransaction2));
        }

        [Fact]
        public async Task InsertDepositReturnsIdSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot1.Id, new decimal(11.54), true, "74", DateTime.Now);
            var returnObj        = await _offerTransactionRepoService.InsertAsync(offerTransaction);

            Assert.True(returnObj.Id > 0);
            Assert.Equal(true, returnObj.IsDeposit);
            Assert.Equal(new decimal(11.54), returnObj.TotalValue);
        }

        [Fact]
        public async Task SelectFromOfferTranasactionDepositIdSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot2.Id, new decimal(11.54), true, "45", DateTime.Now);
            var insertReturn     = await _offerTransactionRepoService.InsertAsync(offerTransaction);

            var findReturn = await _offerTransactionRepoService.FindAsync(insertReturn.Id);

            Assert.True(findReturn.Id > 0);
            Assert.Equal(true, findReturn.IsDeposit);
            Assert.Equal(new decimal(11.54), findReturn.TotalValue);
            Assert.True(DateTime.Now - findReturn.Accepted < TimeSpan.FromSeconds(10));
        }

        [Fact]
        public async Task SelectFromOfferTranasactionWithdrawIdSuccess()
        {
            var offerTransaction = new DatabaseModel.OfferTransaction(_setup.User1.Id, _setup.Bot2.Id, new decimal(11.54), false, "65", DateTime.Now);
            var insertReturn     = await _offerTransactionRepoService.InsertAsync(offerTransaction);

            var findReturn = await _offerTransactionRepoService.FindAsync(insertReturn.Id);

            Assert.True(findReturn.Id > 0);
            Assert.Equal(false, findReturn.IsDeposit);
            Assert.Equal(new decimal(11.54), findReturn.TotalValue);
            Assert.True(DateTime.Now - findReturn.Accepted < TimeSpan.FromSeconds(10));
        }
    }
}
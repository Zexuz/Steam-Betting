using System.Collections.Generic;
using System.Linq;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class ItemDescriptionRepoIntegrationTestSetup
    {
        public string DatabaseName => "BettingTestItemDescription";

        public ItemDescriptionRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Factories.Database.Main, connectionString, DatabaseName).ResetDatabase();
        }
    }

    public class ItemDescriptionIntegrationTest : IClassFixture<ItemDescriptionRepoIntegrationTestSetup>
    {
        private          string                     _connectionString;
        private readonly IDatabaseConnectionFactory _fakedFactory;
        private readonly ItemDescriptionRepoService _itemDescriptionRepoService;


        public ItemDescriptionIntegrationTest(ItemDescriptionRepoIntegrationTestSetup setup)
        {
            _connectionString = new ConnectionStringsForTest().GetConnectionString(setup.DatabaseName);
            var database      = new DatabaseConnection(_connectionString);

            _fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => _fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(database);

            _itemDescriptionRepoService = new ItemDescriptionRepoService(_fakedFactory, new ItemDescriptionQueries());
        }

        [Fact]
        public async void InsertAndGetSuccess()
        {
            var itemDescription = new DatabaseModel.ItemDescription("AWP AZIMOW", new decimal(42.21), "730", "2", "imageUrl",true);
            var insertRes       = await _itemDescriptionRepoService.InsertAsync(itemDescription);
            Assert.True(insertRes.Id > 0);

            var getRes = await new ItemDescriptionRepoService(_fakedFactory, new ItemDescriptionQueries()).FindAsync("AWP AZIMOW");
            Assert.Equal(new decimal(42.21), getRes.Value);
            Assert.Equal("730", getRes.AppId);
            Assert.Equal("2", getRes.ContextId);
            Assert.NotEqual(0, getRes.Id);
        }

        [Fact]
        public async void GetIdsSuccess()
        {
            var itemDescription1 = new DatabaseModel.ItemDescription("AWP AZIMOW xD1", new decimal(12.21), "730", "2", "imageUrl",true);
            var itemDescription2 = new DatabaseModel.ItemDescription("AWP AZIMOW xD2", new decimal(11.21), "730", "2", "imageUrl",true);
            var itemDescription3 = new DatabaseModel.ItemDescription("AWP AZIMOW xD3", new decimal(10.21), "730", "2", "imageUrl",false);
            var itemDescription4 = new DatabaseModel.ItemDescription("AWP AZIMOW xD4", new decimal(10.21), "730", "2", "imageUrl",true);
            var itemDescId1      = await _itemDescriptionRepoService.InsertAsync(itemDescription1);
            var itemDescId2      = await _itemDescriptionRepoService.InsertAsync(itemDescription2);
            var itemDescId3      = await _itemDescriptionRepoService.InsertAsync(itemDescription3);
            await _itemDescriptionRepoService.InsertAsync(itemDescription4);

            var res = await _itemDescriptionRepoService.FindAsync(new List<int>
            {
                itemDescId1.Id,
                itemDescId2.Id,
                itemDescId3.Id,
            });
            
            Assert.False(res.SingleOrDefault(description => description.Id ==itemDescId3.Id)?.Valid);
            Assert.Equal(3, res.Count);
        }

        [Fact]
        public async void InsertOrUpdateSuccess()
        {
            var itemDesc  = new DatabaseModel.ItemDescription("AWP AZIMOW D1asd", new decimal(10), "730", "2", "imageUrl",true);
            var itemDesc1 = new DatabaseModel.ItemDescription("AWP AZIMOW D1asd", new decimal(20), "730", "2", "imageUrl",true);

            var insertRes = await _itemDescriptionRepoService.InsertOrUpdate(itemDesc);
            var findRes   = await _itemDescriptionRepoService.FindAsync(insertRes.Id);

            Assert.Equal(new decimal(10), findRes.Value);

            var insertRes1 = await _itemDescriptionRepoService.InsertOrUpdate(itemDesc1);
            var findRes1   = await _itemDescriptionRepoService.FindAsync(insertRes1.Id);

            Assert.Equal(insertRes1.Id, insertRes.Id);
            Assert.Equal(findRes1.Id, insertRes.Id);
            Assert.Equal(new decimal(20), findRes1.Value);
        }

        [Fact]
        public async void GetFromNamesSuccess()
        {
            var itemDescription1 = new DatabaseModel.ItemDescription("AWP AZIMOW D1", new decimal(12.21), "730", "2", "imageUrl",true);
            var itemDescription2 = new DatabaseModel.ItemDescription("AWP AZIMOW D2", new decimal(11.21), "730", "2", "imageUrl",true);
            var itemDescription3 = new DatabaseModel.ItemDescription("AWP AZIMOW D3", new decimal(10.21), "730", "2", "imageUrl",true);
            var itemDescription4 = new DatabaseModel.ItemDescription("AWP AZIMOW D4", new decimal(10.21), "730", "2", "imageUrl",true);
            var itemDescId1      = await _itemDescriptionRepoService.InsertAsync(itemDescription1);
            var itemDescId2      = await _itemDescriptionRepoService.InsertAsync(itemDescription2);
            var itemDescId3      = await _itemDescriptionRepoService.InsertAsync(itemDescription3);
            await _itemDescriptionRepoService.InsertAsync(itemDescription4);

            var res = await _itemDescriptionRepoService.FindAsync(new List<string>
            {
                "AWP AZIMOW D1",
                "AWP AZIMOW D2",
                "AWP AZIMOW D3",
            });
            Assert.Equal(3, res.Count);
        }

        [Fact]
        public async void GetValueSuccess()
        {
            var itemDescription1 = new DatabaseModel.ItemDescription("AWP AZIMOW Q1", new decimal(12.21), "730", "2", "imageUrl",true);
            var itemDescription2 = new DatabaseModel.ItemDescription("AWP AZIMOW Q2", new decimal(11.21), "730", "2", "imageUrl",true);
            var itemDescription3 = new DatabaseModel.ItemDescription("AWP AZIMOW Q3", new decimal(10.21), "730", "2", "imageUrl",true);
            var itemDescription4 = new DatabaseModel.ItemDescription("AWP AZIMOW Q4", new decimal(1.21), "730", "2", "imageUrl",true);
            var itemDescId1      = await _itemDescriptionRepoService.InsertAsync(itemDescription1);
            var itemDescId2      = await _itemDescriptionRepoService.InsertAsync(itemDescription2);
            var itemDescId3      = await _itemDescriptionRepoService.InsertAsync(itemDescription3);
            var itemDescId4      = await _itemDescriptionRepoService.InsertAsync(itemDescription4);

            var res = await _itemDescriptionRepoService.ValueOfItemDescriptions(new List<int>
            {
                itemDescId1.Id,
                itemDescId2.Id,
                itemDescId3.Id,
                itemDescId4.Id,
            });


            Assert.Equal(new decimal(12.21), res[itemDescId1.Id]);
            Assert.Equal(new decimal(11.21), res[itemDescId2.Id]);
            Assert.Equal(new decimal(10.21), res[itemDescId3.Id]);
            Assert.Equal(new decimal(1.21), res[itemDescId4.Id]);
        }
    }
}
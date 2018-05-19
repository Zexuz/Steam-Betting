using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using FakeItEasy;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public class UserRepoIntegrationTestSetup
    {
        public string DatabaseName => "BettingTestUser";

        public UserRepoIntegrationTestSetup()
        {
            var connectionString = new ConnectionStringsForTest().GetConnectionString("master");
            new DatabaseHelperFactory().GetDatabaseHelperForType(Database.Main, connectionString, DatabaseName).ResetDatabase();
        }
    }


    public class UserRepoIntegrationTest : IClassFixture<UserRepoIntegrationTestSetup>
    {
        private readonly UserRepoService _userRepoService;

        public UserRepoIntegrationTest(UserRepoIntegrationTestSetup setup)
        {
            var connectionString   = new ConnectionStringsForTest().GetConnectionString(setup.DatabaseName);
            var databaseConnection = new DatabaseConnection(connectionString);
            var userRepository     = new QueryFactory().UserQueries;

            var fakedFactory = A.Fake<IDatabaseConnectionFactory>();
            A.CallTo(() => fakedFactory.GetDatabaseConnection(Factories.Database.Main)).Returns(databaseConnection);

            _userRepoService = new UserRepoService(fakedFactory, userRepository);
        }

        [Fact]
        public async Task UserInsertToDatabaseSuccessfuly()
        {
            var user = new DatabaseModel.User(
                "76561198077954113  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );

            var insertRes = await _userRepoService.InsertAsync(user);
            Assert.True(insertRes.Id > 0);

            var fetchedUser = await _userRepoService.FindAsync("76561198077954113");

            Assert.Equal("76561198077954113", fetchedUser.SteamId);
            Assert.Equal("REVANSCH", fetchedUser.Name);
            Assert.Equal("ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg", fetchedUser.ImageUrl);
            Assert.Equal("?partner=117688384&token=mn347bmb", fetchedUser.TradeLink);
            Assert.NotEqual(DateTime.MinValue, fetchedUser.Created);
            Assert.NotEqual(DateTime.MinValue, fetchedUser.LastActive);
            Assert.False(user.SuspendedFromQuote);
        }

        [Fact]
        public async Task GetRangeSuccess()
        {
            var user = new DatabaseModel.User(
                "5454  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );
            var user2 = new DatabaseModel.User(
                "5554  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );
            var user3 = new DatabaseModel.User(
                "5565  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );
            var user4 = new DatabaseModel.User(
                "5585  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );

            var insertRes  = await _userRepoService.InsertAsync(user);
            var insertRes1 = await _userRepoService.InsertAsync(user2);
            var insertRes2 = await _userRepoService.InsertAsync(user3);
            var insertRes3 = await _userRepoService.InsertAsync(user4);

            var fetchedUsers = await _userRepoService.FindAsync(new List<int>
            {
                insertRes1.Id,
                insertRes2.Id,
                insertRes3.Id,
            });

            Assert.Equal(3, fetchedUsers.Count);
        }
        
        [Fact]
        public async Task GetRangeBySteamIdSuccess()
        {
            var user = new DatabaseModel.User(
                "random15641  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );
            var user2 = new DatabaseModel.User(
                "random15642  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );
            var user3 = new DatabaseModel.User(
                "random15643  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );
            var user4 = new DatabaseModel.User(
                "random15644  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,
                false
            );

            var insertRes  = await _userRepoService.InsertAsync(user);
            var insertRes1 = await _userRepoService.InsertAsync(user2);
            var insertRes2 = await _userRepoService.InsertAsync(user3);
            var insertRes3 = await _userRepoService.InsertAsync(user4);

            var fetchedUsers = await _userRepoService.FindAsync(new List<string>
            {
                insertRes1.SteamId,
                insertRes2.SteamId,
                insertRes3.SteamId,
            });

            Assert.Equal(3, fetchedUsers.Count);
        }

        [Fact]
        public async Task UserInsertWithoutTradelinkToDatabaseSuccessfuly()
        {
            var user = new DatabaseModel.User(
                "76561178945361  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                null,
                DateTime.Now,
                DateTime.Now,
                false
            );

            var insertRes = await _userRepoService.InsertAsync(user);
            Assert.True(insertRes.Id > 0);

            var fetchedUser = await _userRepoService.FindAsync("76561178945361");

            Assert.Equal("76561178945361", fetchedUser.SteamId);
            Assert.Equal("REVANSCH", fetchedUser.Name);
            Assert.Equal("ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg", fetchedUser.ImageUrl);
            Assert.Null(fetchedUser.TradeLink);
            Assert.NotEqual(DateTime.MinValue, fetchedUser.Created);
            Assert.NotEqual(DateTime.MinValue, fetchedUser.LastActive);
        }


        [Fact]
        public async Task UserInsertThenUpdateSuccess()
        {
            var userToUpdate = new DatabaseModel.User(
                "76561198077954115  ",
                "   REVANSCH",
                " ba/bab154f01140ec39b2986d97838f0127534ec82d_full.jpg ",
                "?partner=117688384&token=mn347bmb ",
                DateTime.Now,
                DateTime.Now,false
            );


            var retUser = await _userRepoService.InsertAsync(userToUpdate);
            Assert.True(retUser.Id > 0);

            await _userRepoService.UpdateNameAsync("76561198077954115", "Kalle");
            Assert.Equal("Kalle", (await _userRepoService.FindAsync("76561198077954115")).Name);

            await _userRepoService.UpdateTradelinkAsync("76561198077954115", "?partner=117688384&token=ghhgfwaq");
            Assert.Equal("?partner=117688384&token=ghhgfwaq", (await _userRepoService.FindAsync("76561198077954115")).TradeLink);

            await _userRepoService.UpdateImageAsync("76561198077954115", "/someImage.jpg");
            Assert.Equal("/someImage.jpg", (await _userRepoService.FindAsync("76561198077954115")).ImageUrl);
        }

        [Fact]
        public async Task UserUpdateDoesNotExistsReturnThrows()
        {
            var exception1 = await Record.ExceptionAsync(async () =>
                await _userRepoService.UpdateImageAsync("4578", "imageUrl"));
            Assert.IsType(typeof(NoneExpectedResultException), exception1);

            var exception2 = await Record.ExceptionAsync(async () =>
                await _userRepoService.UpdateNameAsync("4578", "nameHere"));
            Assert.IsType(typeof(NoneExpectedResultException), exception2);

            var exception3 = await Record.ExceptionAsync(async () =>
                await _userRepoService.UpdateTradelinkAsync("4578", "newTreadelInk"));
            Assert.IsType(typeof(NoneExpectedResultException), exception3);
        }


        [Fact]
        public async Task UserGetThatDoesNotExistReturnsNull()
        {
            var res = await _userRepoService.FindAsync("5464658");
            Assert.Null(res);
        }

        [Fact]
        public async Task UpdateUserNameAndImageInSameQuerySuccess()
        {
            var userToInsert = new DatabaseModel.User("1457454844545", "Kalle", "imageUrl", null, DateTime.Now, DateTime.Now,false);
            await _userRepoService.InsertAsync(userToInsert);
            await _userRepoService.UpdateImageAndNameAsync("1457454844545", "newName", "newImageUrl");

            var user = await _userRepoService.FindAsync("1457454844545");
            Assert.Equal("newImageUrl", user.ImageUrl);
            Assert.Equal("newName", user.Name);
            Assert.Equal("1457454844545", user.SteamId);
        }

        [Fact]
        public async Task FindUserFromIdSuccess()
        {
            var userToInsert = new DatabaseModel.User("145745484454455", "Kalle", "imageUrl", null, DateTime.Now, DateTime.Now,false);
            var user         = await _userRepoService.InsertAsync(userToInsert);

            var res = await _userRepoService.FindAsync(user.Id);

            Assert.Equal("imageUrl", res.ImageUrl);
            Assert.Equal("Kalle", res.Name);
            Assert.Equal("145745484454455", res.SteamId);
        }

        [Fact]
        public async Task UpdateUserQuote()
        {
            var userToInsert = new DatabaseModel.User("14574548445454", "Kalle", "imageUrl", null, DateTime.Now, DateTime.Now,false);
            await _userRepoService.InsertAsync(userToInsert);
            await _userRepoService.UpdateQuoteAsync("14574548445454", "xD");

            var user = await _userRepoService.FindAsync("14574548445454");
            Assert.Equal("imageUrl", user.ImageUrl);
            Assert.Equal("Kalle", user.Name);
            Assert.Equal("14574548445454", user.SteamId);
            Assert.Equal("xD", user.Quote);
        }
        
        [Fact]
        public async Task UpdateUserQuoteIsSuspendedThrows()
        {
            var userToInsert = new DatabaseModel.User("45adshyuqklxvcnm", "Kalle", "imageUrl", null, DateTime.Now, DateTime.Now,true,"I'm gay");
            await _userRepoService.InsertAsync(userToInsert);
            await Assert.ThrowsAsync<UserSuspendedFromUpdatingQuoteException>(async () => await _userRepoService.UpdateQuoteAsync("45adshyuqklxvcnm", "xD"));
        }
    }
}
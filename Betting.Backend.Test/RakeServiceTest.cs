using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Services.Impl;
using Betting.Models.Models;
using Xunit;

namespace Betting.Backend.Test
{
    public class RakeServiceTest
    {
        private DatabaseModel.User      _winningUser;
        private DatabaseModel.Settings  _settings;
        private List<DatabaseModel.Bet> _bets;

        public RakeServiceTest()
        {
            _winningUser = new DatabaseModel.User("winnerSteamId", "", "", "", DateTime.Now, DateTime.Now, false, null, 1);
        }

        [Fact]
        public async Task WeTakeNoRakeReutrnsSuccess()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 100),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 50),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 50),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 50)
            };


            var res = rakeService.GetItemsThatWeShouldTake(0, bets, itemBets, _winningUser);

            Assert.Equal(0, res.ItemIdsToUs.Count);
            Assert.Equal(3, res.ItemIdsToWinner.Count);
        }

        [Fact]
        public async Task WeTakeRakeButThereAreNoItemsThatWeCanTakeDueToHighValueItemsSuccess()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 100),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 51),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 50),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 49)
            };


            var res = rakeService.GetItemsThatWeShouldTake(5, bets, itemBets, _winningUser);

            Assert.Equal(0, res.ItemIdsToUs.Count);
            Assert.Equal(3, res.ItemIdsToWinner.Count);
        }

        [Fact]
        public async Task WeTakeRakeButThereAreNoItemsThatWeCanTakeDueToWinnerNeedsToReceveAtleastOneItemSuccess()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 100),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 3),
            };


            var res = rakeService.GetItemsThatWeShouldTake(5, bets, itemBets, _winningUser);

            Assert.Equal(0, res.ItemIdsToUs.Count);
            Assert.Equal(1, res.ItemIdsToWinner.Count);
        }

        [Fact]
        public async Task WeTakeAllItemsExceptTheHighestOne()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 100),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 50),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId5", 3),
            };


            var res = rakeService.GetItemsThatWeShouldTake(50, bets, itemBets, _winningUser);

            Assert.Equal(3, res.ItemIdsToUs.Count);
            Assert.Equal(1, res.ItemIdsToWinner.Count);
        }

        [InlineData("DomainName")]
        [InlineData("DomainName.Com")]
        [InlineData("DomainName.Com")]
        [InlineData("domainname")]
        [Theory]
        public async Task WeTakeNoRakeBeacuseOfName(string name)
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 73),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 11),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 10),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId5", 3),
            };

            var user = new DatabaseModel.User("winnerSteamId", name, "", "", DateTime.Now, DateTime.Now, false, null, 1);


            var res = rakeService.GetItemsThatWeShouldTake(10, bets, itemBets, user);

            Assert.Equal(0, res.ItemIdsToUs.Count);
            Assert.Equal(4, res.ItemIdsToUs.Count + res.ItemIdsToWinner.Count);
            Assert.Equal(4, res.ItemIdsToWinner.Count);
        }

        [InlineData("DomainName")]
        [InlineData("DomainName.Com")]
        [InlineData("DomainName")]
        [InlineData("DomainName.come")]
        [Theory]
        public async Task WeTakeRakeBeacuseOfBadName(string name)
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 73),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 11),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 10),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId5", 3),
            };

            var user = new DatabaseModel.User("winnerSteamId", name, "", "", DateTime.Now, DateTime.Now, false, null, 1);


            var res = rakeService.GetItemsThatWeShouldTake(10, bets, itemBets, user);

            Assert.Equal(1, res.ItemIdsToUs.Count);
            Assert.Equal(4, res.ItemIdsToUs.Count + res.ItemIdsToWinner.Count);
            Assert.Equal(3, res.ItemIdsToWinner.Count);
        }

        [Fact]
        public async Task WeTakeRakeNormalSuccess1()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 73),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 11),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 10),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId5", 3),
            };


            var res = rakeService.GetItemsThatWeShouldTake(10, bets, itemBets, _winningUser);

            Assert.Equal(1, res.ItemIdsToUs.Count);
            Assert.Equal(4, res.ItemIdsToUs.Count + res.ItemIdsToWinner.Count);
            Assert.Equal(3, res.ItemIdsToWinner.Count);
        }


        [Fact]
        public async Task WeTakeRakeNormalSuccess2()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Now, 22),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 100),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 50),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId5", 3),
            };


            var res = rakeService.GetItemsThatWeShouldTake(10, bets, itemBets, _winningUser);

            Assert.Equal(3, res.ItemIdsToUs.Count);
            Assert.Equal(1, res.ItemIdsToWinner.Count);
        }

        [Fact]
        public async Task WeTakeRakeNormalSuccess3()
        {
            var rakeService = new RakeService();

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 0, 1, DateTime.Today, 11),
                new DatabaseModel.Bet(2, 0, 1, DateTime.Today, 22),
                new DatabaseModel.Bet(3, 0, 1, DateTime.Today, 33),
                new DatabaseModel.Bet(4, 0, 1, DateTime.Today, 44),
                new DatabaseModel.Bet(5, 0, 1, DateTime.Today, 55),
                new DatabaseModel.Bet(6, 0, 1, DateTime.Today, 66),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(11, 0, "assetId1", 100),
                new DatabaseModel.ItemBetted(22, 0, "assetId2", 50),
                new DatabaseModel.ItemBetted(22, 0, "assetId3", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId4", 3),
                new DatabaseModel.ItemBetted(22, 0, "assetId5", new decimal(0.5)),
                new DatabaseModel.ItemBetted(33, 0, "assetId6", new decimal(10.5)),
                new DatabaseModel.ItemBetted(33, 0, "assetId7", new decimal(45.47)),
                new DatabaseModel.ItemBetted(33, 0, "assetId8", new decimal(4.87)),
                new DatabaseModel.ItemBetted(44, 0, "assetId9", new decimal(78.0)),
                new DatabaseModel.ItemBetted(55, 0, "assetId10", new decimal(54.0)),
                new DatabaseModel.ItemBetted(44, 0, "assetId12", new decimal(5.0)),
                new DatabaseModel.ItemBetted(55, 0, "assetId13", new decimal(4.0)),
                new DatabaseModel.ItemBetted(66, 0, "assetId14", new decimal(8.0)),
                new DatabaseModel.ItemBetted(33, 0, "assetId15", new decimal(8.54)),
            };

            var sum = itemBets.Sum(i => i.Value);
            var x = itemBets.OrderBy(i => i.Value).ToList();
            //374.88
            //37.488
            //14 items - 1


            var res = rakeService.GetItemsThatWeShouldTake(10, bets, itemBets, _winningUser);

            Assert.Equal(6, res.ItemIdsToUs.Count);
            Assert.Equal(7, res.ItemIdsToWinner.Count);
        }
    }
}
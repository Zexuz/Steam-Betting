using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
using Xunit;

namespace Betting.Backend.Test
{
    public class DraftServiceTest
    {
        private          JackpotDraftService  _service;
        private readonly IRepoServiceFactory  _fakedRepoServiceFacotry;
        private readonly ILogServiceFactory   _fakedLogServiceFactory;
        private readonly IRakeService         _fakedRakeService;
        private          IRakeItemRepoService _fakedRakeItemRepoService;
        private          IItemRepoService     _fakedItemRepoService;

        public DraftServiceTest()
        {
            _fakedRepoServiceFacotry = A.Fake<IRepoServiceFactory>();
            _fakedLogServiceFactory = A.Fake<ILogServiceFactory>();
            _fakedRakeService = A.Fake<IRakeService>();

            _fakedItemRepoService = A.Fake<IItemRepoService>();
            _fakedRakeItemRepoService = A.Fake<IRakeItemRepoService>();

            A.CallTo(() => _fakedRepoServiceFacotry.ItemRepoService).Returns(_fakedItemRepoService);
            A.CallTo(() => _fakedRepoServiceFacotry.RakeItemRepoService).Returns(_fakedRakeItemRepoService);

            _service = new JackpotDraftService(_fakedRepoServiceFacotry, _fakedLogServiceFactory, _fakedRakeService);
        }

        [Fact]
        public async Task ChangeOwnerOfItemsSuccess()
        {
            var matchId = 1;
            decimal rake = 10;
            var gameModeId = 2;
            var winningUserId = 1337;

            var bets = new List<DatabaseModel.Bet>();
            var items = new List<DatabaseModel.ItemBetted>();
            var winningUser = new DatabaseModel.User("steamId", "name", "img", "tradelin", DateTime.Now, DateTime.Now, false, null, winningUserId);

            A.CallTo(() => _fakedRakeService.GetItemsThatWeShouldTake(rake, bets, items, winningUser)).Returns(new RakeService.RakeResult
            {
                ItemIdsToUs = new List<AssetAndDescriptionId>
                {
                    new AssetAndDescriptionId
                    {
                        AssetId = "1",
                        DescriptionId = 2,
                    }
                },
                ItemIdsToWinner = new List<AssetAndDescriptionId>
                {
                    new AssetAndDescriptionId
                    {
                        AssetId = "2",
                        DescriptionId = 2,
                    },
                    new AssetAndDescriptionId
                    {
                        AssetId = "3",
                        DescriptionId = 2,
                    },
                    new AssetAndDescriptionId
                    {
                        AssetId = "4",
                        DescriptionId = 2,
                    },
                }
            });

            A.CallTo(() => _fakedItemRepoService.FindAsync(A<List<AssetAndDescriptionId>>._)).Returns(new List<DatabaseModel.Item>
            {
                new DatabaseModel.Item("1", 2, 3, 5, DateTimeOffset.Now)
            });

            await _service.ChangeOwnerOfItems(bets, items, winningUser, matchId, rake, gameModeId);
            
            A.CallTo(() => _fakedItemRepoService.ChangeOwner(A<List<AssetAndDescriptionId>>.That.Matches(list => list.Count == 3),winningUser)).MustHaveHappened();

            A.CallTo(() => _fakedRakeItemRepoService.InsertAsync(A<List<DatabaseModel.RakeItem>>.That.Matches(
                list => 
                    list.Count == 1 &&
                    list[0].AssetId == "1" &&
                    list[0].DescriptionId == 2 &&
                    list[0].GameModeId== gameModeId &&
                    list[0].MatchId == matchId
            ), null)).MustHaveHappened();

            A.CallTo(() => _fakedRakeItemRepoService.InsertAsync(A<List<DatabaseModel.RakeItem>>.That.Matches(list =>
                    list.Count            == 1   &&
                    list[0].AssetId       == "1" &&
                    list[0].DescriptionId == 2
                ), null
            )).MustHaveHappened();

            A.CallTo(() => _fakedRakeService.GetItemsThatWeShouldTake(rake, bets, items, winningUser)).MustHaveHappened();
        }

        [Fact]
        public void DraftWinnerSuccess1()
        {
//            var winPercentage = 49.548644548454;
//            var totalTickets = 5000;
//            var winningTicket = new DraftService().GetWinnigTicket(totalTickets, winPercentage);
//            Assert.Equal(2477, winningTicket);

            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 10),
                new DatabaseModel.Bet(3, 12, 1, DateTime.Today.AddMilliseconds(1), 15),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 11),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(10, 0, "assetId", new decimal(24.77)),
                new DatabaseModel.ItemBetted(11, 0, "assetId", new decimal(25.23)),
                new DatabaseModel.ItemBetted(15, 0, "assetId", new decimal(0.01)) // <---- Winner
            };

            var winningBet = _service.GetWinningBet(49.548644548454, bets, itemBets);

            Assert.Equal(2477, winningBet.WinningTicket);
            Assert.Equal(15, winningBet.Bet.Id);
        }

        [Fact]
        public void DraftWinnerSuccess2()
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 10),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(548.45)),
                new DatabaseModel.ItemBetted(10, 0, "assetId", new decimal(54.4)) // <---- Winner
            };


            var winningBet = _service.GetWinningBet(2.548451581, bets, itemBets);

            Assert.Equal(1536, winningBet.WinningTicket);
            Assert.Equal(10, winningBet.Bet.Id);
        }

        [Fact]
        public void DraftWinnerSuccess3()
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(548.45)),
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(54.4)) // <---- Winner
            };


            var winningBet = _service.GetWinningBet(2, bets, itemBets);

            Assert.Equal(1205, winningBet.WinningTicket);
            Assert.Equal(1, winningBet.Bet.Id);
        }

        [Fact]
        public void DraftWinnerSuccess4()
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
                new DatabaseModel.Bet(3, 12, 1, DateTime.Today.AddSeconds(2), 3),
                new DatabaseModel.Bet(4, 12, 1, DateTime.Today.AddSeconds(3), 4),
                new DatabaseModel.Bet(5, 12, 1, DateTime.Today.AddSeconds(4), 5),
                new DatabaseModel.Bet(6, 12, 1, DateTime.Today.AddSeconds(5), 6),
                new DatabaseModel.Bet(7, 12, 1, DateTime.Today.AddSeconds(6), 7),
                new DatabaseModel.Bet(8, 12, 1, DateTime.Today.AddSeconds(7), 8),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(548.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(0.03)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(0.03)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(0.03)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(10.43)),
                new DatabaseModel.ItemBetted(3, 0, "assetId", new decimal(100.54)),
                new DatabaseModel.ItemBetted(3, 0, "assetId", new decimal(100.54)),
                new DatabaseModel.ItemBetted(3, 0, "assetId", new decimal(100.54)),
                new DatabaseModel.ItemBetted(3, 0, "assetId", new decimal(100.54)),
                new DatabaseModel.ItemBetted(4, 0, "assetId", new decimal(54.4)),
                new DatabaseModel.ItemBetted(4, 0, "assetId", new decimal(54.4)),
                new DatabaseModel.ItemBetted(4, 0, "assetId", new decimal(8.4)),
                new DatabaseModel.ItemBetted(5, 0, "assetId", new decimal(0.47)),
                new DatabaseModel.ItemBetted(5, 0, "assetId", new decimal(0.48)),
                new DatabaseModel.ItemBetted(5, 0, "assetId", new decimal(0.87)),
                new DatabaseModel.ItemBetted(6, 0, "assetId", new decimal(0.3)),
                new DatabaseModel.ItemBetted(7, 0, "assetId", new decimal(0.3)),
                new DatabaseModel.ItemBetted(8, 0, "assetId", new decimal(4.0)),
                new DatabaseModel.ItemBetted(8, 0, "assetId", new decimal(0.4)) // <---- Winner
            };

            //1085,15
            //108515


            var winningBet = _service.GetWinningBet(99.9999999999, bets, itemBets);

            Assert.Equal(108514, winningBet.WinningTicket);
            Assert.Equal(8, winningBet.Bet.Id);
        }

        [Fact]
        public void DraftWinnerSuccess5()
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(548.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(100.45)),
            };

            var sum = itemBets.Sum(item => item.Value);

            //1085,15
            //108515
            var res = Math.Floor(new decimal(1));

            var winningBet = _service.GetWinningBet(99.9999999999, bets, itemBets);

            Assert.Equal(205519, winningBet.WinningTicket);
            Assert.Equal(2, winningBet.Bet.Id);
        }

        [Fact]
        public void DraftWinnerSuccess6()
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(6000.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(9999.45)),
            };

            var sum = itemBets.Sum(item => item.Value);

            //155992.20
            //15599220

            var winningBet = _service.GetWinningBet(99.9999999999, bets, itemBets);

            Assert.Equal(15599219, winningBet.WinningTicket);
            Assert.Equal(2, winningBet.Bet.Id);
        }

        [Fact]
        public void DraftWinnerSuccess7()
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(0.01)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(0.01)),
            };

            var sum = itemBets.Sum(item => item.Value);

            //155992.20
            //15599220

            var winningBet = _service.GetWinningBet(99.9999999999, bets, itemBets);

            Assert.Equal(1, winningBet.WinningTicket);
            Assert.Equal(2, winningBet.Bet.Id);
        }

        [InlineData(45)]
        [InlineData(10)]
        [InlineData(0)]
        [InlineData(49.99999999)]
        [InlineData(50.00000000)]
        [Theory]
        public void DraftWinnerSuccess8(double percentage)
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(0.01)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(0.01)),
            };

            var sum = itemBets.Sum(item => item.Value);

            //155992.20
            //15599220

            var winningBet = _service.GetWinningBet(percentage, bets, itemBets);

            Assert.Equal(0, winningBet.WinningTicket);
            Assert.Equal(1, winningBet.Bet.Id);
        }

        [InlineData(50.000000001)]
        [InlineData(51)]
        [InlineData(95)]
        [InlineData(99.99999999)]
        [Theory]
        public void DraftWinnerSuccess9(double percentage)
        {
            var bets = new List<DatabaseModel.Bet>
            {
                new DatabaseModel.Bet(1, 12, 1, DateTime.Today, 1),
                new DatabaseModel.Bet(2, 12, 1, DateTime.Today.AddSeconds(1), 2),
            };

            var itemBets = new List<DatabaseModel.ItemBetted>
            {
                new DatabaseModel.ItemBetted(1, 0, "assetId", new decimal(0.01)),
                new DatabaseModel.ItemBetted(2, 0, "assetId", new decimal(0.01)),
            };

            var sum = itemBets.Sum(item => item.Value);

            //155992.20
            //15599220

            var winningBet = _service.GetWinningBet(percentage, bets, itemBets);

            Assert.Equal(1, winningBet.WinningTicket);
            Assert.Equal(2, winningBet.Bet.Id);
        }
    }
}
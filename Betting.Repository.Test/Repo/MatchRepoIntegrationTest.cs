using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Impl;
using Betting.Repository.Services.Impl;
using Betting.Repository.Services.Interfaces;
using Xunit;

namespace Betting.Repository.Test.Repo
{
    public sealed class MatchRepoIntegrationTestSetup : RepoServiceTestBase
    {
        public             DatabaseModel.GameMode GameMode     { get; private set; }
        protected override string                 DatabaseName => "BettingTestMatch";

        public MatchRepoIntegrationTestSetup()
        {
            InitTest().Wait();
        }

        protected override async Task InitTest()
        {
            await base.InitTest();
            var gameModeRepoService = new GameModeRepoService(FakedFactory);

            GameMode = new DatabaseModel.GameMode("GameMode1", 1);
            await gameModeRepoService.Insert(GameMode);
        }
    }

    public class MatchRepoIntegrationTest : IClassFixture<MatchRepoIntegrationTestSetup>
    {
        private readonly MatchRepoIntegrationTestSetup _setup;
        private readonly MatchRepoService              _matchRepoService;
        private readonly UserRepoService               _userRepoService;

        public MatchRepoIntegrationTest(MatchRepoIntegrationTestSetup setup)
        {
            _setup            = setup;
            _matchRepoService = new MatchRepoService(_setup.FakedFactory, new MatchQueries());
            _userRepoService  = new UserRepoService(_setup.FakedFactory, new UserQueries());
        }

        [Fact]
        public async void MatchInsertThenGetThatMatchSuccess()
        {
            var match = new DatabaseModel.Match(1, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId,
                _setup.GameMode.Id, DateTime.Today);

            var returnMatch = await _matchRepoService.InsertAsync(match);
            Assert.True(returnMatch.Id > 0);


            var selectRes = await _matchRepoService.FindAsync(1);
            Assert.Equal(1, selectRes.RoundId);
            Assert.Equal(42.31767743933838.ToString(CultureInfo.InvariantCulture), selectRes.Percentage);
            Assert.Equal("hash", selectRes.Hash);
            Assert.Equal("salt", selectRes.Salt);
            Assert.Equal(DateTime.Today, selectRes.Created);
            Assert.Null(selectRes.TimerStarted);
        }


        [Fact]
        public async void GetCurrentMatchSucces()
        {
            var matches = new List<DatabaseModel.Match>
            {
                new DatabaseModel.Match(5, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                    DateTime.Today),
                new DatabaseModel.Match(10, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                    DateTime.Today),
                new DatabaseModel.Match(154, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                    DateTime.Today),
                new DatabaseModel.Match(15, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                    DateTime.Today),
                new DatabaseModel.Match(541, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                    DateTime.Today),
            };

            foreach (var match in matches)
            {
                await _matchRepoService.InsertAsync(match);
            }

            var selectRes = await _matchRepoService.GetCurrentMatch();
            Assert.Equal(541, selectRes.RoundId);
        }

        [Fact]
        public async void UpdateTimerOnMatch()
        {
            var match = new DatabaseModel.Match(245, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId,
                _setup.GameMode.Id, DateTime.Today);
            await _matchRepoService.InsertAsync(match);

            var selectRes1 = await _matchRepoService.FindAsync(match.RoundId);
            Assert.Null(selectRes1.TimerStarted);

            await _matchRepoService.StartTimerForMatch(match.RoundId, DateTime.Today);
            var selectRes2 = await _matchRepoService.FindAsync(match.RoundId);

            Assert.Equal(245, selectRes2.RoundId);
            Assert.Equal(DateTime.Today, selectRes2.TimerStarted);
        }

        [Fact]
        public async void CloseMatchSuccess()
        {
            var match = new DatabaseModel.Match(246, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId,
                _setup.GameMode.Id, DateTime.Today);
            await _matchRepoService.InsertAsync(match);

            await _matchRepoService.CloseMatch(match.RoundId);
            var selectRes2 = await _matchRepoService.FindAsync(match.RoundId);

            Assert.Equal(246, selectRes2.RoundId);
            Assert.Equal(0, selectRes2.Status);
        }

        [Fact]
        public async void AddWinnerToMatch()
        {
            var winner = await _userRepoService.InsertAsync("randomSteamId1", "randomName", "imgUr,");
            var match  = new DatabaseModel.Match(178, "salt", "hash", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId,
                _setup.GameMode.Id, DateTime.Today);

            await _matchRepoService.InsertAsync(match);
            await _matchRepoService.CloseMatch(match.RoundId);
            await _matchRepoService.AddWinnerToMatch(winner, match.RoundId);
            var selectRes2 = await _matchRepoService.FindAsync(match.RoundId);

            Assert.Equal(178, selectRes2.RoundId);
            Assert.Equal(0, selectRes2.Status);
            Assert.Equal(winner.Id, selectRes2.WinnerId);
        }

        [Fact]
        public async void GetMatchesUserWon()
        {
            var winner    = await _userRepoService.InsertAsync("randomSteamId2", "randomName", "imgUr,");
            var notWinner = await _userRepoService.InsertAsync("randomSteamI54", "randomName", "imgUr,");

            await _matchRepoService.InsertAsync(new DatabaseModel.Match(179, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(181, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(182, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, notWinner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(183, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(184, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            var selectRes = await _matchRepoService.GetMatchesUserWon(winner, 20, 0);

            Assert.Equal(3, selectRes.Count);
        }

        [Fact]
        public async void GetMatchIdsUserWon()
        {
            var winner = await _userRepoService.InsertAsync("randomSteamId3", "randomName", "imgUr,");

            await _matchRepoService.InsertAsync(new DatabaseModel.Match(199, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(191, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(192, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(193, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(194, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(195, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            var selectRes = await _matchRepoService.GetMatchIdsUserWon(winner);

            Assert.Equal(4, selectRes.Count);
        }

        [Fact]
        public async void GetMatchesFromRoundId()
        {
            var winner = await _userRepoService.InsertAsync("randomSteamId4", "randomName", "imgUr,");

            await _matchRepoService.InsertAsync(new DatabaseModel.Match(299, "hash299", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(291, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(292, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, null,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(293, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(294, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, null,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            await _matchRepoService.InsertAsync(new DatabaseModel.Match(295, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 0, null, winner.Id,
                _setup.GameMode.CurrentSettingId, _setup.GameMode.Id, DateTime.Now));
            var selectRes = await _matchRepoService.FindAsync(new List<int>
            {
                299,
                291,
                292,
                293,
                294,
                295,
            });

            Assert.Equal(6, selectRes.Count);
            Assert.True(selectRes.Find(m => m.RoundId == 299).Salt == "hash299");
        }


        [Fact]
        public async void InsertPercentageSuccess()
        {
            var match = new DatabaseModel.Match(300, "hash", "salt", 42.31767743933838.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId,
                _setup.GameMode.Id, DateTime.Now);
            var match1 = new DatabaseModel.Match(301, "hash", "salt", 99.0.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                DateTime.Now);
            var match2 = new DatabaseModel.Match(302, "hash", "salt", 98.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                DateTime.Now);
            var match3 = new DatabaseModel.Match(303, "hash", "salt", 1.0000001.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                DateTime.Now);
            var match4 = new DatabaseModel.Match(304, "hash", "salt", 58.448484131151515145.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId,
                _setup.GameMode.Id, DateTime.Now);
            var match5 = new DatabaseModel.Match(305, "hash", "salt", 54.1215110.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.CurrentSettingId, _setup.GameMode.Id,
                DateTime.Now);
            await _matchRepoService.InsertAsync(match);
            await _matchRepoService.InsertAsync(match1);
            await _matchRepoService.InsertAsync(match2);
            await _matchRepoService.InsertAsync(match3);
            await _matchRepoService.InsertAsync(match4);
            await _matchRepoService.InsertAsync(match5);

            Assert.Equal(42.31767743933838.ToString(CultureInfo.InvariantCulture), (await _matchRepoService.FindAsync(300)).Percentage);
            Assert.Equal(99.0.ToString(CultureInfo.InvariantCulture), (await _matchRepoService.FindAsync(301)).Percentage);
            Assert.Equal(98.ToString(CultureInfo.InvariantCulture), (await _matchRepoService.FindAsync(302)).Percentage);
            Assert.Equal(1.0000001.ToString(CultureInfo.InvariantCulture), (await _matchRepoService.FindAsync(303)).Percentage);
            Assert.Equal(58.448484131151515145.ToString(CultureInfo.InvariantCulture), (await _matchRepoService.FindAsync(304)).Percentage);
            Assert.Equal(54.1215110.ToString(CultureInfo.InvariantCulture), (await _matchRepoService.FindAsync(305)).Percentage);
        }

        [Fact]
        public async void InsertMatchWithNoGamdeModeIdThrows()
        {
            var match = new DatabaseModel.Match(310, "hash", "salt", 54.1215110.ToString(CultureInfo.InvariantCulture), 1, null, null, _setup.GameMode.Id, 0, DateTime.Now);
            await Assert.ThrowsAsync<SqlException>(async () => await _matchRepoService.InsertAsync(match));
        }

        [Fact]
        public async void InsertMatchWithNoSettingIdThrows()
        {
            var match = new DatabaseModel.Match(310, "hash", "salt", 54.1215110.ToString(CultureInfo.InvariantCulture), 1, null, null, 0, _setup.GameMode.Id, DateTime.Now);
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _matchRepoService.InsertAsync(match));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Shared;

namespace Betting.Repository.Services
{
    public class MongoJackpotRepoService : IMongoJackpotRepoService
    {
        private readonly IMongoDbConnectionFacotry _mongoDbConnectionFacotry;

        public MongoJackpotRepoService(IMongoDbConnectionFacotry mongoDbConnectionFacotry)
        {
            _mongoDbConnectionFacotry = mongoDbConnectionFacotry;
        }

        public async Task SetWinnerTicketForMatch(DatabaseModel.CoinFlip match, int ticket)
        {
            var findFilter = GetFindFilter().Where(m => m.LookUpId == match.Id);
            var updateFilter = GetUpdateFilter().Set(m => m.WinningTicket, ticket);
            await GetCollection().UpdateOneAsync(findFilter, updateFilter);
        }

        public async Task SetWinnerForMatch(DatabaseModel.CoinFlip match, UserWithQuote user)
        {
            var findFilter = GetFindFilter().Where(m => m.LookUpId == match.Id);
            var updateFilter = GetUpdateFilter().Set(m => m.Winner, user);
            await GetCollection().UpdateOneAsync(findFilter, updateFilter);
        }


        public async Task UpdateTimmerStartedForMatch(DatabaseModel.CoinFlip match, DateTime timer)
        {
            var findFilter = GetFindFilter().Where(m => m.LookUpId == match.Id);
            var updateFilter = GetUpdateFilter().Set(m => m.TimerStarted, timer);
            await GetCollection().UpdateOneAsync(findFilter, updateFilter);
        }

        public async Task UpdateStatusForMatch(DatabaseModel.CoinFlip match, MatchStatus status)
        {
            var findFilter = GetFindFilter().Where(m => m.LookUpId == match.Id);
            var updateFilter = GetUpdateFilter().Set(m => m.Status, (int) status);
            await GetCollection().UpdateOneAsync(findFilter, updateFilter);
        }

        public async Task AddBetToMatch(DatabaseModel.CoinFlip match, CoinFlipBet bet)
        {
            var findFilter = GetFindFilter().Where(m => m.LookUpId == match.Id);
            var updateFilter = GetUpdateFilter().Push(m => m.Bets, bet);
            await GetCollection().UpdateOneAsync(findFilter, updateFilter);
        }

        public async Task<List<CoinFlipMatch>> GetAllOpenOrDraftinMatchesFromMongoDb()
        {
            var filter = GetFindFilter().Where(match => match.Status != (int) MatchStatus.Closed);
            var res = await GetCollection().FindAsync(filter);
            return res.Select(i => new CoinFlipMatch(i)).Reverse().ToList();
        }

        public async Task<Result<CoinFlipMatchHistory>> GetHistory(int start, int count)
        {
            var filter = GetFindFilter().Where(match => match.Status == MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Closed));

            var res = await MongoDbHelper.Query(GetCollection().Collection, filter, start, count, match => match.TimerStarted);

            var result = new Result<CoinFlipMatchHistory>
            {
                CurrentIndex = start,
                Total = res.Total,
                Data = res.Data.Select(i => new CoinFlipMatchHistory(i)).ToList()
            };

            return result;
        }

        public async Task<Result<CoinFlipMatchHistory>> GetPersonalHistory(int start, int count, string steamId)
        {
            var betFilter = new BsonDocumentFilterDefinition<MongoDbModels.JackpotMatch>(new BsonDocument("Bets.User.SteamId", steamId));

            var filter = GetFindFilter().And
            (
                betFilter,
                GetFindFilter().Where(match => match.Status == MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Closed))
            );

            var res = await MongoDbHelper.Query(GetCollection().Collection, filter, start, count, match => match.TimerStarted);

            var result = new Result<CoinFlipMatchHistory>
            {
                CurrentIndex = start,
                Total = res.Total,
                Data = res.Data.Select(i => new CoinFlipMatchHistory(i)).ToList()
            };

            return result;
        }


        public async Task<CoinFlipMatchHistory> FindMatchFromRoundId(int lookUpId)
        {
            var filter = GetFindFilter().Where(match => match.LookUpId == lookUpId);
            var res = await GetCollection().FindSingleOrDefaultAsync(filter);
            return new CoinFlipMatchHistory(res);
        }

        public async Task InsertAsync(MongoDbModels.JackpotMatch coinFlipMatchModel)
        {
            await GetCollection().InsertOneAsync(coinFlipMatchModel);
        }

        private FilterDefinitionBuilder<MongoDbModels.JackpotMatch> GetFindFilter()
        {
            return Builders<MongoDbModels.JackpotMatch>.Filter;
        }

        private UpdateDefinitionBuilder<MongoDbModels.JackpotMatch> GetUpdateFilter()
        {
            return Builders<MongoDbModels.JackpotMatch>.Update;
        }

        private IGenericCollection<MongoDbModels.JackpotMatch> GetCollection()
        {
            return _mongoDbConnectionFacotry.GetCollection<MongoDbModels.JackpotMatch>();
        }
    }
}
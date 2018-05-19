using System;
using System.Linq;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Interfaces;
using MongoDB.Driver;

namespace Betting.Repository.Services
{
    public class MongoPreHashRepoService : IMongoPreHashRepoService
    {
        private readonly IMongoDbConnectionFacotry _mongoDbConnectionFacotry;

        public MongoPreHashRepoService(IMongoDbConnectionFacotry mongoDbConnectionFacotry)
        {
            _mongoDbConnectionFacotry = mongoDbConnectionFacotry;
        }

        public async Task Insert(MongoDbModels.PreHash hash)
        {
            await GetCollection().InsertOneAsync(hash);
        }

        public async Task<MongoDbModels.PreHash> Find(string hash, string steamId)
        {
            var hashFilter = GetFindFilter().Eq(p => p.Hash, hash);
            var userFilter = GetFindFilter().Eq(p => p.UserSteamId, steamId);
            var filter = GetFindFilter().And(hashFilter, userFilter);

            var res = await GetCollection().FindAsync(filter);

            MongoDbModels.PreHash preHash;

            try
            {
                preHash = res.Single();
            }
            catch (Exception e)
            {
                throw new PreHashNotFoundException($"No prehash found for user {steamId} with hash {hash}");
            }

            if (DateTime.Now - preHash.Created > TimeSpan.FromMinutes(30))
                throw new ToOldPreHashException("The pre hash has expired.");

            return preHash;
        }

        private FilterDefinitionBuilder<MongoDbModels.PreHash> GetFindFilter()
        {
            return Builders<MongoDbModels.PreHash>.Filter;
        }

        private IGenericCollection<MongoDbModels.PreHash> GetCollection()
        {
            return _mongoDbConnectionFacotry.GetCollection<MongoDbModels.PreHash>();
        }
    }
}
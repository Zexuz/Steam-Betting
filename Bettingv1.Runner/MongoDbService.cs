using System;
using System.Threading.Tasks;
using Bettingv1.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using RpcCommunicationHistory;
using Shared.Shared;

namespace Bettingv1.Runner
{
    public class MongoDbService
    {
        private IMongoCollection<MatchModel> _collection;

        public MongoDbService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("mydb");
            _collection = database.GetCollection<MatchModel>("Bettingv1History");
        }


        public async Task<Result<MatchModel>> GetGlobalHistory(GetGlobalHistoryRequest request)
        {
            return await MongoDbHelper.Query(_collection, model => true, request.Offset, request.Limt);
        }

        public async Task<Result<MatchModel>> GetPersonalHistory(GetPersonalHistoryRequest request)
        {
            var filter = new BsonDocumentFilterDefinition<MatchModel>(new BsonDocument("Bets.User.SteamId", request.SteamId));
            return await MongoDbHelper.Query(_collection, filter, request.Offset, request.Limt);
        }

        public void Print()
        {
            var data = MongoDbHelper.Query(_collection, model => model.RoundId > 10, 100, 10).Result;

            Console.WriteLine($"Total: {data.Total}");
            Console.WriteLine($"From: {data.CurrentIndex}");

            foreach (var matchModel in data.Data)
            {
                Console.WriteLine($"{matchModel.RoundId}, {matchModel.Value}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using RpcCommunication;

namespace CsgoDraffle.Logging
{
    public class MongoLoggingService
    {
        private IMongoDatabase _database;

        public MongoLoggingService(string databaseName, string url)
        {
            var factory = new MongoConnectionFactory();
            var client  = factory.GetMongoCleint(url);


            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>() where T : class
        {
            return _database.GetCollection<T>(typeof(T).Name);
        }

        public bool IsOnline()
        {
            return _database.RunCommandAsync((Command<BsonDocument>) "{ping:1}").Wait(1000);
        }

        public async Task AddErrorLog(LogError logError)
        {
            var logErrorCollection = GetCollection<ErrorLogIndex>();
            var toInsert           = new ErrorLogIndex
            {
                ErrorPath   = logError.ErrorPath,
                Message     = logError.Message,
                StackTrace  = logError.StackTrace,
                Time        = DateTime.FromBinary(Convert.ToInt64(logError.Time)),
                UserSteamId = logError.UserSteamId
            };
            await logErrorCollection.InsertOneAsync(toInsert);
        }

        public async Task<List<LogError>> GetErrorLogs(DateTime? start, DateTime? end, string userSteamId)
        {
            var logErrorCollection = GetCollection<ErrorLogIndex>();
            var res                = new List<ErrorLogIndex>();

            if (start.HasValue && end.HasValue && !string.IsNullOrEmpty(userSteamId))
                res = await logErrorCollection.Find(index =>
                    index.UserSteamId == userSteamId
                    && index.Time     > start
                    && index.Time     < end
                ).ToListAsync();
            else if (start                                                     == null && end == null)
                res = await logErrorCollection.Find(index => index.UserSteamId == userSteamId).ToListAsync();
            else if (userSteamId                                               == null)
                res = await logErrorCollection.Find(index => index.Time        > start && index.Time < end).ToListAsync();

            return res.Select(index => new LogError
            {
                ErrorPath   = index.ErrorPath,
                Message     = index.Message,
                StackTrace  = index.StackTrace,
                Time        = index.Time.ToBinary().ToString(),
                UserSteamId = index.UserSteamId
            }).ToList();
        }

        public async Task AddOrUpdateUserLog(UserLogIndex userLog)
        {
            var userLogCollection = GetCollection<UserLogIndex>();
            if (DoesExist(userLog.SteamId, userLogCollection))
            {
                await UpdateAsync(userLogCollection, userLog.SteamId, userLog.Logs[0]);
                return;
            }

            await InsertAsync(userLogCollection, userLog);
        }

        public async Task<UserLogIndex> FindUserLog(string steamId)
        {
            var userCollection = GetCollection<UserLogIndex>();
            var res            = await userCollection.Find(x => x.SteamId == steamId).ToListAsync();
            if (res.Count                                                 == 0)
                return new UserLogIndex
                {
                    SteamId = steamId,
                    Logs    = new List<Log>()
                };
            return res.First();
        }

        private async Task UpdateAsync(IMongoCollection<UserLogIndex> collection, string steamid, Log log)
        {
            var filter = Builders<UserLogIndex>.Filter.Eq(l => l.SteamId, steamid);
            var update = Builders<UserLogIndex>.Update.AddToSet(l => l.Logs, log);
            await collection.UpdateOneAsync(filter, update);
        }

        private async Task InsertAsync(IMongoCollection<UserLogIndex> collection, UserLogIndex userLogIndex)
        {
            await collection.InsertOneAsync(userLogIndex);
        }

        private bool DoesExist(string steamId, IMongoCollection<UserLogIndex> collection)
        {
            return collection.Find(index => index.SteamId == steamId).Count() > 0;
        }
    }
}
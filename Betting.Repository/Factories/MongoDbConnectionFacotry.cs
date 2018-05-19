using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Betting.Repository.Factories
{
    public class MongoDbConnectionFacotry : IMongoDbConnectionFacotry
    {
        private readonly IMongoDatabase _database;

        public MongoDbConnectionFacotry(IConfiguration configuration)
        {
            var mongosection = configuration.GetSection("MongoDb");
            var connectionString = mongosection.GetSection("ConnectionString").Value;
            var databaseName = mongosection.GetSection("Database").Value;

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IGenericCollection<TEntity> GetCollection<TEntity>() where TEntity : class
        {
            var collection = _database.GetCollection<TEntity>(typeof(TEntity).Name);
            return new GenericCollection<TEntity>(collection);
        }
    }
}
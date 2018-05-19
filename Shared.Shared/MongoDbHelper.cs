using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Shared.Shared
{
    public static class MongoDbHelper
    {
        public static async Task<Result<T>> Query<T>(IMongoCollection<T> collection, Expression<Func<T, bool>> filter, int skip, int limit)
        {
            var query = collection.Find(filter);
            return await GetResult(skip, limit, query);
        }

        public static async Task<Result<T>> Query<T>(IMongoCollection<T> collection, BsonDocumentFilterDefinition<T> filter, int skip, int limit)
        {
            var query = collection.Find(filter);
            return await GetResult(skip, limit, query);
        }
        
        public static async Task<Result<T>> Query<T>(IMongoCollection<T> collection, FilterDefinition<T> filter, int skip, int limit, Expression<Func<T, object>> sortExpression)
        {
            var query = collection.Find(filter).SortByDescending(sortExpression);
            return await GetResult(skip, limit, query);
        }

        private static async Task<Result<T>> GetResult<T>(int skip, int limit, IFindFluent<T, T> query)
        {
            var result = new Result<T>();

            var totalTask = query.CountAsync();
            var itemsTask = query.Skip(skip).Limit(limit).ToListAsync();

            await Task.WhenAll(totalTask, itemsTask);

            result.Total = totalTask.Result;
            result.Data = itemsTask.Result;

            return result;
        }
    }
}
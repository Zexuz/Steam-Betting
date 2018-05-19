using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Repository.Interfaces;
using MongoDB.Driver;

namespace Betting.Repository.Impl
{
    public class GenericCollection<TEntity> : IGenericCollection<TEntity> where TEntity : class
    {
        public IMongoCollection<TEntity> Collection { get; }

        private readonly IMongoCollection<TEntity> _collection;

        public GenericCollection(IMongoCollection<TEntity> collection)
        {
            _collection = collection;
            Collection = _collection;
        }

        public async Task InsertOneAsync(TEntity entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task InsertManyAsync(IEnumerable<TEntity> entities)
        {
            await _collection.InsertManyAsync(entities);
        }

        public async Task UpdateOneAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition)
        {
            await _collection.UpdateOneAsync(filterDefinition, updateDefinition);
        }

        public async Task<List<TEntity>> FindAsync(FilterDefinition<TEntity> filterDefinition)
        {
            var res = await _collection.FindAsync(filterDefinition);
            return await res.ToListAsync();
        }

        public async Task<TEntity> FindSingleOrDefaultAsync(FilterDefinition<TEntity> filterDefinition)
        {
            var query = _collection.Find(filterDefinition);
            var res = await query.Limit(1).ToListAsync();
            return res.FirstOrDefault();
        }

        public async Task<List<TEntity>> GetAll()
        {
            var res = await _collection.FindAsync(entity => true);
            return await res.ToListAsync();
        }
    }
}
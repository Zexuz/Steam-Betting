using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Betting.Repository.Interfaces
{
    public interface IGenericCollection<TEntity> where TEntity : class
    {
        IMongoCollection<TEntity> Collection { get; }
        Task                InsertOneAsync(TEntity entity);
        Task                InsertManyAsync(IEnumerable<TEntity> entities);
        Task                UpdateOneAsync(FilterDefinition<TEntity> filterDefinition, UpdateDefinition<TEntity> updateDefinition);
        Task<List<TEntity>> FindAsync(FilterDefinition<TEntity> filterDefinition);
        Task<TEntity>       FindSingleOrDefaultAsync(FilterDefinition<TEntity> filterDefinition);
        Task<List<TEntity>> GetAll();
    }
}
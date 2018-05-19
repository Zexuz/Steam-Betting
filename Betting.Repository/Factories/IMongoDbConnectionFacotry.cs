using Betting.Repository.Interfaces;

namespace Betting.Repository.Factories
{
    public interface IMongoDbConnectionFacotry
    {
        IGenericCollection<TEntity> GetCollection<TEntity>() where TEntity : class;
    }
}
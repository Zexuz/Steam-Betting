using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Interfaces
{
    public interface ILogServiceFactory
    {
        ILogService<T> CreateLogger<T>();
    }
}
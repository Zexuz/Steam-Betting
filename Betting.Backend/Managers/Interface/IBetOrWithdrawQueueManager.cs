using Betting.Backend.Managers.Impl;

namespace Betting.Backend.Managers.Interface
{
    public interface IBetOrWithdrawQueueManager
    {
        void Add(string steamId, QueueAction action);
        bool DoesExist(string steamId);
        void Remover(string steamId);
    }
}
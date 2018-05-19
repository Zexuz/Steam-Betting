using System.Collections.Generic;
using Betting.Backend.Managers.Interface;

namespace Betting.Backend.Managers.Impl
{
    public class BetOrWithdrawQueueManager : IBetOrWithdrawQueueManager
    {
        private readonly object _lock;

        private readonly Dictionary<string, QueueAction> _dictionary;

        public BetOrWithdrawQueueManager()
        {
            _dictionary = new Dictionary<string, QueueAction>();
            _lock       = new object();
        }

        public void Add(string steamId, QueueAction action)
        {
            lock (_lock)
            {
                _dictionary.Add(steamId, action);
            }
        }

        public bool DoesExist(string steamId)
        {
            lock (_lock)
            {
                return _dictionary.ContainsKey(steamId);
            }
        }

        public void Remover(string steamId)
        {
            lock (_lock)
            {
                _dictionary.Remove(steamId);
            }
        }
    }


    public enum QueueAction
    {
        Bet,
        Withdraw
    }
}
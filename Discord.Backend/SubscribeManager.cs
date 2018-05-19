using System.Collections.Generic;
using System.Linq;
using Discord.Backend.Enum;

namespace Discord.Backend
{
    public class SubscribeManager
    {
        private readonly Dictionary<ulong, HashSet<int>> _subscribes;

        public SubscribeManager()
        {
            _subscribes = new Dictionary<ulong, HashSet<int>>();
        }

        public void Subscribe(ulong userId, Event @event)
        {
            Subscribe(userId,new List<int>{(int)@event});
        }
        public void UnSubscribe(ulong userId, Event @event)
        {
            UnSubscribe(userId,new List<int>{(int)@event});
        }
        
        public void Subscribe(ulong userId, IEnumerable<int> events)
        {
            if (!_subscribes.ContainsKey(userId))
                _subscribes[userId] = new HashSet<int>();

            foreach (var i in events)
                _subscribes[userId].Add(i);
        }
        
        public void UnSubscribe(ulong userId, IEnumerable<int> events)
        {
            if (!_subscribes.ContainsKey(userId)) return;

            foreach (var i in events)
                _subscribes[userId].Remove(i);
        }

        public List<ulong> GetUsersSubscribedToEvent(int eventNr)
        {
            return _subscribes.Where(pair => pair.Value.Contains(eventNr)).Select(pair => pair.Key).ToList();
        }
        public List<ulong> GetUsersSubscribedToEvent(Event @event)
        {
            return GetUsersSubscribedToEvent((int) @event);
        }
    }
}
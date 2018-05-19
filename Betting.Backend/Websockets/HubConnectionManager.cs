using System;
using System.Collections.Generic;
using System.Linq;

namespace Betting.Backend.Websockets
{
    public class HubConnectionManager : IHubConnectionManager
    {
        private Dictionary<Type, Dictionary<string,List<string>>> _dictionary;

        public HubConnectionManager()
        {
            _dictionary = new Dictionary<Type, Dictionary<string, List<string>>>();
        }

        public List<string> Get(Type type, string steamId)
        {
            //this returns a list of connectionIds
            return _dictionary[type].SingleOrDefault(id => id.Key == steamId).Value;
        }

        public void Add(Type type, ConnectionAndSteamId connectionAndSteamId)
        {
            if (!_dictionary.ContainsKey(type))
                _dictionary.Add(type, new Dictionary<string, List<string>>());
            
            if (!_dictionary[type].ContainsKey(connectionAndSteamId.SteamId))
                _dictionary[type].Add(connectionAndSteamId.SteamId,new List<string>());

            _dictionary[type][connectionAndSteamId.SteamId].Add(connectionAndSteamId.ConnectionId);
        }
        
        public void Remove(Type type, ConnectionAndSteamId connectionAndSteamId)
        {
            if (!_dictionary.ContainsKey(type)) return;
            if (!_dictionary[type].ContainsKey(connectionAndSteamId.SteamId)) return;
            
            _dictionary[type][connectionAndSteamId.SteamId].Remove(connectionAndSteamId.ConnectionId);
        }

        public class ConnectionAndSteamId
        {
            public string SteamId      { get; set; }
            public string ConnectionId { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Websockets;
using Microsoft.Extensions.Configuration;

namespace Betting.Backend.Managers.Impl
{
    public class HotStatusManager : IHotStatusManager
    {
        private readonly ICoinFlipHubConnections       _coinFlipHubConnections;
        private readonly TimeSpan                      _hotLenght;
        private          Dictionary<string, DateTime?> _hotMatches;

        public HotStatusManager(IConfiguration configuration, ICoinFlipHubConnections coinFlipHubConnections)
        {
            _coinFlipHubConnections = coinFlipHubConnections;

            var lenghtInSec = int.Parse(configuration.GetSection("HotStatus").GetSection("LenghtInSec").Value);

            _hotLenght = TimeSpan.FromSeconds(lenghtInSec);
            _hotMatches = new Dictionary<string, DateTime?>();
        }

        public async void AddHotMatch(string steamId, string roundId)
        {
            
            if (!_hotMatches.ContainsKey(roundId) || _hotMatches[roundId] == null)
                await _coinFlipHubConnections.MatchIsHot(roundId,steamId);

            _hotMatches[roundId] = DateTime.Now.Add(_hotLenght);
        }
        
        public async void RemoveExperiedHotStatuses()
        {
            foreach (var hotMatch in _hotMatches.ToList())
            {
                if (!IsExpiered(hotMatch.Key)) continue;
                
                await _coinFlipHubConnections.MatchIsNoLongerHot(hotMatch.Key);
                _hotMatches[hotMatch.Key] = null;
            }
        }
        
        private bool IsExpiered(string roundId)
        {
            return _hotMatches[roundId] < DateTime.Now;
        }
    }
}
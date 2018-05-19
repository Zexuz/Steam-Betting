using System;
using System.Collections.Generic;
using RpcCommunication;

namespace Betting.Backend.Cache
{
    public class SteamInventoryCacheManager : ISteamInventoryCacheManager
    {
        private Dictionary<string, SteamInventoryCache> _steamInventoryCaches;

        public TimeSpan CacheTimeSpan { get; set; }

        public SteamInventoryCacheManager()
        {
            _steamInventoryCaches = new Dictionary<string, SteamInventoryCache>();
        }

        public void AddToCache(string steamId, GetPlayerSteamInventoryResponse cahce)
        {
            RemoveOutdatedCahches();
            try
            {
                _steamInventoryCaches.Add(steamId, new SteamInventoryCache(CacheTimeSpan, cahce));
            }
            catch (System.Exception e)
            {
                // ignored
            }
        }

        public bool HasCache(string steamId)
        {
            RemoveOutdatedCahches();
            var hasCache = _steamInventoryCaches.ContainsKey(steamId);
            return hasCache;
        }

        public GetPlayerSteamInventoryResponse LookupCache(string steamId)
        {
            RemoveOutdatedCahches();
            try
            {
                var cache = _steamInventoryCaches[steamId].Inventory;
                return cache;
            }
            catch (System.Exception e)
            {
                return null;
            }
        }

        private void RemoveOutdatedCahches()
        {
            var idsToRemove = new List<string>();
            foreach (var cache in _steamInventoryCaches)
            {
                if (cache.Value.IsValid) continue;
                idsToRemove.Add(cache.Key);
            }

            foreach (var steamId in idsToRemove)
            {
                _steamInventoryCaches.Remove(steamId);
            }
        }
    }
}
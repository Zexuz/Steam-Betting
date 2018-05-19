using System;
using RpcCommunication;

namespace Betting.Backend.Cache
{
    public interface ISteamInventoryCacheManager
    {
        TimeSpan                        CacheTimeSpan { get; set; }
        void                            AddToCache(string steamId, GetPlayerSteamInventoryResponse cahce);
        bool                            HasCache(string steamId);
        GetPlayerSteamInventoryResponse LookupCache(string steamId);
    }
}
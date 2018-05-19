using System;
using RpcCommunication;

namespace Betting.Backend.Cache
{
    public class SteamInventoryCache
    {
        public bool                            IsValid   => ((Created + LifeSpan) - DateTime.Now).TotalSeconds > 0;
        public GetPlayerSteamInventoryResponse Inventory { get; }

        private DateTime Created  { get; }
        private TimeSpan LifeSpan { get; }

        public SteamInventoryCache(TimeSpan lifeSpan, GetPlayerSteamInventoryResponse inventory)
        {
            Created   = DateTime.Now;
            LifeSpan  = lifeSpan;
            Inventory = inventory;
        }
    }
}
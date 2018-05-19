using System;
using System.Collections.Generic;

namespace Betting.Backend.Websockets
{
    public interface IHubConnectionManager
    {
        List<string> Get(Type type, string steamId);
        void         Add(Type type, HubConnectionManager.ConnectionAndSteamId connectionAndSteamId);
        void         Remove(Type type, HubConnectionManager.ConnectionAndSteamId connectionAndSteamId);
    }
}
using System;

namespace Betting.Backend.Services.Interfaces
{
    public interface IDiscordService
    {
        IDiscordAuthService AuthService { get; }
        void                SendPersonalMessageAsync(string message, string toSteamId);
        void                AddUserAsync(string id, string steamId);
        void                ChatMessageAsync(string name, string message);
        void                CoinFlipCreateAsync(bool csgo, bool pubg, decimal value, string coinFlipId, string userId);
        void                CoinFlipJoinAsync(decimal value, string coinFlipId, string userId);
        void                CoinFlipWinnerAsync(string coinFlipId, decimal totalValue);
        void                GlobalExceptionErrorAsync(string corelcationId, string location, Exception e, string userId);
        void                JackpotBetAsync(int roundId, string userId, decimal value);
        void                JackpotWinnerAsync(int roundId, decimal value);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Betting.Repository.Exceptions;

namespace Betting.Repository.Helpers
{
    public static class GameModeHelper
    {
        private static Dictionary<GameModeType, string> _gameModeTypeToString = new Dictionary<GameModeType, string>
        {
            {GameModeType.JackpotCsgo, "Jackpot"},
            {GameModeType.CoinFlip, "CoinFlip"},
        };

        public static GameModeType GetTypeFromString(string type)
        {
            try
            {
                return _gameModeTypeToString.First(kvp => kvp.Value == type).Key;
            }
            catch (Exception)
            {
                throw new InvalidGameModeTypeException($"The type {type} is unknown");
            }
        }

        public static string GetStringFromType(GameModeType type)
        {
            try
            {
                return _gameModeTypeToString.First(kvp => kvp.Key == type).Value;
            }
            catch (Exception)
            {
                throw new InvalidGameModeTypeException($"The type {type} is unknown");
            }
        }
    }
}
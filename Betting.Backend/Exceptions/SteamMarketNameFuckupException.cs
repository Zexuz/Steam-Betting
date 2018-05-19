using System;

namespace Betting.Backend.Exceptions
{
    internal class SteamMarketNameFuckupException:Exception
    {
        public SteamMarketNameFuckupException(string str,Exception exception) : base(str,exception)
        {
        }
    }
}
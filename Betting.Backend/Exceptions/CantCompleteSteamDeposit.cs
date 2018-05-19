using System;

namespace Betting.Backend.Exceptions
{
    public class CantCompleteSteamDeposit : Exception
    {
        public CantCompleteSteamDeposit(string s, Exception e) : base(s, e)
        {
        }
    }
}
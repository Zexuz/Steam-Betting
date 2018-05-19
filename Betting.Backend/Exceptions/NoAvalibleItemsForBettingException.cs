using System;

namespace Betting.Backend.Exceptions
{
    public class NoAvalibleItemsForBettingException : Exception
    {
        public NoAvalibleItemsForBettingException(string s) : base(s)
        {
        }
    }
}
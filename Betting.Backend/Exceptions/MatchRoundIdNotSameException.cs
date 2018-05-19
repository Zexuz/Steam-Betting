using System;

namespace Betting.Backend.Exceptions
{
    public class MatchRoundIdNotSameException : Exception
    {
        public MatchRoundIdNotSameException(string s) : base(s)
        {
        }
    }
}
using System;

namespace Betting.Backend.Exceptions
{
    public class MatchDoesNotAllowBetsNow : Exception
    {
        public MatchDoesNotAllowBetsNow(string s) : base(s)
        {
        }
    }
}
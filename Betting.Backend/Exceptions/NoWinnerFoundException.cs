using System;

namespace Betting.Backend.Exceptions
{
    public class NoWinnerFoundException : Exception
    {
        public NoWinnerFoundException(string s) : base(s)
        {
        }
    }
}
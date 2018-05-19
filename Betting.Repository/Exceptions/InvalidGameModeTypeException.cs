using System;

namespace Betting.Repository.Exceptions
{
    public class InvalidGameModeTypeException : Exception
    {
        public InvalidGameModeTypeException(string s) : base(s)
        {
        }
    }
}
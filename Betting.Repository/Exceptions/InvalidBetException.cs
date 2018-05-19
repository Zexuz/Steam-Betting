using System;

namespace Betting.Repository.Exceptions
{
    public class InvalidBetException : Exception
    {
        public InvalidBetException(string s) : base(s)
        {
        }

        public InvalidBetException(string s, Exception sqlException) : base(s, sqlException)
        {
        }
    }
}
using System;

namespace Betting.Backend.Exceptions
{
    public class NoBotInDatabaseException : Exception
    {
        public NoBotInDatabaseException(string msg) : base(msg)
        {
        }
    }
}
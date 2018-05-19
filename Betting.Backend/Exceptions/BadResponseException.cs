using System;

namespace Betting.Backend.Exceptions
{
    public class BadResponseException : Exception
    {
        public BadResponseException(string s) : base(s)
        {
        }
    }
}
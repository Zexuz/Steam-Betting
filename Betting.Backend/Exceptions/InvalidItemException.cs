using System;

namespace Betting.Backend.Exceptions
{
    public class InvalidItemException : Exception
    {
        public InvalidItemException(string s) : base(s)
        {
        }
    }
}
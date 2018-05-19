using System;

namespace Betting.Backend.Exceptions
{
    public class InvalidValueException : Exception
    {
        public InvalidValueException(string str) : base(str)
        {
        }
    }
}
using System;

namespace Betting.Backend.Exceptions
{
    public class InvalidItemTypeException : Exception
    {
        public InvalidItemTypeException(string s) : base(s)
        {
        }
    }
}
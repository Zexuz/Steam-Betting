using System;

namespace Betting.Backend.Exceptions
{
    public class ToFewItemsOnBetException : Exception
    {
        public ToFewItemsOnBetException(string s) : base(s)
        {
        }
    }
}
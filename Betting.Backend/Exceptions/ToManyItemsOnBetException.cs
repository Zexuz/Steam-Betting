using System;

namespace Betting.Backend.Exceptions
{
    public class ToManyItemsOnBetException : Exception
    {
        public ToManyItemsOnBetException(string s) : base(s)
        {
        }
    }
}
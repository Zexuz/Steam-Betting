using System;

namespace Betting.Backend.Exceptions
{
    public class ToMuchValueOnBetException : Exception
    {
        public ToMuchValueOnBetException(string s) : base(s)
        {
        }
    }
}
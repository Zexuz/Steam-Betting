using System;

namespace Betting.Backend.Exceptions
{
    public class ToLittleValueOnBetException : Exception
    {
        public ToLittleValueOnBetException(string s) : base(s)
        {
        }
    }
}
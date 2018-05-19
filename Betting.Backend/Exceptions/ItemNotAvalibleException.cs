using System;

namespace Betting.Backend.Exceptions
{
    public class ItemNotAvalibleException : Exception
    {
        public ItemNotAvalibleException(string s) : base(s)
        {
        }
    }
}
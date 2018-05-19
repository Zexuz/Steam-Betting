using System;

namespace Betting.Backend.Exceptions
{
    public class TradeLinkNotSetExeption : Exception
    {
        public TradeLinkNotSetExeption(string msg) : base(msg)
        {
        }
    }
}
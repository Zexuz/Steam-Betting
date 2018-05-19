using System;

namespace Betting.Backend.Exceptions
{
    public class RegexMatchNotFoundException : Exception
    {
        public RegexMatchNotFoundException(string s) : base(s)
        {
        }
    }
}
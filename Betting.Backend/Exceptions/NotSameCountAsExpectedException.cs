using System;

namespace Betting.Backend.Exceptions
{
    public class NotSameCountAsExpectedException : Exception
    {
        public NotSameCountAsExpectedException(string s) : base(s)
        {
        }
    }
}
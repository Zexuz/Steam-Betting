using System;

namespace Betting.Repository.Exceptions
{
    public class ToOldPreHashException : Exception
    {
        public ToOldPreHashException(string s):base(s)
        {
        }
    }
}
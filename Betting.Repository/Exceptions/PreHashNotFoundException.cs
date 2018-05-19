using System;

namespace Betting.Repository.Exceptions
{
    public class PreHashNotFoundException : Exception
    {
        public PreHashNotFoundException(string s):base(s)
        {
        }
    }
}
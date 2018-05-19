using System;

namespace Betting.Backend.Exceptions
{
    public class NoItemsAvalibleExecption : Exception
    {
        public NoItemsAvalibleExecption(string str) : base(str)
        {
        }
    }
}
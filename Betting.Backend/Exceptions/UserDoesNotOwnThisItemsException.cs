using System;

namespace Betting.Backend.Exceptions
{
    public class UserDoesNotOwnThisItemsException : Exception
    {
        public UserDoesNotOwnThisItemsException(string s) : base(s)
        {
        }
    }
}
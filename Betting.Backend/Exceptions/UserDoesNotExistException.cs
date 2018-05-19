using System;

namespace Betting.Backend.Exceptions
{
    public class UserDoesNotExistException : Exception
    {
        public UserDoesNotExistException(string s) : base(s)
        {
        }
    }
}
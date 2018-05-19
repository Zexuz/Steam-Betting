using System;

namespace Betting.Backend.Exceptions
{
    public class NotAllowedAppIdOnMatchException : Exception
    {
        public NotAllowedAppIdOnMatchException(string s):base(s)
        {
            
        }
    }
}
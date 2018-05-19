using System;

namespace Betting.Repository.Exceptions
{
    public class UserSuspendedFromUpdatingQuoteException : Exception
    {
        public UserSuspendedFromUpdatingQuoteException(string s):base(s)
        {
        }
    }
}
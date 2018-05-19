using System;

namespace Betting.Backend.Exceptions
{
    public class InvalidAssetAndDecriptionIdException : Exception
    {
        public InvalidAssetAndDecriptionIdException(string s) : base(s)
        {
        }
    }
}
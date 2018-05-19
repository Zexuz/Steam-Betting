using System;

namespace Betting.WebApi.Exceptions
{
    public class InvalidJsonFromSteamException : Exception
    {
        public InvalidJsonFromSteamException(string str, Exception exception) : base(str, exception)
        {
        }
    }
}
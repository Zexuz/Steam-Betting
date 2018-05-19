using System;

namespace Betting.Backend.Exceptions
{
    public class GameModeIsNotEnabledException : Exception
    {
        public GameModeIsNotEnabledException(string s):base(s)
        {
        }
    }
}
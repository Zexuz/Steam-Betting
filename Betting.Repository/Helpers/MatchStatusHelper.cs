using System;
using Betting.Models;

namespace Betting.Repository.Helpers
{
    public static class MatchStatusHelper
    {
        public static int GetIntFromMatchStatus(MatchStatus status)
        {
            switch (status)
            {
                case MatchStatus.Closed:
                    return 0;
                case MatchStatus.Open:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static MatchStatus GetMatchStatusFromInt(int i)
        {
            switch (i)
            {
                case 0:
                    return MatchStatus.Closed;
                case 1:
                    return MatchStatus.Open;
                default:
                    throw new ArgumentOutOfRangeException(nameof(i), i, null);
            }
        }
    }
}
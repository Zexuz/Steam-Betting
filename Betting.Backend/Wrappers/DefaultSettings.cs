using System;
using Grpc.Core;

namespace Betting.Backend.Wrappers
{
    public static class DefaultSettings
    {
        
        public static CallOptions GetDefaultSettings(TimeSpan deadline)
        {
            return new CallOptions(deadline: DateTime.UtcNow.Add(deadline));
        }
        
        public static CallOptions GetDefaultSettings(int sec = 10)
        {
            return new CallOptions(deadline: DateTime.UtcNow.AddSeconds(sec));
        }
    }
}
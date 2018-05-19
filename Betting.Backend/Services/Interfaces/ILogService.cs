using System;
using System.Collections.Generic;

namespace Betting.Backend.Services.Interfaces
{
    public interface ILogService<T>
    {
        void Info(string text);
        void Debug(string text);
        void Critical(Exception e);
        void Error(string userId, string location, Exception exception, Dictionary<string, object> info);
        void Error(string userId, string location, Exception exception);
    }
}
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Betting.Backend.Implementations
{
    public class LogServiceFactory : ILogServiceFactory
    {
        private readonly IGmailService            _gmailService;
        private readonly bool                     _isDebug;

        public LogServiceFactory(IHostingEnvironment env,IGmailService gmailService)
        {
            _gmailService  = gmailService;
            _isDebug       = env.EnvironmentName.Contains("Dev");
        }

        public ILogService<T> CreateLogger<T>()
        {
            if (_isDebug)
                return new DebugLogService<T>(_gmailService);
            return new LogService<T>(_gmailService);
        }
    }
}
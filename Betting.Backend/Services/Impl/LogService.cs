using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using Betting.Backend.Services.Interfaces;
using Serilog.Core.Trackers;
using Exception = System.Exception;

namespace Betting.Backend.Services.Impl
{
    public class DebugLogService<T> : LogServiceBase<T>
    {
        public DebugLogService(IGmailService gmailService) : base(gmailService)
        {
        }

        protected override bool IsDevMode => true;
    }

    public class LogService<T> : LogServiceBase<T>
    {
        public LogService(IGmailService gmailService) : base(gmailService)
        {
        }

        protected override bool IsDevMode => false;
    }

    public abstract class LogServiceBase<T> : ILogService<T>
    {
        protected abstract bool          IsDevMode { get; }
        private readonly   IGmailService _gmailService;

        protected LogServiceBase(IGmailService gmailService)
        {
            _gmailService = gmailService;
        }

        public void Debug(string text)
        {
            if (!IsDevMode) return;

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("DEBUG: ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"{typeof(T).FullName}: {text}");
            Console.ResetColor();
        }

        public void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("INFO: ");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine($"{typeof(T).FullName}: {text}");
            Console.ResetColor();
        }

        public void Critical(Exception e)
        {
            var strBuilder = new StringBuilder();
            strBuilder.Append(e.Message);
            strBuilder.Append(" | ");
            strBuilder.Append(e.TargetSite == null ? null : e.TargetSite.Name);
            strBuilder.AppendLine();
            strBuilder.AppendLine(e.StackTrace);

            _gmailService.SendEmail(new List<MailAddress>
            {
                new MailAddress("Robin.Edbom@gmail.com")
            }, strBuilder.ToString(), "Critical exception!").Wait();
            Error(null,null,e);
        }

        public void Error(string userId,string location, Exception exception,Dictionary<string,object> info)
        {
            var errorTracker = new ErrorTracker(userId, location, "DomainName", "Backend", exception, Guid.NewGuid().ToString(), info);
            errorTracker.Stop();
        }

        public void Error(string userId, string location, Exception exception)
        {
            Error(userId,location,exception,null);
        }
    }
}
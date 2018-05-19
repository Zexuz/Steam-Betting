using System;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Serilog.Core
{
    public static class Logger
    {
        private static readonly ILogger PerfLogger;
        private static readonly ILogger ErrorLogger;
        private static readonly ILogger MatchLogger;

        static Logger()
        {
            var options = new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
            };
            PerfLogger = new LoggerConfiguration().WriteTo.Elasticsearch(options).CreateLogger();
            ErrorLogger = new LoggerConfiguration().WriteTo.Elasticsearch(options).CreateLogger();
            MatchLogger = new LoggerConfiguration().WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "matchstatsh-{0:yyyy.MM.dd}"
            }).CreateLogger();
        }

        public static void WritePerf(LogDetail log)
        {
            PerfLogger.Write(LogEventLevel.Information, "{@LogDetail}", log);
        }

        public static void WriteError(LogDetail log)
        {
            ErrorLogger.Write(LogEventLevel.Error, "{@LogDetail}", log);
        }
        
        public static void WriteJackpotMatch(object log)
        {
            MatchLogger.Write(LogEventLevel.Error, "{@MatchLogDetail}", log);
        }
        
        public static void WriteCoinMatch(object log)
        {
            MatchLogger.Write(LogEventLevel.Error, "{@MatchLogDetail}", log);
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using RpcCommunication;

namespace CsgoDraffle.Logging
{
    public class LogginServiceServer : LogginService.LogginServiceBase
    {
        private readonly MongoLoggingService _loggingService;

        public LogginServiceServer(MongoLoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public override async Task<AddToErrorLogResponse> AddErrorLogs(AddToErrorLogRequest request, ServerCallContext context)
        {
            await _loggingService.AddErrorLog(request.LogError);
            return new AddToErrorLogResponse();
        }

        public override async Task<GetErrorLogResponse> GetErrorLogs(GetErrorLogRequest request, ServerCallContext context)
        {
            var start = long.Parse(request.StartDate) == 0
                ? (DateTime?) null
                : DateTimeHelper.ConvertStringToDatetime(request.StartDate);
            var end = long.Parse(request.EndDate) == 0
                ? (DateTime?) null
                : DateTimeHelper.ConvertStringToDatetime(request.EndDate);

            var steamId = string.IsNullOrEmpty(request.SteamId) ? null : request.SteamId;

            var res = await _loggingService.GetErrorLogs(start, end, steamId);
            return new GetErrorLogResponse
            {
                Errors = {res}
            };
        }

        public override async Task<LoggingUser> AddLogToUse(UserLog request, ServerCallContext context)
        {
            var logs = request.Logs.Select(log => new Log
            {
                Action = log.Action,
                Time   = DateTimeHelper.ConvertStringToDatetime(log.Time),
                Value  = log.Value,
            }).ToList();

            await _loggingService.AddOrUpdateUserLog(new UserLogIndex
            {
                Logs    = logs,
                SteamId = request.SteamId
            });

            return new LoggingUser
            {
                SteamId = request.SteamId
            };
        }

        public override async Task<UserLog> GetLogsForUser(LoggingUser request, ServerCallContext context)
        {
            var res = await _loggingService.FindUserLog(request.SteamId);

            return new UserLog
            {
                SteamId = res.SteamId,
                Logs    =
                {
                    res.Logs.Select(log => new RpcCommunication.Log
                    {
                        Action = log.Action,
                        Time   = log.Time.ToBinary().ToString(),
                        Value  = log.Value,
                    })
                }
            };
        }
    }

    public static class DateTimeHelper
    {
        public static DateTime ConvertStringToDatetime(string str)
        {
            return DateTime.FromBinary(Convert.ToInt64(str));
        }
    }
}
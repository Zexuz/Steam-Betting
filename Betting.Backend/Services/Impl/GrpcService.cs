using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Betting.Backend.Cache;
using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public interface IGrpcService
    {
        Task<Dictionary<string, object>> CheckStatusOnGrpcServices();
        Task                             OnServerStart();
    }

    public class GrpcService : IGrpcService
    {
        private readonly IGrpcServiceFactory      _grpcServiceFactory;
        private          ILogService<GrpcService> _logService;

        public GrpcService(IGrpcServiceFactory grpcServiceFactory, ILogServiceFactory logServiceFactory)
        {
            _grpcServiceFactory = grpcServiceFactory;
            _logService = logServiceFactory.CreateLogger<GrpcService>();
        }

        public async Task<Dictionary<string, object>> CheckStatusOnGrpcServices()
        {
            var dict = new Dictionary<string, object>();

            var discordTimeTask = Ping(() => _grpcServiceFactory.GetDiscordSercviceClient().PingAsync());
            var historyTimeTask = Ping(() => _grpcServiceFactory.GetHistorySercviceClient().PingAsync());
            var chatTimeTask = Ping(() => _grpcServiceFactory.GetChatSercviceClient().PingAsync());
            var steamTimeTask = Ping(() => _grpcServiceFactory.GetSteamServiceClient(new SteamInventoryCacheManager()).PingAsync());
            var ticketTimeTask = Ping(() => _grpcServiceFactory.GetTicketSercviceClient().PingAsync());

            await Task.WhenAll(chatTimeTask, discordTimeTask, historyTimeTask, steamTimeTask, ticketTimeTask);

            dict.Add("Discord", discordTimeTask.Result);
            dict.Add("History", historyTimeTask.Result);
            dict.Add("Chat", chatTimeTask.Result);
            dict.Add("steam", steamTimeTask.Result);
            dict.Add("ticket", ticketTimeTask.Result);

            return dict;
        }

        public async Task OnServerStart()
        {
            var res = await CheckStatusOnGrpcServices();

            foreach (var kvp in res)
            {
                if (kvp.Value == null)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        _logService.Info($"{kvp.Key} is not responding!");
                        await Task.Delay(100);
                    }

                    for (int i = 0; i < 3; i++)
                        _logService.Info("===========");
                    continue;
                }
                
                _logService.Info($"{kvp.Key} is online, {kvp.Value} ms!");
            }
        }

        private static async Task<long?> Ping(Func<Task> ping)
        {
            long time;
            try
            {
                var stopwatch = Stopwatch.StartNew();
                await ping();
                stopwatch.Stop();
                time = stopwatch.ElapsedMilliseconds;
            }
            catch (Exception e)
            {
                return null;
            }

            return time;
        }
    }
}
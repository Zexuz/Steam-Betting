using Betting.Backend.Cache;
using Betting.Backend.Interfaces;
using Betting.Backend.Wrappers;
using Betting.Backend.Wrappers.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Configuration;

using RpcCommunication;
using RpcCommunicationChat;
using RpcCommunicationDiscord;
using RpcCommunicationHistory;
using RpcCommunicationTicket;

namespace Betting.Backend.Factories
{
    public class GrpcServiceFactory : IGrpcServiceFactory
    {
        private readonly IGrpcConnectionFactory _connectionFactory;
        private readonly IConfiguration         _configuration;
        private readonly ILogServiceFactory     _logServiceFactory;

        public GrpcServiceFactory(IGrpcConnectionFactory connectionFactory, IConfiguration configuration, ILogServiceFactory logServiceFactory)
        {
            _connectionFactory = connectionFactory;
            _configuration = configuration;
            _logServiceFactory = logServiceFactory;
        }

        public ISteamServiceClientWrapper GetSteamServiceClient(ISteamInventoryCacheManager cacheManager)
        {
            var channel = GetChannel("SteamBots");
            var steamServiceClient = new SteamService.SteamServiceClient(channel);
            return new SteamServiceClientWrapper(steamServiceClient, cacheManager,_logServiceFactory);
        }

        public IChatServiceClientWrapper GetChatSercviceClient()
        {
            var channel = GetChannel("Chat");
            var chatServiceClient = new ChatService.ChatServiceClient(channel);

            return new ChatServiceClientWrapper(chatServiceClient,_logServiceFactory);
        }

        public ITicketServiceClientWrapper GetTicketSercviceClient()
        {
            var channel = GetChannel("Ticket");
            var chatServiceClient = new TicketService.TicketServiceClient(channel);

            return new TicketServiceClientWrapper(chatServiceClient,_logServiceFactory);
        }

        public IDiscordServiceClientWrapper GetDiscordSercviceClient()
        {
            var channel = GetChannel("Discord");
            var chatServiceClient = new DiscordService.DiscordServiceClient(channel);

            return new DiscordServiceClientWrapper(chatServiceClient, _logServiceFactory);
        }
        
        public IHistoryServiceClientWrapper GetHistorySercviceClient()
        {
            var channel = GetChannel("History");
            var historyServiceClient = new Bettingv1HisotryService.Bettingv1HisotryServiceClient(channel);

            return new HistoryServiceClientWrapper(historyServiceClient, _logServiceFactory);
        }

        private Channel GetChannel(string section)
        {
            var connectionConfig = _configuration.GetSection("Grpc").GetSection(section).GetSection("Server");
            var host = connectionConfig.GetSection("Host").Value;
            var port = int.Parse(connectionConfig.GetSection("Port").Value);

            var channel = _connectionFactory.CreateConnection(host, port);
            return channel;
        }
    }
}
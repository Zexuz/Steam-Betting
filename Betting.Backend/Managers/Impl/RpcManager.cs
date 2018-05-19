using System.Threading.Tasks;
using Betting.Backend.Implementations;
using Betting.Backend.Managers.Interface;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using RpcCommunication;

namespace Betting.Backend.Managers.Impl
{
    public class RpcManager : IRpcManager
    {
        private readonly Server _server;

        public RpcManager(RpcSteamListener steamListener, IConfiguration configuration)
        {
            var grpcServer = configuration.GetSection("Grpc").GetSection("SteamBots").GetSection("Client");

            var serverHost = grpcServer.GetSection("Host").Value;
            var serverPort = int.Parse(grpcServer.GetSection("Port").Value);

            _server = new Server
            {
                Services = {StatusChanged.BindService(steamListener)},
                Ports    = {new ServerPort(serverHost, serverPort, ServerCredentials.Insecure)}
            };
        }

        public void Start()
        {
            _server.Start();
        }

        public async Task ShutdownAsync()
        {
            await _server.ShutdownAsync();
        }
    }
}
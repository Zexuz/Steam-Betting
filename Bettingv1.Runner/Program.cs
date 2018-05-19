using System;
using Grpc.Core;
using RpcCommunicationHistory;


namespace Bettingv1.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Start();
        }

        private void Start()
        {
            var mongoDbService = new MongoDbService();
            var historyServiceServer = new BettingHistoryRpcServer(mongoDbService);

            var server = new Server
            {
                Services = {Bettingv1HisotryService.BindService(historyServiceServer)},
                Ports = {new ServerPort("localhost", 50058, ServerCredentials.Insecure)}
            };

            server.Start();

            while (true)
            {
                var input = Console.ReadLine();
                if(input?.ToLower() == "exit")
                    break;
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using CsgoDraffle.Logging.Extensions;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RpcCommunication;


namespace CsgoDraffle.Logging.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            Console.WriteLine("Usage --port xxxx | --ip x.x.x.x | --mongoport");

            var portString      = config.GetSection("port").Value;
            var ipString        = config.GetSection("ip").Value;
            var mongoPortString = config.GetSection("mongoport").Value;

            var port      = string.IsNullOrEmpty(portString) ? 50055 : int.Parse(portString);
            var ip        = string.IsNullOrEmpty(ipString) ? IPAddress.Any : IPAddress.Parse(ipString);
            var mongoPort = string.IsNullOrEmpty(mongoPortString) ? 27017 : int.Parse(mongoPortString);

            Console.WriteLine($"Running rpc on {ip}:{port}");

            var mongoClient = new MongoLoggingService("DraffleBackend", $"mongodb://localhost:{mongoPort}");

            Thread  mongoServerThread = null;
            Process mongoProcess      = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (mongoClient.IsOnline())
                {
                    Console.WriteLine($"Mongo is already running on port {mongoPort}, will use that instance!");
                }
                else
                {
                    "mkdir -p ~/mongodb/data/db".BashSimple();
                    mongoServerThread =
                        new Thread(() => $"mongod --dbpath ~/mongodb/data/db/ --port {mongoPort}".GetProcessFromBash(out mongoProcess));
                    mongoServerThread.Start();
                    Console.WriteLine($"Mongod server started on port {mongoPort}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("ERROR: You are not running on a Linux OS");
                if (!mongoClient.IsOnline())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Mongo server is not runnin!");
                    Console.WriteLine("Shutting down!");
                    return;
                }

                Console.WriteLine($"Mongo server running on port {mongoPort}");
            }

            var logginServiceServer = new LogginServiceServer(new MongoLoggingService("Draffle", $"mongodb://localhost:{mongoPort}"));

            var server = new Server
            {
                Services = {LogginService.BindService(logginServiceServer)},
                Ports    = {new ServerPort(ip.ToString(), port, ServerCredentials.Insecure)}
            };

            server.Start();

            do
            {
                Console.WriteLine("Type 'Exit' to exit");
            } while (Console.ReadLine().ToLower() != "exit");

            server.ShutdownAsync().Wait();
            Console.WriteLine("Rpc server shutdown!");

            if (mongoServerThread != null)
            {
                mongoProcess.Kill();
                Console.WriteLine("Mongo server was killed!");
                mongoServerThread.Join();
            }
        }

        private void Print(UserLogIndex user)
        {
            var str = JsonConvert.SerializeObject(user, Formatting.Indented);
            Console.WriteLine(str);
        }
    }
}
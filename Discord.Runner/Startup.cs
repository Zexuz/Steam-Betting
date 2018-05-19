using System;
using System.Threading.Tasks;
using Discord.Backend;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Discord.Runner
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();
            var enviorment = config["environment"];
            if (string.IsNullOrEmpty(enviorment))
                throw new ArgumentException("--environment dev|prod needs to be provided");

            var builder = new ConfigurationBuilder()       // Create a new instance of the config builder
                .SetBasePath(AppContext.BaseDirectory)     // Specify the default location for the config file
                .AddJsonFile($"{enviorment}.config.json"); // Add this (json encoded) file to the configuration
            Configuration = builder.Build();               // Build the configuration
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection(); // Create a new instance of a service collection
            ConfigureServices(services);

            var provider = services.BuildServiceProvider(); // Build the service provider
            provider.GetRequiredService<CommandHandler>();  // Start the command handler service

            provider.GetRequiredService<GrpcServerService>().StartGrpcServer();
            await provider.GetRequiredService<StartupService>().StartAsync(); // Start the startup service
            await Task.Delay(-1);                                             // Keep the program alive
        }
        
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    // Add discord to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    MessageCacheSize = 100          // Cache 1,000 messages per channel
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    // Add the command service to the collection
                    LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                    DefaultRunMode = RunMode.Async, // Force all commands to run async by default
                    CaseSensitiveCommands = false   // Ignore case when executing commands
                }))
                .AddSingleton<GrpcServerService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()   
                .AddSingleton<Random>()           
                .AddSingleton<SubscribeManager>() 
                .AddSingleton(Configuration);     
        }

    }
}
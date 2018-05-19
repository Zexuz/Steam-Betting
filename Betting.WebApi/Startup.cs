using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Betting.Backend.Cache;
using Betting.Backend.Factories;
using Betting.Backend.Implementations;
using Betting.Backend.Interfaces;
using Betting.Backend.Managers.Impl;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Services.Interfaces.IoC;
using Betting.Backend.Services.IoC;
using Betting.Backend.Websockets;
using Betting.Models.Models;
using Betting.Repository;
using Betting.Repository.Factories;
using Betting.Repository.Helpers;
using Betting.Repository.Impl;
using Betting.Repository.Interfaces;
using Betting.Repository.Services;
using Betting.Repository.Services.Impl;
using Betting.Repository.Services.Interfaces;
using Betting.WebApi.Exceptions;
using Betting.WebApi.Extensions;
using Betting.WebApi.Helpers;
using Betting.WebApi.Middleware;
using Betting.WebApi.Websocket.Factories;
using Betting.WebApi.Websocket.Hubs;
using FluentScheduler;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
 
using RpcCommunication;
using Shared.Shared.Web;
using Swashbuckle.AspNetCore.Swagger;
using ChatService = Betting.Backend.Services.Impl.ChatService;
using DiscordService = Betting.Backend.Services.Impl.DiscordService;
using Exception = System.Exception;
using SteamService = Betting.Backend.Services.Impl.SteamService;
using TicketService = Betting.Backend.Services.Impl.TicketService;

namespace Betting.WebApi
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        private readonly ILoggerFactory      _loggerFactory;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _env = env;
            _loggerFactory = loggerFactory;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        private IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => { options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme; })
                .AddCookie(SetUpCookieOptions)
                .AddSteam(o =>
                {
                    o.ApplicationKey = Configuration.GetSection("Steam").GetSection("ApiKey").Value;
                    o.Events.OnAuthenticated = context =>
                    {
                        context.HttpContext.Items["User"] = context.User;
                        return Task.CompletedTask;
                    };
                });

            // Add framework services.
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.CustomSchemaIds(x => x.FullName);
                c.SwaggerDoc("v1", new Info {Title = "My API", Version = "v1"});

                var basePath = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "WebApi.xml");
                c.IncludeXmlComments(xmlPath);
            });

            services.AddSignalR();

            //todo only allow GET request if we want to make a "softShutdown"

            var container = BuildIoC(services);
            IoC.Container = container;

            FirstTimeSetUp(container).Wait();
            StartManagersAndCheckStatus(container).Wait();


            return new AutofacServiceProvider(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();

            if (env.EnvironmentName.Contains("Development-With-Prod-db"))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("YOU ARE USING THE PRODUCTION DATABASE!!!! WATCH OUT!!!");
                Console.WriteLine("YOU ARE USING THE PRODUCTION DATABASE!!!! WATCH OUT!!!");
                Console.WriteLine("YOU ARE USING THE PRODUCTION DATABASE!!!! WATCH OUT!!!");
                Console.WriteLine("YOU ARE USING THE PRODUCTION DATABASE!!!! WATCH OUT!!!");
                Console.WriteLine("YOU ARE USING THE PRODUCTION DATABASE!!!! WATCH OUT!!!");
                Console.WriteLine("YOU ARE USING THE PRODUCTION DATABASE!!!! WATCH OUT!!!");
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("-----------------------------------------------------------------------------");
                Console.WriteLine("-----------------------------------------------------------------------------");
            }


            if (env.EnvironmentName.Contains("Dev"))
            {
                loggerFactory.CreateLogger<Startup>();
                loggerFactory.AddDebug();

                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

                app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials().Build());
            }
            else
            {
                app.UseMiddleware<IgnoreRouteMiddleware>();
                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins(new[]
                {
                   "DomainName"
                }).AllowCredentials().Build());
            }

            app.UseExceptionHandler("/api/debug/error");

            app.UseMvc();

//            app.UseMvc(routes =>
//            {
//                routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
//            });

            app.UseSignalR(routes => { routes.MapHub<SteamHub>($"ws/{typeof(SteamHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<TestHub>($"ws/{typeof(TestHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<MatchHub>($"ws/{typeof(MatchHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<BetHub>($"ws/{typeof(BetHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<ChatHub>($"ws/{typeof(ChatHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<InfoHub>($"ws/{typeof(InfoHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<TicketHub>($"ws/{typeof(TicketHub).Name}"); });
            app.UseSignalR(routes => { routes.MapHub<CoinFlipHub>($"ws/{typeof(CoinFlipHub).Name}"); });
        }

        public static IContainer BuildIoC(IServiceCollection services)
        {
            var builder = new ContainerBuilder();
            IContainer container = null;
            IContainer Factory() => container;

            if (services != null)
                builder.Populate(services);
            builder.RegisterInstance((Func<IContainer>) Factory);

            builder.RegisterType<SteamService>().As<ISteamService>().SingleInstance();
            builder.RegisterType<RpcManager>().As<IRpcManager>().SingleInstance();
            builder.RegisterType<JobScheduleManager>().As<IJobScheduleManager>().SingleInstance();
            builder.RegisterType<RpcSteamListener>().AsSelf().SingleInstance();

            builder.RegisterType<LogServiceFactory>().As<ILogServiceFactory>().SingleInstance();

            builder.RegisterType<BotManager>().As<IBotManager>().SingleInstance();
            builder.RegisterType<OfferManager>().As<IOfferManager>().SingleInstance();
            builder.RegisterType<SteamInventoryCacheManager>().As<ISteamInventoryCacheManager>().SingleInstance();
            builder.RegisterType<JackpotMatchManager>().As<IJackpotMatchManager>().SingleInstance();
            builder.RegisterType<CoinFlipManager>().As<ICoinFlipManager>().SingleInstance();
            builder.RegisterType<BetOrWithdrawQueueManager>().As<IBetOrWithdrawQueueManager>().SingleInstance();

            builder.RegisterType<HttpRequestService>().As<IHttpRequestService>().SingleInstance();
            builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();

            builder.RegisterType<GrpcConnectionFactory>().As<IGrpcConnectionFactory>().SingleInstance();
            builder.RegisterType<MongoDbConnectionFacotry>().As<IMongoDbConnectionFacotry>().SingleInstance();
            builder.RegisterType<HotStatusManager>().As<IHotStatusManager>().SingleInstance();
            builder.RegisterType<HubConnectionManager>().As<IHubConnectionManager>().SingleInstance();

            builder.RegisterType<GrpcServiceFactory>().As<IGrpcServiceFactory>();

            builder.RegisterType<JackpotDraftService>().As<IJackpotDraftService>();

            builder.RegisterType<BotService>().As<IBotService>();
            builder.RegisterType<RegexService>().As<IRegexService>();
            builder.RegisterType<HashService>().As<IHashService>();
            builder.RegisterType<ValueConverter>().As<IValueConverter>();
            builder.RegisterType<QueryFactory>().As<IQueryFactory>();
            builder.RegisterType<MatchQueries>().As<IMatchQueries>();
            builder.RegisterType<BetQueries>().As<IBetQueries>();
            builder.RegisterType<BetService>().As<IBetService>();
            builder.RegisterType<RakeItemQueries>().As<IRakeItemQueries>();
            builder.RegisterType<ItemQueries>().As<IItemQueries>();
            builder.RegisterType<ItemBetQueries>().As<IItemBetQueries>();
            builder.RegisterType<ItemDescriptionQueries>().As<IItemDescriptionQueries>();
            builder.RegisterType<UserQueries>().As<IUserQueries>();
            builder.RegisterType<ItemService>().As<IItemService>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<StaffService>().As<IStaffService>();
            builder.RegisterType<LevelService>().As<ILevelService>();
            builder.RegisterType<ChatService>().As<IChatService>();
            builder.RegisterType<TicketService>().As<ITicketService>();
            builder.RegisterType<DiscordAuthAuthService>().As<IDiscordAuthService>();
            builder.RegisterType<DiscordService>().As<IDiscordService>();
            builder.RegisterType<GrpcService>().As<IGrpcService>();
            builder.RegisterType<CoinFlipService>().As<ICoinFlipService>();
            builder.RegisterType<RakeService>().As<IRakeService>();
            builder.RegisterType<MongoPreHashRepoService>().As<IMongoPreHashRepoService>();
            builder.RegisterType<SteamMarketScraperService>().As<ISteamMarketScraperService>();
            builder.RegisterType<JsonRequestParser>().As<IJsonRequestParser>();

            builder.RegisterType<RepoServiceFactory>().As<IRepoServiceFactory>();

            builder.RegisterType<UserRepoService>().As<IUserRepoService>();
            builder.RegisterType<RakeItemRepoService>().As<IRakeItemRepoService>();
            builder.RegisterType<MatchRepoService>().As<IMatchRepoService>();
            builder.RegisterType<ItemRepoService>().As<IItemRepoService>();
            builder.RegisterType<ItemDescriptionRepoService>().As<IItemDescriptionRepoService>();
            builder.RegisterType<ItemBettedRepoService>().As<IItemBettedRepoService>();
            builder.RegisterType<BotRepoService>().As<IBotRepoService>();
            builder.RegisterType<BetRepoService>().As<IBetRepoService>();
            builder.RegisterType<OfferTransactionRepoService>().As<IOfferTranascrionRepoService>();
            builder.RegisterType<ItemInOfferTransactionRepoService>().As<IItemInOfferTransactionRepoService>();
            builder.RegisterType<SettingRepoService>().As<ISettingRepoService>();
            builder.RegisterType<StaffRepoService>().As<IStaffRepoService>();
            builder.RegisterType<LevelRepoService>().As<ILevelRepoService>();
            builder.RegisterType<GameModeRepoService>().As<IGameModeRepoService>();
            builder.RegisterType<JackpotSettingRepo>().As<IJackpotSettingRepo>();
            builder.RegisterType<CoinFlipMatchRepoService>().As<ICoinFlipMatchRepoService>();
            builder.RegisterType<MongoJackpotRepoService>().As<IMongoJackpotRepoService>();
            builder.RegisterType<ItemTransferService>().As<IItemTransferService>();

            builder.RegisterType<StaffQueries>().As<IStaffQueries>();
            builder.RegisterType<LevelQueries>().As<ILevelQueries>();
            builder.RegisterType<BetQueries>().As<IBetQueries>();
            builder.RegisterType<MatchQueries>().As<IMatchQueries>();
            builder.RegisterType<ItemQueries>().As<IItemQueries>();
            builder.RegisterType<ItemDescriptionQueries>().As<IItemDescriptionQueries>();
            builder.RegisterType<ItemBetQueries>().As<IItemBetQueries>();
            builder.RegisterType<BotQueries>().As<IBotQueries>();
            builder.RegisterType<ItemInOfferTransactionQueries>().As<IItemInOfferTransactionQueries>();
            builder.RegisterType<OfferTransationQueries>().As<IOfferTransationQueries>();
            builder.RegisterType<TransactionFactory>().As<ITransactionFactory>();
            builder.RegisterType<OfferService>().As<IOfferService>();
            builder.RegisterType<PricingServiceFactory>().As<IPricingServiceFactory>();
            builder.RegisterType<GmailService>().As<IGmailService>();
            builder.RegisterType<GameModeSettingSettingService>().As<IGameModeSettingService>();

            builder.RegisterType<WebSocketSenderFactory>().As<IWebSocketSenderFactory>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetSteamHubSocketSender()).As<ISteamHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetAdminHubSocketSender()).As<ITestHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetMatchHubSocketSender()).As<IMatchHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetBetHubSocketSender()).As<IBetHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetChatHubSocketSender()).As<IChatHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetInfoHubSocketSender()).As<IInfoHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetTicketHubSocketSender()).As<ITicketHubConnections>();
            builder.Register(context => context.Resolve<IWebSocketSenderFactory>().GetCoinFlipHubSocketSender()).As<ICoinFlipHubConnections>();

            builder.RegisterInstance<IRandomService>(new RandomService(32));

            builder.RegisterType<DatabaseHelperFactory>().As<IDatabaseHelperFactory>();
            builder.RegisterType<DatabaseConnectionFactory>().As<IDatabaseConnectionFactory>();

            builder.RegisterType<AutofacLifetimeScopeResolver>().As<ILifetimeScopeResolver>();
            builder.RegisterType<AutofacScopeContext>().As<IScopeContext>();


            container = builder.Build();

            return container;
        }

        private async Task StartManagersAndCheckStatus(IContainer container)
        {
            var botManager = container.Resolve<IBotManager>();
            var rpcManager = container.Resolve<IRpcManager>();
            var offerManager = container.Resolve<IOfferManager>();
            var jackpotMatchManager = container.Resolve<IJackpotMatchManager>();
            var coinFlipManager = container.Resolve<ICoinFlipManager>();
            var steamInventoryCacheManager = container.Resolve<ISteamInventoryCacheManager>();
            var betOrWithdrawQueueManager = container.Resolve<IBetOrWithdrawQueueManager>();
            var jobScheduleManager = container.Resolve<IJobScheduleManager>();
            var hubConnectionManager = container.Resolve<IHubConnectionManager>();

            var grpcService = container.Resolve<IGrpcService>();

            var settingService = container.Resolve<ISettingsService>();
            var currentSetting = await settingService.GetCurrentSettings();


            var cacheTimeSpan = TimeSpan.FromSeconds(currentSetting.SteamInventoryCacheTimerInSec);
            steamInventoryCacheManager.CacheTimeSpan = cacheTimeSpan;
            rpcManager.Start();
            jackpotMatchManager.Start();
            jobScheduleManager.Start();
            coinFlipManager.Start();

            JobManager.Initialize((JobScheduleManager) jobScheduleManager);

            await grpcService.OnServerStart();
        }


        private void SetUpCookieOptions(CookieAuthenticationOptions o)
        {
            if (_env.EnvironmentName.Equals("Production"))
            {
                o.Cookie.Domain = ".DomainName.com";
            }

            o.LoginPath = new PathString("/api/account/login");
            o.LogoutPath = new PathString("/api/account/logout");
            o.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == (int) HttpStatusCode.OK)
                    {
                        ctx.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        ctx.Response.Redirect(ctx.RedirectUri);
                    }

                    return Task.FromResult(0);
                },
                OnSignedIn = async ctx => { await OnSignedIn(ctx); }
            };
        }

        private async Task OnSignedIn(CookieSignedInContext ctx)
        {
            var userJson = ctx.HttpContext.Items["User"];
            var steamId = ctx.Principal.GetSteamId();

            var steamApiKey = Configuration.GetSection("Steam").GetSection("ApiKey").Value;
            var url = $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steamApiKey}&steamids={steamId}";
            if (userJson == null)
                userJson = await new HttpClient().GetStringAsync(url);

            DatabaseModel.User user;
            try
            {
                var res = new Regex(@"(https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/|\.jpg)")
                    .Replace(userJson.ToString(), "");
                user = SteamPlayerMapping.GetUserFromSteamObject(res);
            }
            catch (Exception e)
            {
                throw new InvalidJsonFromSteamException("Can't convert from steamJsonPlayer data to a user", e);
            }


            if (steamId != user.SteamId)
                throw new ArgumentException("SteamId is not the same!");
            var firstTimeUser = await IoC.Container.Resolve<IUserService>().UserLoggedIn(user);
            if (firstTimeUser)
            {
                UserValidationResult userValidationResult = new UserValidationResult
                {
                    Error = "Unknown error",
                    ShouldReviceItems = false
                };

                try
                {
                    var playerInfo = await IoC.Container.Resolve<ISteamService>().GetPlayerInfoAsync(user.SteamId);
                    userValidationResult = new UserValidationResult(userJson.ToString(), playerInfo);
                    if (userValidationResult.ShouldReviceItems)
                    {
                        var userToSendFrom = await IoC.Container.Resolve<IUserService>().FindAsync(Constants.OurRandomSteamId);
                        var itemToSend = await GetItemToSend(userToSendFrom);
                        if(itemToSend.Count != 0)
                        {
                            await SendItems(user,itemToSend,userToSendFrom);
                        }
                        else
                        {
                            userValidationResult.Error = "Out of items";
                            userValidationResult.ShouldReviceItems = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                var redirectUrl =
                    $"{ctx.Request.Scheme}://{ctx.Request.Host.Host}/coinflip/create?success={userValidationResult.ShouldReviceItems.ToString()}&reason={userValidationResult.Error}";

                ctx.Response.Redirect(redirectUrl);
                var ste = ctx.HttpContext.Response.Body;
                var s = new StreamWriter(ste);
                await s.WriteAsync("");
                await s.FlushAsync();
                ctx.Response.Body = ste;
            }
        }

        private static async Task SendItems(DatabaseModel.User user, List<AssetAndDescriptionId> itemsToSend, DatabaseModel.User userToSendFrom)
        {
            await IoC.Container.Resolve<IItemTransferService>().TransferItemsAsync(userToSendFrom, user.SteamId, itemsToSend);
        }

        private static async Task<List<AssetAndDescriptionId>> GetItemToSend(DatabaseModel.User userToSendFrom)
        {
            var avalibleItems = await IoC.Container.Resolve<IItemService>().GetAvalibleItemsForUser(userToSendFrom);

            var itemsToSend = avalibleItems
                .Select(item => new AssetAndDescriptionId
                {
                    AssetId = item.AssetId,
                    DescriptionId = item.DescriptionId
                })
                .Take(5)
                .ToList();
            return itemsToSend;
        }

        private class UserValidationResult
        {
            public bool   ShouldReviceItems { get; set; }
            public string Error             { get; set; }

            public UserValidationResult()
            {
            }

            public UserValidationResult(string json, GetPlayerInfoResponse playerInfo)
            {
                var user = SteamPlayerMapping.GetSteamUserFromSteamObject(json);
//                if (user.profilestate) ;

                if (user.communityvisibilitystate != 3)
                {
                    Error = "Profile is not public";
                    ShouldReviceItems = false;
                    return;
                }

                if (playerInfo.DataCase == GetPlayerInfoResponse.DataOneofCase.Error)
                {
                    Error = playerInfo.Error.Message;
                    ShouldReviceItems = false;
                    return;
                }

                if (playerInfo.PlayerInfo.IsLimitedAccount)
                {
                    Error = "Account is limit";
                    ShouldReviceItems = false;
                    return;
                }

                if (playerInfo.PlayerInfo.TradeBanState != "None")
                {
                    Error = "Tradebanned account";
                    ShouldReviceItems = false;
                    return;
                }

                var memberSince = DateTimeOffset.FromUnixTimeSeconds(long.Parse(playerInfo.PlayerInfo.MemberSince));
                var diff = DateTime.UtcNow - memberSince;
                if (diff < TimeSpan.FromDays(90))
                {
                    Error = "The account is to new";
                    ShouldReviceItems = false;
                    return;
                }

                ShouldReviceItems = true;
            }
        }

        private async Task FirstTimeSetUp(IContainer container)
        {
            var databaseHelperFactory = container.Resolve<IDatabaseHelperFactory>();

            var BettingSettingConnectionString = Configuration.GetConnectionString("Master");
            var databaseHelperForBetting = databaseHelperFactory.GetDatabaseHelperForType(Database.Main, BettingSettingConnectionString);
            var databaseHelperForBettingSettings =
                databaseHelperFactory.GetDatabaseHelperForType(Database.Settings, BettingSettingConnectionString, "[Betting.Settings]");

            var doesDatabaseExist = await ValidateDatabaseAndTableExist(databaseHelperForBettingSettings);

            var settingRepo = container.Resolve<ISettingRepoService>();

            if (!doesDatabaseExist)
            {
                Console.WriteLine("No settings table was found, Creating one!");
                await settingRepo.SetSettingsAsync(GetObjectFromConsole<DatabaseModel.Settings>());
            }

            if (!await ValidateDatabaseAndTableExist(databaseHelperForBetting))
            {
                //TODO CREATE ALL GAMEMODES HERE!
                Console.WriteLine("No JackpotSetting table was found, Creating one!");
                var jackpotSetting = GetObjectFromConsole<DatabaseModel.JackpotSetting>();
                var jackpotSettingService = container.Resolve<IJackpotSettingRepo>();

                var jackpotId = (await jackpotSettingService.InsertAsync(jackpotSetting)).Id;

                var gameModeRepoService = container.Resolve<IGameModeRepoService>();
                await gameModeRepoService.Insert(new DatabaseModel.GameMode(GameModeHelper.GetStringFromType(GameModeType.JackpotCsgo), jackpotId));
                await gameModeRepoService.Insert(new DatabaseModel.GameMode(GameModeHelper.GetStringFromType(GameModeType.CoinFlip), 0)); //TODO FIX THIS 0 id.

                await container.Resolve<IJackpotMatchManager>().InitGameModeWhenFreshDatabase();
                var pricingService = container.Resolve<IPricingServiceFactory>().GetPricingService(PricingServices.CsgoFast);
                await pricingService.UpdatePricingForAll();
            }
        }

        private T GetObjectFromConsole<T>()
        {
            var settings = (T) FormatterServices.GetUninitializedObject(typeof(T));

            foreach (var prop in typeof(T).GetProperties())
            {
                Console.Write($"Need value for {prop.Name} : ");
                var value = Console.ReadLine();
                var changeType = Convert.ChangeType(value, prop.PropertyType);
                prop.SetValue(settings, changeType);
            }

            return settings;
        }

        private async Task<bool> ValidateDatabaseAndTableExist(IDatabaseHelper databaseHelper)
        {
            if (await databaseHelper.DoesDatabaseExist()) return true;
            databaseHelper.CreateDatabase();
            databaseHelper.CreateTables();
            return false;
        }
    }
}
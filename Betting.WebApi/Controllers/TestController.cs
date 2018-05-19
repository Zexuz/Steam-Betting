using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Managers.Impl;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Websockets;
using Betting.Backend.Wrappers.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
using RpcCommunication;
using RpcCommunicationDiscord;
using Item = RpcCommunication.Item;

namespace Betting.WebApi.Controllers
{
    [Interception]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        private IDiscordServiceClientWrapper _discordSercviceClient;

        private readonly ISteamHubConnections   _steamHubConnections;
        private readonly IRepoServiceFactory    _repoServiceFactory;
        private readonly IJackpotMatchManager   _jackpotMatchManager;
        private readonly ICoinFlipManager _coinFlipManager;

        public TestController(
            ISteamHubConnections steamHubConnections,
            IMatchHubConnections matchHubConnections,
            IRepoServiceFactory repoServiceFactory,
            IJackpotMatchManager jackpotMatchManager,
            ICoinFlipManager coinFlipManager,
            IDatabaseConnectionFactory connectionFactory,
            IGrpcServiceFactory grpcService
        )
        {
            _steamHubConnections = steamHubConnections;
            _repoServiceFactory = repoServiceFactory;
            _jackpotMatchManager = jackpotMatchManager;
            _coinFlipManager = coinFlipManager;
            _discordSercviceClient = grpcService.GetDiscordSercviceClient();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        [HttpGet("Discord/sendpm")]
        public async Task<IActionResult> TicketUpdate(string message, string steamId)
        {
            await _discordSercviceClient.SendPersonalMessageAsync(new PersonalMessageRequest
            {
                Message = message,
                SteamId = steamId
            });

            return Ok();
        }

//        [Route("resetmatch")]
//        [HttpGet]
//        [Authorize]
//        public async Task ResetMatch()
//        {
//            var connection = _connectionFactory.GetDatabaseConnection(Database.Main);
//            await connection.ExecuteNonQueryAsync(new SqlQuery("DELETE FROM ItemBetted;", new Dictionary<string, object>()));
//            await connection.ExecuteNonQueryAsync(new SqlQuery("DELETE FROM Bet;", new Dictionary<string, object>()));
//            await connection.ExecuteNonQueryAsync(new SqlQuery("DELETE FROM RakeItem;", new Dictionary<string, object>()));
//            await connection.ExecuteNonQueryAsync(new SqlQuery("DELETE FROM Match WHERE roundId != 1;", new Dictionary<string, object>()));
//            await connection.ExecuteNonQueryAsync(new SqlQuery("Update [Match] Set TimerStarted = null;", new Dictionary<string, object>()));
//            await connection.ExecuteNonQueryAsync(new SqlQuery("Update [Match] Set Status = 1;", new Dictionary<string, object>()));
//            await _matchHubConnections.NewMatchCreated(1, TimeSpan.FromSeconds(30), 50);
//            _jackpotMatchManager.Reset();
//        }


        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Route("betrandom")]
        [HttpGet]
        [Authorize]
        public async Task RandomUserBets()
        {
            var user = await _repoServiceFactory.UserRepoService.FindAsync("randomSteamId");

            var itemDesc = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync("Chroma 2 case");

            var roundId = (await _repoServiceFactory.MatchRepoService.GetCurrentMatch()).RoundId;

            if (user == null)
            {
                var insertUser = new DatabaseModel.User("randomSteamId", "Rip skins", "b2/b2f83f910fe134cd063357c0a90517ce224a4c04", "Tradelnik",
                    DateTime.Now,
                    DateTime.Now,
                    false);
                user = await _repoServiceFactory.UserRepoService.InsertAsync(insertUser);
            }

            var bots = await _repoServiceFactory.BotRepoService.GetAll();
            var botId = bots.First().Id;

            var random1 = RandomString(10);
            var random2 = RandomString(10);
            var random3 = RandomString(10);
            var random4 = RandomString(10);

            try
            {
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random1, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random2, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random3, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random4, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
            }
            catch (System.Exception e)
            {
                // ignored
            }

            _jackpotMatchManager.PlaceBetOnMatch(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = random1, DescriptionId = itemDesc.Id},
                new AssetAndDescriptionId {AssetId = random2, DescriptionId = itemDesc.Id},
                new AssetAndDescriptionId {AssetId = random3, DescriptionId = itemDesc.Id},
                new AssetAndDescriptionId {AssetId = random4, DescriptionId = itemDesc.Id},
            }, roundId, user.SteamId);
        }
        
        /// <summary>
        ///  Create a bet request on a coinflip match
        /// </summary>
        ///<remarks>
        /// Used for adding a fake user with fake skins to a coinflip match.
        /// 
        /// The Skins which is betted is N amount of Chroma 2 cases.
        /// In order for the bet to be accepted, the request needs to be within range.
        /// 
        /// </remarks>
        /// <param name="lookUpId">The lookUpId, can be found on the coinFlipMatch object.</param>
        /// <param name="nrOfSkins">Nr of skins to bet, 1-10, default is 5</param>
        [Route("coinflip/betrandom")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RandomUserBetsOnCoinFlip(int lookUpId, int nrOfSkins = 5)
        {
            if (nrOfSkins <= 0)
                return BadRequest("NrOfSkins needs to be grater than 0");
            
            if (nrOfSkins > 10)
                return BadRequest("NrOfSkins needs to be CAN'T be grater than 10");
            
            var user = await _repoServiceFactory.UserRepoService.FindAsync("randomSteamId");

            var itemDesc = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync("Chroma 2 case");

            if (user == null)
            {
                var insertUser = new DatabaseModel.User("randomSteamId", "Rip skins", "b2/b2f83f910fe134cd063357c0a90517ce224a4c04", "Tradelnik",
                    DateTime.Now,
                    DateTime.Now,
                    false);
                user = await _repoServiceFactory.UserRepoService.InsertAsync(insertUser);
            }

            var bots = await _repoServiceFactory.BotRepoService.GetAll();
            var botId = bots.First().Id;

            for (int i = 0; i < nrOfSkins; i++)
            {
                
            }
            var random1 = RandomString(10);
            var random2 = RandomString(10);
            var random3 = RandomString(10);
            var random4 = RandomString(10);

            try
            {
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random1, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random2, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random3, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(random4, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
            }
            catch (System.Exception e)
            {
                // ignored
            }

            _coinFlipManager.PlaceBet(new List<AssetAndDescriptionId>
            {
                new AssetAndDescriptionId {AssetId = random1, DescriptionId = itemDesc.Id},
                new AssetAndDescriptionId {AssetId = random2, DescriptionId = itemDesc.Id},
                new AssetAndDescriptionId {AssetId = random3, DescriptionId = itemDesc.Id},
                new AssetAndDescriptionId {AssetId = random4, DescriptionId = itemDesc.Id},
            }, lookUpId, user.SteamId);

            return Ok();
        }


        [Route("betmultiplerandom")]
        [HttpGet]
        [Authorize]
        public async Task RandomUserBets(int nrOrSkinsPlyer1, int nrOrSkinsPlyer2)
        {
            var itemDesc = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync("Chroma 2 case");
            var roundId = (await _repoServiceFactory.MatchRepoService.GetCurrentMatch()).RoundId;

            var user1 = await GetRandomUser("randomSteamId1", "Robbni");
            var user2 = await GetRandomUser("randomSteamId2", "Iski");


            var bots = await _repoServiceFactory.BotRepoService.GetAll();
            var botId = bots.First().Id;

            await BetSkinsForUser(itemDesc, botId, user1, roundId, nrOrSkinsPlyer1);
            await BetSkinsForUser(itemDesc, botId, user2, roundId, nrOrSkinsPlyer2);
        }

        [Route("sendwithdraw")]
        [HttpGet]
        [Authorize]
        public async Task SendWithdraw()
        {
            var userSteamId = User.GetSteamId();
            var request = new OfferStatusRequest
            {
                StatusCode = 6,
                SteamId = userSteamId,
                OfferSend = new OfferStatusOffer
                {
                    SteamOffer = new SteamOffer
                    {
                        ConfirmationMethod = 0,
                        Id = "id",
                        IsOurOffer = true,
                        ItemsToGive =
                        {
                            new Item
                            {
                                Amount = 1,
                                AppId = 730,
                                AssetId = "assetid",
                                IconUrlLarge = "img url",
                                MarketHashName = "marketHashName",
                            }
                        }
                    }
                },
            };
            await _steamHubConnections.SendOfferStatusToUser(request, userSteamId);
        }

        [Route("senddeposit")]
        [HttpGet]
        [Authorize]
        public async Task SendDeposit()
        {
            var userSteamId = User.GetSteamId();
            var request = new OfferStatusRequest
            {
                StatusCode = 5,
                SteamId = userSteamId,
                OfferSend = new OfferStatusOffer
                {
                    SteamOffer = new SteamOffer
                    {
                        ConfirmationMethod = 0,
                        Id = "id",
                        IsOurOffer = true,
                        ItemsToReceive =
                        {
                            new Item
                            {
                                Amount = 1,
                                AppId = 730,
                                AssetId = "assetid",
                                IconUrlLarge = "img url",
                                MarketHashName = "marketHashName",
                            }
                        }
                    }
                },
            };
            await _steamHubConnections.SendOfferStatusToUser(request, userSteamId);
        }


        private async Task BetSkinsForUser(DatabaseModel.ItemDescription itemDesc, int botId, DatabaseModel.User user, int roundId, int nrOfSkins)
        {
            var insertedItems = new List<string>();
            for (int i = 0; i < nrOfSkins; i++)
            {
                var randomAssetId = RandomString(10);
                try
                {
                    await _repoServiceFactory.ItemRepoService.InsertAsync(new DatabaseModel.Item(randomAssetId, itemDesc.Id, botId, user.Id, DateTimeOffset.Now));
                    insertedItems.Add(randomAssetId);
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }

            var itemsToBet = insertedItems.Select(assetId => new AssetAndDescriptionId {AssetId = assetId, DescriptionId = itemDesc.Id}).ToList();
            _jackpotMatchManager.PlaceBetOnMatch(itemsToBet, roundId, user.SteamId);
        }

        private async Task<DatabaseModel.User> GetRandomUser(string steamId, string name)
        {
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);

            if (user == null)
            {
                var insertUser = new DatabaseModel.User(steamId, name, "b2/b2f83f910fe134cd063357c0a90517ce224a4c04", "Tradelnik",
                    DateTime.Now,
                    DateTime.Now,
                    false);
                user = await _repoServiceFactory.UserRepoService.InsertAsync(insertUser);
            }

            return user;
        }
    }
}
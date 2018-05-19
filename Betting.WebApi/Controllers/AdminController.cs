using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpcCommunication;
using RpcCommunicationChat;
using RpcCommunicationTicket;
using BotInfo = Betting.WebApi.Models.BotInfo;
using Exception = System.Exception;
using Item = RpcCommunication.Item;
using User = RpcCommunication.User;

namespace Betting.WebApi.Controllers
{
#if DEBUG
    [Interception]
#endif
    [Route("api/[controller]")]
    [Authorize]
    [AdminRole]
    public class AdminController : Controller
    {
        private readonly ISteamService          _steamService;
        private readonly IPricingServiceFactory _pricingServiceFactory;
        private readonly ISettingsService       _settingsService;
        private readonly ILevelService          _levelService;
        private readonly IRepoServiceFactory    _repoServiceFactory;
        private readonly IStaffService          _staffService;
        private readonly IChatService           _chatService;
        private readonly ITicketService         _ticketService;
        private readonly IGrpcService           _grpcService;
        private readonly IItemTransferService   _itemTransferService;
        private readonly IUserService           _userService;

        public AdminController
        (
            ISteamService steamService,
            IPricingServiceFactory pricingServiceFactory,
            ISettingsService settingsService,
            ILevelService levelService,
            IRepoServiceFactory repoServiceFactory,
            IStaffService staffService,
            IChatService chatService,
            ITicketService ticketService,
            IGrpcService grpcService,
            IItemTransferService itemTransferService,
            IUserService userService
        )
        {
            _steamService = steamService;
            _pricingServiceFactory = pricingServiceFactory;
            _settingsService = settingsService;
            _levelService = levelService;
            _repoServiceFactory = repoServiceFactory;
            _staffService = staffService;
            _chatService = chatService;
            _ticketService = ticketService;
            _grpcService = grpcService;
            _itemTransferService = itemTransferService;
            _userService = userService;
        }

        private struct RakeAndItemDescripton
        {
            public DatabaseModel.RakeItem        RakeItem        { get; set; }
            public DatabaseModel.ItemDescription ItemDescription { get; set; }
        }

        [HttpPost("items/transfer")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> SendItemsToPlayer([FromBody] TransferItemsModel model)
        {
            var currentUser = await _userService.FindAsync(User.GetSteamId());
            if (await _itemTransferService.TransferItemsAsync(currentUser, model.ToSteamId, model.Items))
                return Ok();
            return BadRequest("Could not process request, please check input");
        }


        [HttpGet("bot/status")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<BotInfo>), 200)]
        public async Task<IActionResult> GetItemInBotInfo()
        {
            var getBotTask = _repoServiceFactory.BotRepoService.GetAll();
            var allItemsTask = _repoServiceFactory.ItemRepoService.GetAll();
            var allRakeItemRaks = _repoServiceFactory.RakeItemRepoService.GetAllWithSoldStatus(false);

            await Task.WhenAll(allItemsTask, allRakeItemRaks, getBotTask);

            var botsItemDict = new Dictionary<DatabaseModel.Bot, int>();
            var botsRakeItemDict = new Dictionary<DatabaseModel.Bot, int>();
            foreach (var bot in getBotTask.Result)
            {
                botsItemDict.Add(bot, 0);
                botsRakeItemDict.Add(bot, 0);
            }

            foreach (var item in allItemsTask.Result)
            {
                foreach (var bot in getBotTask.Result)
                {
                    if (bot.Id != item.LocationId) continue;
                    botsItemDict[bot]++;
                }
            }

            foreach (var item in allRakeItemRaks.Result)
            {
                foreach (var bot in getBotTask.Result)
                {
                    if (bot.Id != item.LocationId) continue;
                    botsRakeItemDict[bot]++;
                }
            }

            var response = new BotInfoResponseModel {BotInfos = new List<BotInfo>()};

            foreach (var bot in getBotTask.Result)
            {
                response.BotInfos.Add(new BotInfo
                {
                    Bot = bot,
                    RakeItemsCount = botsRakeItemDict[bot],
                    UserItemCount = botsItemDict[bot]
                });
            }

            return Ok(response);
        }


        [HttpGet("pingGrpc")]
        [Produces("application/json")]
        public async Task<dynamic> PingAllGrpc()
        {
            var dict = await _grpcService.CheckStatusOnGrpcServices();
            return Ok(dict);
        }


        [HttpPost("sellrake")]
        [Produces("application/json")]
        public async Task<dynamic> SellRake([FromBody] SellRakeModel sellRakeModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(sellRakeModel);

            return await _steamService.SellItemsAsync(new SellItemsFromOpskinsBotRequest
            {
                AppId = sellRakeModel.AppId,
                ContextId = sellRakeModel.ContextId,
            });
        }

        [HttpPost("sendRakeToSellBot")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<MakeOfferResponse>), 200)]
        public async Task<IActionResult> SendRakeItemToBot([FromBody] SendRakeItemsModel sendRakeItemsModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(sendRakeItemsModel);

            var dict = CreateOfferWithBotAndItems(sendRakeItemsModel);

            var botLoginDetails = await _steamService.GetBotLoginInfo(new GetBotLoginInfoRequest {Bot = new Bot {BotType = botType.Opskins}});

            if (botLoginDetails.DataCase == GetBotLoginInfoResponse.DataOneofCase.Error)
                return new ObjectResult($"Botlogin request returned error: {botLoginDetails.Error.Message}") {StatusCode = 502};

            var opskinBot = botLoginDetails.BotInfo;

            var listRespoinse = new List<MakeOfferResponse>();
            var makeOfferRequset = new List<MakeOfferRequest>();

            foreach (var kvp in dict)
            {
                var bot = await _repoServiceFactory.BotRepoService.FindAsync(kvp.Key);
                makeOfferRequset.Add(new MakeOfferRequest
                {
                    User = new User
                    {
                        SteamId = opskinBot.Bot.SteamId,
                        TradeLink = opskinBot.TradeLink,
                    },
                    BotName = bot.Name,
                    SendItems = true,
                    Message = "sendSellRakeOffer",
                    Items = {kvp.Value}
                });
            }

            foreach (var offerRequest in makeOfferRequset)
            {
                MakeOfferResponse res;
                try
                {
                    res = await _steamService.MakeOfferAsync(offerRequest);
                }
                catch (Exception e)
                {
                    e.Data.Add("MakeOfferRequest", offerRequest);
                    throw;
                }

                listRespoinse.Add(res);
            }

            return Ok(listRespoinse);
        }

        [HttpGet("rake")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(RakeItemResponseModel), 200)]
        public async Task<dynamic> GetRakeInventory([FromBody] GetRakeInvenotryModel rakeInvenotryModel)
        {
            var rakeItems = await _repoServiceFactory.RakeItemRepoService.GetAll();

            var itemDescriptionsIds = rakeItems.Select(rake => rake.DescriptionId).ToList();
            var botLocations = rakeItems.Select(rake => rake.LocationId).ToList();

            var bots = await _repoServiceFactory.BotRepoService.FindAsync(botLocations);
            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescriptionsIds);


            var res = new RakeItemResponseModel();

            foreach (var rakeItem in rakeItems)
            {
                var rakeItemModel = new RakeItemModel {RakeItem = rakeItem};

                foreach (var bot in bots)
                {
                    if (bot.Id != rakeItem.LocationId) continue;
                    rakeItemModel.Location = bot;
                    break;
                }

                foreach (var itemDescription in itemDescriptions)
                {
                    if (itemDescription.Id != rakeItem.DescriptionId) continue;
                    rakeItemModel.Description = itemDescription;
                    break;
                }

                res.Items.Add(rakeItemModel);
            }

            return Ok(res);
        }


        [HttpPost("ticket/{ticketId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SingleTicketResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<dynamic> SuportResponse([FromBody] SupportResponseModel ticket, string ticketId)
        {
            var steamId = User.GetSteamId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ticket);
            }

            var res = await _ticketService.AdminRespondToTicket(new AdminRespondToTicketRequest
            {
                Message = new InputMessage
                {
                    MessageBody = ticket.Message,
                    Name = ticket.Name
                },
                TicketId = ticketId
            });

            if (res.DataCase == SingleTicketResponse.DataOneofCase.Error)
            {
                return new ObjectResult(res.Error)
                {
                    StatusCode = 503
                };
            }

            return Ok(res);
        }

        [HttpPost("ticket")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SingleTicketResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<dynamic> CreateTicket([FromBody] SupportCreateModel ticket)
        {
            var steamId = User.GetSteamId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ticket);
            }

            var res = await _ticketService.AdminCreateTicket(new AdminCreateTicketRequest
            {
                Message = new InputMessage
                {
                    MessageBody = ticket.Message,
                    Name = ticket.Name
                },
                SteamId = ticket.SteamId,
                Title = ticket.Title
            });

            if (res.DataCase == SingleTicketResponse.DataOneofCase.Error)
            {
                return new ObjectResult(res.Error)
                {
                    StatusCode = 503
                };
            }

            return Ok(res);
        }

        [HttpPost("ticket/status/{ticketId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SingleTicketResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateTicket([FromBody] SupportChangeStatus status, string ticketId)
        {
            var steamId = User.GetSteamId();

            if (!ModelState.IsValid)
            {
                return BadRequest(status);
            }

            if (!Enum.TryParse(status.Status, true, out TicketStatus ticketStatus))
            {
                return BadRequest($"Status {status.Status} is not a valid status");
            }

            var res = await _ticketService.AdminChangeStatusOnTicket(new AdminChangeStatusOnTicketRequest
            {
                Status = ticketStatus,
                TicketId = ticketId
            });

            if (res.DataCase == SingleTicketResponse.DataOneofCase.Error)
            {
                return new ObjectResult(res.Error)
                {
                    StatusCode = 503
                };
            }

            return Ok();
        }


        [HttpGet("ticket")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ListTicketsResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetTickets(SupportListTicket model)
        {
            var ticketStatus = TicketStatus.Empty;
            if (!string.IsNullOrEmpty(model.Status))
                if (!Enum.TryParse(model.Status, true, out ticketStatus))
                {
                    return BadRequest($"Status {model.Status} is not a valid status");
                }

            var request = new AdminGetTicketsOnQueryRequest();

            if (ticketStatus != TicketStatus.Empty) request.Status = ticketStatus;
            if (!string.IsNullOrEmpty(model.SteamId)) request.SteamId = model.SteamId;
            if (!string.IsNullOrEmpty(model.TicketId)) request.TicketId = model.TicketId;

            var res = await _ticketService.AdminGetTicketsOnQuery(request);

            if (res.DataCase == ListTicketsResponse.DataOneofCase.Error)
            {
                return new ObjectResult(res.Error)
                {
                    StatusCode = 503
                };
            }

            return Ok(res);
        }


        [Produces("application/json")]
        [HttpGet("chat/mutedusers")]
        public async Task<MutedUsersResponse> GetMutedUsers()
        {
            return await _chatService.GetMutedUsers();
        }

        /// <summary>
        ///  Finds a old chat message
        /// </summary>
        ///<remarks>
        /// Used to retreive a users old chat messages
        /// </remarks>
        /// <param name="steamId">The steamId of a user to lookup</param>
        /// <param name="startTime">A valid datetime as a string</param>
        /// <param name="endTime">A valid datetime as a string</param>
        /// <response code="200">Returnes the list</response>
        /// <returns>Returns a list of ChatMessages</returns>
        [Produces("application/json")]
        [HttpGet("chat/findmessages")]
        [ProducesResponseType(typeof(List<ChatMessage>), 200)]
        public async Task<IActionResult> Find(string startTime, string endTime, string steamId)
        {
            var res = await _chatService.GetMessagesOnParams(startTime, endTime, steamId);
            return Ok(res.ChatMessage);
        }

        [Produces("application/json")]
        [ProducesResponseType(typeof(MuteUserResponse), 200)]
        [HttpPost("chat/mute")]
        public async Task<IActionResult> MuteUser([FromBody] MuteUserModel muteUserModel)
        {
            if (string.IsNullOrEmpty(muteUserModel.Reason))
                return BadRequest();

            if (string.IsNullOrEmpty(muteUserModel.SteamId))
                return BadRequest();

            if (muteUserModel.Seconds <= 0)
                return BadRequest();

            return Ok(await _chatService.MuteUser(muteUserModel.Reason, muteUserModel.Seconds, muteUserModel.SteamId));
        }

        [Produces("application/json")]
        [HttpPost("chat/unmute")]
        public async Task<IActionResult> UnMuteUser([FromBody] UnmuteUserModel unmuteUserModel)
        {
            if (string.IsNullOrEmpty(unmuteUserModel.SteamId))
                return BadRequest();
            await _chatService.UnmuteUser(unmuteUserModel.SteamId);
            return Ok();
        }

        ///  <summary>
        ///   Get current rake taken of gamemode
        ///  </summary>
        /// <remarks>
        ///  Used to retreive a Key-Value-Pair list of dates and values of rake we took for a gameModeType.
        ///  </remarks>
        ///  <param name="getRakeModel.Start">From when</param>
        ///  <param name="getRakeModel.End">To when (but not include)</param>
        ///  <param name="getRakeModel.LenghtInSec">Intervall span, 3600sec = 1 hour span.</param>
        ///  <param name="getRakeModel"></param>
        /// <param name="gameModeType">The gamemode type. Jackpot or CoinFlip</param>
        /// <response code="200">Returnes the list</response>
        ///  <returns>Returns a list of KVP</returns>
        [HttpGet("statistics/rake/{gameModeType}")]
        [ProducesResponseType(typeof(StatsResponse), 200)]
        public async Task<IActionResult> Rake(GetStatsModel getRakeModel, string gameModeType)
        {
            var gameMode = await _repoServiceFactory.GameModeRepoService.Find(gameModeType);
            if (gameMode == null)
                return BadRequest($"No game mode found for type {gameModeType}");

            var data = await _repoServiceFactory.RakeItemRepoService.FindFromGameModeIdAsync(new List<int>
            {
                gameMode.Id
            });

            var rake = data.Where(r => r.Received > getRakeModel.Start && r.Received < getRakeModel.End).ToList();
            var interval = new TimeSpan(0, 0, getRakeModel.LenghtInSec);

            var itemDescIds = rake.Select(i => i.DescriptionId).ToList();
            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescIds);

            var list = new List<RakeAndItemDescripton>();
            foreach (var itemDescription in itemDescriptions)
            {
                foreach (var item in rake)
                {
                    if (item.DescriptionId != itemDescription.Id) continue;
                    list.Add(new RakeAndItemDescripton {RakeItem = item, ItemDescription = itemDescription});
                }
            }

            var dict = new Dictionary<DateTime, decimal>();

            for (DateTime a = getRakeModel.Start; a < getRakeModel.End; a += interval)
            {
                var rake1 = list.Where(r => r.RakeItem.Received > a && r.RakeItem.Received < (a + interval)).ToList();
                dict.Add(a, rake1.Sum(r => r.ItemDescription.Value));
            }

            var res = new StatsResponse
            {
                End = getRakeModel.End,
                Text = gameMode.Type,
                Start = getRakeModel.Start,
                Values = new List<Dictionary<string, object>>()
            };

            foreach (var point in dict)
            {
                var dict1 = new Dictionary<string, object>
                {
                    {"Value", point.Value},
                    {"Time", point.Key},
                };
                res.Values.Add(dict1);
            }

            return Ok(res);
        }

        ///  <summary>
        ///   Get current rake taken of all gameModes combined
        ///  </summary>
        /// <remarks>
        /// Kinda like statistics/rake/{gameModeType} but this get all gameModes and combines them.
        ///  </remarks>
        ///  <param name="getStatsModel"></param>
        /// <response code="200">Returnes the list</response>
        ///  <returns>Returns a list of KVP</returns>
        [HttpGet("statistics/rake")]
        [ProducesResponseType(typeof(StatsResponse), 200)]
        public async Task<IActionResult> Rake(GetStatsModel getStatsModel)
        {
            var res = new StatsResponse
            {
                End = getStatsModel.End,
                Text = "All combined",
                Start = getStatsModel.Start,
                Values = new List<Dictionary<string, object>>()
            };

            var data = await _repoServiceFactory.RakeItemRepoService.GetAll();

            var rake = data.Where(r => r.Received > getStatsModel.Start && r.Received < getStatsModel.End).ToList();
            var interval = new TimeSpan(0, 0, getStatsModel.LenghtInSec);

            var itemDescIds = rake.Select(i => i.DescriptionId).ToList();
            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescIds);

            var list = new List<RakeAndItemDescripton>();
            foreach (var itemDescription in itemDescriptions)
            {
                foreach (var item in rake)
                {
                    if (item.DescriptionId != itemDescription.Id) continue;
                    list.Add(new RakeAndItemDescripton {RakeItem = item, ItemDescription = itemDescription});
                }
            }

            for (var a = getStatsModel.Start; a < getStatsModel.End; a += interval)
            {
                var rake1 = list.Where(r => r.RakeItem.Received > a && r.RakeItem.Received < (a + interval)).ToList();
                var sum = rake1.Sum(r => r.ItemDescription.Value);
                res.Values.Add(new Dictionary<string, object>
                {
                    {"Value", sum},
                    {"Time", a},
                });
            }

            return Ok(res);
        }

        /// <summary>
        ///  Used to see number of all new users created within at time frame
        /// </summary>
        /// <param name="statsModel"></param>
        /// <returns>Returns a list of KVP</returns>
        [HttpGet("statistics/user/created")]
        [ProducesResponseType(typeof(StatsResponse), 200)]
        public async Task<IActionResult> GetUserCreatedStatus(GetStatsModel statsModel)
        {
            var data = await _repoServiceFactory.UserRepoService.FindAsync(statsModel.Start, statsModel.End);

            var interval = new TimeSpan(0, 0, statsModel.LenghtInSec);

            var dict = new Dictionary<DateTime, decimal>();

            for (DateTime a = statsModel.Start; a < statsModel.End; a += interval)
            {
                var rake1 = data.Where(user => user.Created > a && user.Created < (a + interval)).ToList();
                dict.Add(a, rake1.Count);
            }

            var res = new StatsResponse
            {
                End = statsModel.End,
                Start = statsModel.Start,
                Text = "New Users",
                Values = new List<Dictionary<string, object>>()
            };

            foreach (var point in dict)
            {
                var dict1 = new Dictionary<string, object>
                {
                    {"Value", point.Value},
                    {"Time", point.Key},
                };
                res.Values.Add(dict1);
            }

            return Ok(res);
        }

        /// <summary>
        ///  Used to see number of all new matches created within at time frame
        /// </summary>
        /// <param name="statsModel"></param>
        /// <returns>Returns a list of KVP</returns>
        [HttpGet("statistics/match/played")]
        [ProducesResponseType(typeof(StatsResponse), 200)]
        public async Task<IActionResult> GetMatchPlayedStatus(GetStatsModel statsModel)
        {
            var data = await _repoServiceFactory.MatchRepoService.FindAsync(statsModel.Start, statsModel.End);

            var interval = new TimeSpan(0, 0, statsModel.LenghtInSec);

            var dict = new Dictionary<DateTime, decimal>();

            for (DateTime a = statsModel.Start; a < statsModel.End; a += interval)
            {
                var rake1 = data.Where(user => user.Created > a && user.Created < (a + interval)).ToList();
                dict.Add(a, rake1.Count);
            }

            var res = new StatsResponse
            {
                End = statsModel.End,
                Start = statsModel.Start,
                Text = "Matches played",
                Values = new List<Dictionary<string, object>>()
            };

            foreach (var point in dict)
            {
                var dict1 = new Dictionary<string, object>
                {
                    {"Value", point.Value},
                    {"Time", point.Key},
                };
                res.Values.Add(dict1);
            }

            return Ok(res);
        }

        [HttpGet("levels")]
        public async Task<dynamic> GetLevels()
        {
            return await _levelService.GetAll();
        }

        [HttpPost("level")]
        public async Task<dynamic> AddLevel([FromBody] DatabaseModel.Level level)
        {
            return await _levelService.Add(level);
        }

        [HttpPost("staff")]
        public async Task<dynamic> AddStaff([FromBody] DatabaseModel.Staff staff)
        {
            return await _staffService.Add(staff);
        }

        [HttpDelete("staff")]
        public async Task<dynamic> DeleteStaff(int id)
        {
            return await _staffService.Remove(id);
        }

        [HttpDelete("level")]
        public async Task<dynamic> DeleteLevel(int id)
        {
            return await _levelService.Remove(id);
        }

        [HttpGet("staffs")]
        public async Task<dynamic> GetStaff()
        {
            return await _staffService.GetAll();
        }

        [HttpGet("isstaff")]
        [AllowAnonymous]
        public async Task<bool> IsAdmin(string steamId)
        {
            return await _staffService.IsAdmin(steamId);
        }

        [HttpGet("settings")]
        public async Task<DatabaseModel.Settings> GetSettings()
        {
            var data = await _settingsService.GetCurrentSettings();
            return data;
        }

        [HttpPost("settings")]
        public async Task<IActionResult> SetSettings([FromBody] DatabaseModel.Settings settings)
        {
            await _settingsService.SetOrUpdateSettings(settings);
            return new OkResult();
        }

        [HttpGet("startbots")]
        public async Task<dynamic> StartBots()
        {
            return await _steamService.StartAllBotsAsync(new StartAllBotsRequest());
        }

        [HttpGet("shutdownbots")]
        public async Task<dynamic> ShutdownBots()
        {
            return await _steamService.StopAllBotsAsync(new StopAllBotsRequest());
        }

        [HttpGet("updatepricing")]
        public async Task<IActionResult> UpdatePricing()
        {
            var pricingService = _pricingServiceFactory.GetPricingService(PricingServices.CsgoFast);
            await pricingService.UpdatePricingForPubgAsync();
            await pricingService.UpdatePricingForCsgoAsync();
            return new OkResult();
        }

        private static Dictionary<string, List<Item>> CreateOfferWithBotAndItems(SendRakeItemsModel sendRakeItemsModel)
        {
            var uniqueBotLocations = sendRakeItemsModel.Items.Select(i => i.InBotSteamId).Distinct().ToList();
            var dict = new Dictionary<string, List<Item>>();
            foreach (var botSteamId in uniqueBotLocations)
            {
                dict.Add(botSteamId, new List<Item>());

                foreach (var item in sendRakeItemsModel.Items)
                {
                    if (botSteamId != item.InBotSteamId) continue;
                    dict[botSteamId].Add(new Item
                    {
                        Amount = 1,
                        AppId = item.AppId,
                        AssetId = item.AssetId,
                        ContextId = item.ContextId
                    });
                }
            }

            return dict;
        }
    }
}
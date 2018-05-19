using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Repository.Factories;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
 
using RpcCommunication;
using Serilog.Core.Trackers;

namespace Betting.WebApi.Controllers
{
    [Authorize]
    [AdminRole]
    [Route("api/[controller]")]
    public class DebugController : Controller
    {
        private readonly ISteamService       _steamService;
        private readonly IRepoServiceFactory _repoServiceFactory;
        private readonly IItemService        _itemService;
        private readonly IDiscordService _discordService;


        public DebugController
        (
            ISteamService steamService,
            IGrpcServiceFactory grpcServiceFactory,
            IRepoServiceFactory repoServiceFactory,
            IItemService itemService,
            ILogServiceFactory logServiceFactory,
            IDiscordService discordService
        )
        {
            _steamService = steamService;
            _repoServiceFactory = repoServiceFactory;
            _itemService = itemService;
            _discordService = discordService;
        }

        [AllowAnonymous]
        [HttpGet("error")]
        [HttpPost("error")]
        [HttpDelete("error")]
        [HttpOptions("error")]
        public IActionResult Index()
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var id = Guid.NewGuid().ToString();

            if (exceptionFeature != null)
            {
                var exceptionThatOccurred = exceptionFeature.Error;

                string userSteamId = "Not logged in user";
                try
                {
                    userSteamId = User.GetSteamId();
                }
                catch (System.Exception)
                {
                    // ignored
                }

                Dictionary<string, object> arguments = null;
                string loaction = "Unkown location";
                try
                {
                    arguments = HttpContext.Items["arguments"] as Dictionary<string, object>;
                    loaction = HttpContext.Items["location"].ToString();
                }
                catch (System.Exception)
                {
                    // ignored
                }

                var errorTracker = new ErrorTracker(userSteamId, loaction, "DomainName", null, exceptionThatOccurred, id, arguments);

                //WE do not want to wait for a confirmation from the sink, We either did it successfull or not. DO NOT CARE!
                Task.Factory.StartNew(() => { errorTracker.Stop(); });
                _discordService.GlobalExceptionErrorAsync(id,loaction,exceptionThatOccurred,userSteamId);
            }

            Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "*");
            Request.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");

            
            return StatusCode(500, $"An error occurred, if you continue to see this error, contact support with this id '{id}' to get help");
        }

        /// <summary>
        ///  Users inventory lookup
        /// </summary>
        ///<remarks>
        /// Used to retreive a users avalible items in his inventory
        /// </remarks>
        /// <param name="steamId">The steamId of a user to lookup</param>
        /// <response code="200">Returnes the list</response>
        /// <response code="404">The user of that steamId was not found in the database</response>
        /// <returns>Returns a list of items</returns>
        [HttpGet("fakeUserInventory")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<PlayerInventory.Item>), 200)]
        public async Task<IActionResult> FakeUserInventory(string steamId)
        {
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);
            if (user == null)
                return NotFound($"SteamId: {steamId} is not found in the database");
            return Ok(await _itemService.GetAvalibleItemsForUser(user));
        }

        [HttpGet]
        [Route("botlog")]
        public async Task<dynamic> GetLogs(SteamLogModels.BotLog botLogModel)
        {
            var request = new GetBotLoggRequest();
            if (ParseBotObject(botLogModel, out var error, out var bot))
            {
                return error;
            }

            if (botLogModel.Page.HasValue)
                request.Page = botLogModel.Page.Value;
            if (botLogModel.StatusCode.HasValue)
                request.StatusCode = botLogModel.StatusCode.Value;
            if (bot != null)
                request.Bot = bot;

            return await _steamService.GetBotLogg(request);
        }

        [HttpGet]
        [Route("logininfo")]
        public async Task<dynamic> GetLoginInfo(SteamLogModels.LoginLog loginLogModel)
        {
            var request = new GetBotLoginInfoRequest();

            if (ParseBotObject(loginLogModel, out var error, out var bot))
            {
                return error;
            }

            if (bot != null)
                request.Bot = bot;

            return await _steamService.GetBotLoginInfo(request);
        }

        [HttpGet]
        [Route("offerlog")]
        public async Task<dynamic> GetOfferLog(SteamLogModels.OfferLog offerLogModel)
        {
            var request = new GetOfferLoggRequest();

            if (ParseBotObject(offerLogModel, out var error, out var bot))
            {
                return error;
            }

            if (offerLogModel.Page.HasValue)
                request.Page = offerLogModel.Page.Value;
            if (offerLogModel.StatusCode.HasValue)
                request.StatusCode = offerLogModel.StatusCode.Value;
            if (bot != null)
                request.Bot = bot;
            if (!string.IsNullOrEmpty(offerLogModel.OfferId))
                request.OfferId = offerLogModel.OfferId;
            if (!string.IsNullOrEmpty(offerLogModel.UserSteamId))
                request.SteamId = offerLogModel.UserSteamId;

            return await _steamService.GetOfferLogg(request);
        }

        [HttpGet]
        [Route("exceptionlog")]
        public async Task<dynamic> GetExceptionLog(SteamLogModels.ExceptionLog exceptionLog)
        {
            var request = new GetExceptionLoggRequest();
            if (exceptionLog.Page.HasValue)
                request.Page = exceptionLog.Page.Value;

            return await _steamService.GetExceptionLog(request);
        }

        [HttpGet]
        [Route("opskinslog")]
        public async Task<dynamic> GetOpskinsLog(int page, int statusCode)
        {
            return await _steamService.GetOpskinsLogg(new GetOpskinsLoggRequest
            {
                Page = page,
                StatusCode = statusCode
            });
        }

        private bool ParseBotObject(SteamLogModels.Bot botLog, out ObjectResult errorResult, out Bot bot)
        {
            bot = null;
            errorResult = null;

            try
            {
                bot = GetBotObject(botLog);
            }
            catch (ArgumentException e)
            {
                errorResult = StatusCode(400, $"The botType {botLog.BotType} is not valid");
                return true;
            }

            return false;
        }

        private Bot GetBotObject(SteamLogModels.Bot botModel)
        {
            var bot = new Bot();
            if (!string.IsNullOrEmpty(botModel.BotType))
                bot.BotType = Enum.Parse<botType>(botModel.BotType, true);
            if (!string.IsNullOrEmpty(botModel.Name))
                bot.Username = botModel.Name;
            if (!string.IsNullOrEmpty(botModel.SteamId))
                bot.SteamId = botModel.SteamId;

            return bot;
        }
    }
}
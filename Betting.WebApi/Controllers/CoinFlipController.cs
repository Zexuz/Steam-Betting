using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Interfaces;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Betting.WebApi.Controllers
{
    [Interception]
    [Route("api/[controller]")]
    [ValidateModel]
    public class CoinFlipController : Controller
    {
        private readonly ICoinFlipService                _coinFlipService;
        private readonly IItemService                    _itemService;
        private readonly IRepoServiceFactory             _repoServiceFactory;
        private readonly ICoinFlipManager                _coinFlipManager;
        private readonly IHotStatusManager               _hotStatusManager;
        private          ILogService<CoinFlipController> _logService;

        public CoinFlipController
        (
            ICoinFlipService coinFlipService,
            IDatabaseConnectionFactory connectionFactory,
            IItemService itemService,
            IRepoServiceFactory repoServiceFactory,
            ICoinFlipManager coinFlipManager,
            ILogServiceFactory logServiceFactory,
            IHotStatusManager hotStatusManager
        )
        {
            _logService = logServiceFactory.CreateLogger<CoinFlipController>();
            _coinFlipService = coinFlipService;
            _itemService = itemService;
            _repoServiceFactory = repoServiceFactory;
            _coinFlipManager = coinFlipManager;
            _hotStatusManager = hotStatusManager;
        }


        [Authorize]
        [HttpPost("create")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CoinFlipMatch), 200)]
        public async Task<IActionResult> CreateCoinFlipMatch([FromBody] CreateCoinFlipModel model)
        {
            var userSteamId = User.GetSteamId();
            if (!model.Settings.AllowCsgo && !model.Settings.AllowPubg)
                return BadRequest("Either CSGO or PUBG must be enabled.");

            if (string.IsNullOrEmpty(model.CoinFlipPreHash))
                return BadRequest("Prehash needs to bet set inorder to create a match.");

//            if (model.Items.Count > model.Settings.MaxItem)
//                return BadRequest("You can't have a lower maxItem limit then you have betted items");
//
//            if (model.Items.Count < model.Settings.MinItem)
//                return BadRequest("You can't have a higher minItem limit then you have betted items");

            if (model.Settings.Diff < 5)
                return BadRequest("The diff value can't be less than 5%");

            if (model.Settings.Diff > 10000) //Upper limit is retarded and only sets limits for the users..
                return BadRequest("The diff value can't be grater than 10000%");


            var setting = new CreateCoinFlipSettingModel
            {
                AllowCsgo = model.Settings.AllowCsgo,
                AllowPubg = model.Settings.AllowPubg,
                MaxItem = 10,
                MinItem = 1,
                Diff = model.Settings.Diff,
                PreHash = model.CoinFlipPreHash,
            };

            try
            {
                var coinFlipMatch = await _coinFlipService.CreateMatch(userSteamId, model.IsHead, model.Items, setting);
                return Ok(coinFlipMatch);
            }
            catch (Exception ex)
            {
                if (
                    ex is GameModeIsNotEnabledException   ||
                    ex is ToManyItemsOnBetException       ||
                    ex is ToFewItemsOnBetException        ||
                    ex is NotAllowedAppIdOnMatchException ||
                    ex is ToMuchValueOnBetException       ||
                    ex is ToLittleValueOnBetException     ||
                    ex is InvalidItemException
                )
                {
                    return BadRequest(ex.Message);
                }

                if (ex is ToOldPreHashException)
                    return StatusCode(410, "Some error ocured, please try again");
                if (ex is PreHashNotFoundException)
                {
                    var location = ControllerContext.HttpContext.Items["location"].ToString();
                    var arguments = ControllerContext.HttpContext.Items["arguments"] as Dictionary<string, object>;
                    _logService.Error(userSteamId, location, ex, arguments);
                    return StatusCode(404, "Did not found the hash, try again");
                }

                throw;
            }
        }


        [Authorize]
        [HttpGet("prehash")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(PreHashModel), 200)]
        public async Task<IActionResult> GetPreHash()
        {
            var userSteamId = User.GetSteamId();
            var hash = await _coinFlipService.CreatePreHash(userSteamId);
            return Ok(new PreHashModel {Hash = hash});
        }

        [Authorize]
        [HttpPost("bet")]
        [Produces("application/json")]
        public async Task<IActionResult> Bet([FromBody] BetModel betModel)
        {
            var steamId = User.GetSteamId();

            var itemsToBet = betModel.Items.Select(item => new AssetAndDescriptionId
            {
                AssetId = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            await _coinFlipManager.PlaceBet(itemsToBet, betModel.RoundId, steamId);

            return new OkResult();
        }

        [HttpGet("openordrafting")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<CoinFlipMatch>), 200)]
        public async Task<IActionResult> GetOpenOrDrafting()
        {
            return Ok(await _coinFlipService.GetAllOpenOrDraftinMatchesFromMongoDb());
        }

        /// <summary>
        ///  Lookup a coinflip match
        /// </summary>
        /// <remarks>
        /// Used for getting info on a specific match in the past.
        /// </remarks>
        /// <param name="lookUpId">The lookUpId of the match, must be grater than 0</param>
        [HttpGet("match/{lookUpId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(CoinFlipMatchHistory), 200)]
        public async Task<IActionResult> GetSpecificMatch(int lookUpId)
        {
            if (lookUpId <= 0)
                return BadRequest("LookUpId must be grater than 0");

            return Ok(await _coinFlipService.GetMatchAsync(lookUpId));
        }

        /// <summary>
        /// Used for global coinflip history
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="start">The current start posision</param>
        /// <param name="count">Nr of indexes to fetch, 1-20 is valid range</param>
        [HttpGet("match/history/global")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<CoinFlipMatchHistory>), 200)]
        public async Task<IActionResult> GetGlobalHistory(int start, int count)
        {
            if (start < 0)
                return BadRequest("Start must be grater than 0");

            if (count < 0)
                return BadRequest("Count must be grater than 0");

            if (count > 20)
                return BadRequest("Count must be less than 20");

            return Ok(await _coinFlipService.GetGlobalHistory(start, count));
        }

        /// <summary>
        /// Used for private coinflip history
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="start">The current start posision</param>
        /// <param name="count">Nr of indexes to fetch, 1-20 is valid range</param>
        [Authorize]
        [HttpGet("match/history/personal")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<CoinFlipMatchHistory>), 200)]
        public async Task<IActionResult> GetPersonalHistory(int start, int count)
        {
            if (start < 0)
                return BadRequest("Start must be grater than 0");

            if (count < 0)
                return BadRequest("Count must be grater than 0");

            if (count > 20)
                return BadRequest("Count must be less than 20");

            return Ok(await _coinFlipService.GetPersonalHistory(start, count, User.GetSteamId()));
        }


        /// <summary>
        ///  Makes a match hot for a certan time.
        /// </summary>
        /// <remarks>
        /// Calling this method will trigger the websocket event and make the match hot.
        /// And after X time will trigger the websocket "NoLongerHot" event.
        /// </remarks>
        /// <param name="roundId">The GUID of the match, looks like XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX</param>
        [Authorize]
        [HttpGet("hot/{roundId}")]
        [Produces("application/json")]
        [ProducesResponseType(200)]
        public IActionResult SetMatchToHot(string roundId)
        {
            if (string.IsNullOrEmpty(roundId))
                return BadRequest("RoundId needs a value");

            _hotStatusManager.AddHotMatch(User.GetSteamId(), roundId);
            return Ok();
        }
    }
}
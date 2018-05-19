using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Managers.Impl;
using Betting.Models.Models;
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
    public class MatchController : Controller
    {
        private readonly IJackpotMatchManager _jackpotMatchManager;

        public MatchController
        (
            IJackpotMatchManager jackpotMatchManager
        )
        {
            _jackpotMatchManager = jackpotMatchManager;
        }


        [HttpGet("info")]
        public async Task<JackpotMatch> Status()
        {
            return await _jackpotMatchManager.GetCurrentMatch();
        }

        [HttpGet]
        [Route("history/{roundId}")]
        public async Task<IActionResult> HistoryDetailed(int roundId)
        {
            if (roundId <= 0)
                return BadRequest("RoundId needs to be grater than 0");
            
            return Ok(await _jackpotMatchManager.GetMatchHistory(roundId));
        }

        [HttpGet]
        [Route("history")]
        public async Task<dynamic> History(int fromId)
        {
            return await _jackpotMatchManager.GetMatchHistory(fromId, 20);
        }

        [Authorize]
        [HttpPost("bet")]
        public async Task<IActionResult> Bet([FromBody] BetModel betModel)
        {
            var steamId = User.GetSteamId();

            var itemsToBet = betModel.Items.Select(item => new AssetAndDescriptionId
            {
                AssetId       = item.AssetId,
                DescriptionId = item.DescriptionId,
            }).ToList();

            _jackpotMatchManager.PlaceBetOnMatch(itemsToBet, betModel.RoundId, steamId);

            return new OkResult();
        }
    }
}
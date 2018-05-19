using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Wrappers.Interfaces;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RpcCommunicationHistory;


namespace Betting.WebApi.Controllers
{
    [Interception]
    [Route("api/[controller]")]
    public class HistoryController : Controller
    {
        private readonly IHistoryServiceClientWrapper _historyService;

        public HistoryController(IGrpcServiceFactory grpcServiceFactory)
        {
            _historyService = grpcServiceFactory.GetHistorySercviceClient();
        }

        /// <summary>
        ///  History from Bettingv1. 
        /// </summary>
        ///<remarks>
        /// Used to retreive a list of match histories from Bettingv1
        /// 
        /// Current page limit is 10
        /// </remarks>
        /// <param name="offset">The offset to start from, EG 10</param>
        /// <response code="200">Returnes the list</response>
        /// <response code="400">Offset is not in a valid range</response>
        /// <returns>Returns a personal list of matchHistories from Bettingv1</returns>
        [HttpGet("Match/Global")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MatchResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Global(int offset)
        {
            if (offset < 0)
                return BadRequest("Offset need to be grater than 0");
            return Ok(await _historyService.GetGlobalMatchHistoryAsync(new GetGlobalHistoryRequest
            {
                Limt = 10,
                Offset = offset
            }));
        }
        
        /// <summary>
        ///  History from Bettingv1. 
        /// </summary>
        ///<remarks>
        /// Used to retreive a list of match histories that the current logged in user was participation in from Bettingv1
        /// 
        /// Current page limit is 10
        /// </remarks>
        /// <param name="offset">The offset to start from, EG 10</param>
        /// <response code="200">Returnes the list</response>
        /// <response code="400">Offset is not in a valid range</response>
        /// <returns>Returns a personal list of matchHistories from Bettingv1</returns>
        [Authorize]
        [HttpGet("Match/Private")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MatchResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Private(int offset)
        {
            if (offset < 0)
                return BadRequest("Offset need to be grater than 0");
            var steamId = User.GetSteamId();
            return Ok(await _historyService.GetPersonalMatchHistoryAsync(new GetPersonalHistoryRequest
            {
                Limt = 10,
                Offset = offset,
                SteamId = steamId
            }));
        }
    }
}
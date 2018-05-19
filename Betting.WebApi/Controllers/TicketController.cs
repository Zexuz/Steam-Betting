using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
using RpcCommunicationTicket;

namespace Betting.WebApi.Controllers
{
    [Interception]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }


        [HttpPost("ticket")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SingleTicketResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateTicket([FromBody] TicketCreateModel ticket)
        {
            var steamId = User.GetSteamId();

            if (!ModelState.IsValid)
            {
                return BadRequest(ticket);
            }

            var res = await _ticketService.UserCreateTicket(new UserCreateTicketRequest
            {
                Message = new InputMessage
                {
                    MessageBody = ticket.Message,
                    Name = "User"
                },
                SteamId = steamId,
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

        [HttpPost("ticket/{ticketId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SingleTicketResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> UpdateTicket([FromBody] TicketResponseModel ticket, string ticketId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ticket);
            }

            var steamId = User.GetSteamId();
            var res = await _ticketService.UserRespondToTicket(new UserRespondToTicketRequest
            {
                Message = new InputMessage
                {
                    MessageBody = ticket.Message,
                    Name = "User"
                },
                SteamId = steamId,
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

        [HttpGet("ticket")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ListTicketsResponse), 200)]
        public async Task<IActionResult> GetAllTickets()
        {
            var steamId = User.GetSteamId();
            var res = await _ticketService.UserGetAllTickets(steamId);

            if (res.DataCase == ListTicketsResponse.DataOneofCase.Error)
            {
                return new ObjectResult(res.Error)
                {
                    StatusCode = 503
                };
            }

            return Ok(res);
        }

        [HttpGet("ticket/unread")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(UserCountUnreadTicketsResponse), 200)]
        public async Task<IActionResult> UnreadTicketsCount()
        {
            var steamId = User.GetSteamId();
            var res = await _ticketService.UserCountUnreadTickets(steamId);
            return Ok(res);
            
        }

        [HttpPost("unread/{ticketId}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(SingleTicketResponse), 200)]
        public async Task<IActionResult> SetTicketReadStatus(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
                return BadRequest("TicketId needs a value");

            var steamId = User.GetSteamId();
            var res = await _ticketService.UserMarkTicketAsRead(steamId, ticketId);
            if (res.DataCase == SingleTicketResponse.DataOneofCase.Error)
            {
                return new ObjectResult(res.Error)
                {
                    StatusCode = 503
                };
            }
            return Ok(res);
        }
    }
}
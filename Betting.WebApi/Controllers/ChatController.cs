using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Betting.WebApi.Controllers
{
    [Interception]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [Authorize]
        [HttpPost("Message")]
        public async Task<IActionResult> SendMessage([FromBody] SendChatMessageModel messageModel)
        {
            var steamId = User.GetSteamId();
            await _chatService.SendMessage(messageModel.Message, steamId);
            return Ok();
        }

        [HttpGet("Message")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(List<ChatMessageModel>), 200)]
        public async Task<IActionResult> GetMessges()
        {
            var data = await _chatService.GetLatestMessages();
            return Ok(data);
        }
    }
}
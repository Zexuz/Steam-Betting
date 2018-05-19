using System.Threading.Tasks;
using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Wrappers.Interfaces;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
using RpcCommunicationDiscord;

namespace Betting.WebApi.Controllers
{
    [Interception]
    [Authorize]
    [Route("api/[controller]")]
    public class DiscordController : Controller
    {
        private readonly IDiscordService                _discordService;
        private readonly IDiscordServiceClientWrapper   _discordSercviceClient;
        private readonly ILogService<DiscordController> _logger;

        public DiscordController(IDiscordService discordService, IGrpcServiceFactory grpcServiceFactory, ILogServiceFactory factory)
        {
            _discordService = discordService;
            _discordSercviceClient = grpcServiceFactory.GetDiscordSercviceClient();
            _logger = factory.CreateLogger<DiscordController>();
        }

        [HttpGet("auth")]
        public IActionResult Auth()
        {
            return Redirect(_discordService.AuthService.AuthUrl);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var steamId = User.GetSteamId();

            var discordAuthResource = await _discordService.AuthService.GetDiscordAuthResource(code, _discordService.AuthService.RedirectUrl);
            var profileInfo = await _discordService.AuthService.GetProfileInfo(discordAuthResource.AccessToken);

            await _discordSercviceClient.AddUserAsync(new AddUserRequest
            {
                Id = profileInfo.Id,
                SteamId = steamId
            });

            return Ok();
        }
    }
}
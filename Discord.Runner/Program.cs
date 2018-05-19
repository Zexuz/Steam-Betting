using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Backend;
using Discord.Commands;
using Discord.WebSocket;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Runner
{
    class Program
    {

        public static Task Main(string[] args) => Startup.RunAsync(args);

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        public async Task MainAsync()
        {


            await Task.Delay(-1);
        }


//        private async void DiscordServiceServerOnTicketResponse(object sender, PersonalMessageRequest request)
//        {
//            var disscordId = _db.FindDiscordIdFromSteamId(request.SteamId);
//
//            if (disscordId == null) return;
//
//            if (!(_client.GetChannel(401906186611326976) is ISocketMessageChannel channel)) return;
//
//            var user = await channel.GetUserAsync(disscordId.Value);
//            await user.SendMessageAsync(request.Message);
//        }
//
//        private async void DiscordServiceServerOnAddUser(object sender, AddUserRequest request)
//        {
//            if (_db.DoesSteamIdExist(request.SteamId)) return;
//
//            if (!(_client.GetChannel(401906186611326976) is ISocketMessageChannel channel)) return;
//
//            var user = await channel.GetUserAsync(Convert.ToUInt64(request.Id));
//            await channel.SendMessageAsync($"User {user.Mention}{user.Username} just synced DomainName with discord!");
//
//            _db.InsertUser(request.Id, request.SteamId);
//        }
    }
}
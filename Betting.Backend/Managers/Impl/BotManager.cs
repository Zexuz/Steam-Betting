using Betting.Backend.Implementations;
using Betting.Backend.Managers.Interface;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using RpcCommunication;

namespace Betting.Backend.Managers.Impl
{
    public class BotManager : IBotManager
    {
        private IBotRepoService _botServiceRepo;

        public BotManager(RpcSteamListener rpcSteamListener, IRepoServiceFactory factory)
        {
            _botServiceRepo                     =  factory.BotRepoService;
            rpcSteamListener.OnBotStatusChanged += RpcSteamListenerOnOnBotStatusChanged;
        }

        private async void RpcSteamListenerOnOnBotStatusChanged(object sender, BotStatusChangedRequest offer)
        {
            //todo make this into the manger it deservs!
            if (offer.Bot.BotType != botType.OfferVendor) return;
            if (offer.StatusCode  != 1) return;

            var reqBot = offer.Bot;

            var databaseBot = await _botServiceRepo.FindAsync(offer.Bot.SteamId);
            if (databaseBot == null)
            {
                await _botServiceRepo.InsertAsync(new DatabaseModel.Bot(reqBot.SteamId, reqBot.Username));
            }
        }
    }
}
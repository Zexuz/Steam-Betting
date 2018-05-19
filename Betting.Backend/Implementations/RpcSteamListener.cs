using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Grpc.Core;
using RpcCommunication;

namespace Betting.Backend.Implementations
{
    public class RpcSteamListener : StatusChanged.StatusChangedBase
    {
        private ILogService<RpcSteamListener> _logService;

        public delegate void OfferStatusChangedDelegate(object sender, OfferStatusRequest offer);

        public delegate void BotStatusChangedDelegate(object sender, BotStatusChangedRequest offer);

        public delegate void OpSkinsStatusChangedDelegate(object sender, OpskinsStatusChangedRequest offer);

        public delegate void SteamStatusChangedDelegate(object sender, SteamStatusRequest offer);


        public event OfferStatusChangedDelegate   OnOfferStatusChanged;
        public event BotStatusChangedDelegate     OnBotStatusChanged;
        public event OpSkinsStatusChangedDelegate OnOpSkinsStatusChanged;
        public event SteamStatusChangedDelegate   OnSteamStatusChanged;

        public RpcSteamListener(ILogServiceFactory logServiceFactory)
        {
            _logService = logServiceFactory.CreateLogger<RpcSteamListener>();
        }

        public override Task<OfferStatusResponse> OfferStatusChanged(OfferStatusRequest request, ServerCallContext context)
        {
            OnOfferStatusChanged?.Invoke(this, request);
            return Task.FromResult(new OfferStatusResponse
            {
                ItIsHandled = true
            });
        }

        public override Task<BotStatusChangedResponse> BotStatusChanged(BotStatusChangedRequest request, ServerCallContext context)
        {
            OnBotStatusChanged?.Invoke(this, request);
            return Task.FromResult(new BotStatusChangedResponse());
        }

        public override Task<OpskinsStatusChangedResponse> OpskinsStatusChanged(OpskinsStatusChangedRequest request, ServerCallContext context)
        {
            OnOpSkinsStatusChanged?.Invoke(this, request);
            return Task.FromResult(new OpskinsStatusChangedResponse());
        }

        public override Task<SteamStatusResponse> SteamStatusChanged(SteamStatusRequest request, ServerCallContext context)
        {
            OnSteamStatusChanged?.Invoke(this, request);
            return Task.FromResult(new SteamStatusResponse());
        }
    }
}
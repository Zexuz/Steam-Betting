using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Betting.Backend.Cache;
using Betting.Backend.Factories;
using Betting.Backend.Implementations;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Wrappers.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using FakeItEasy;
 
using RpcCommunication;
using Xunit;
using Item = RpcCommunication.Item;

namespace Betting.Backend.Test
{
    public class OfferManagerTest
    {
        private  OfferStatusRequest         _offerMinmalInfo;
        private readonly ISteamHubConnections       _fakedSteamHubConnection;
        private readonly IOfferService              _fakedOfferService;
        private readonly IGrpcServiceFactory        _fakedGrpcServiceFactory;
        private readonly IUserService               _fakedUserService;
        private readonly ILogServiceFactory         _loggerDummy;
        private readonly ISteamServiceClientWrapper _fakedSteamServiceClient;
        private readonly IRepoServiceFactory        _fakedRepoServiceFactory;
        private readonly IRakeItemRepoService       _fakedRakeItemRepo;

        public OfferManagerTest()
        {
            _fakedSteamHubConnection = A.Fake<ISteamHubConnections>();
            _fakedOfferService = A.Fake<IOfferService>();
            _fakedGrpcServiceFactory = A.Fake<IGrpcServiceFactory>();
            _fakedUserService = A.Fake<IUserService>();
            _loggerDummy = A.Dummy<ILogServiceFactory>();
            _fakedSteamServiceClient = A.Fake<ISteamServiceClientWrapper>();
            _fakedRepoServiceFactory = A.Fake<IRepoServiceFactory>();
            _fakedRakeItemRepo = A.Fake<IRakeItemRepoService>();

            A.CallTo(() => _fakedRepoServiceFactory.RakeItemRepoService).Returns(_fakedRakeItemRepo);

            _offerMinmalInfo = new OfferStatusRequest
            {
                Bot = new Bot
                {
                    Username = "botName",
                    SteamId = "botSteamId"
                },
                SteamId = "user Steamid",
                StatusCode = int.MinValue,
                StatusMessage = "",
                OfferSend = new OfferStatusOffer
                {
                    SteamOffer = new SteamOffer
                    {
                        ItemsToGive =
                        {
                            new Item {AppId = 730, ContextId = "2", AssetId = "1"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "2"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "3"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "4"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "5"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "6"},
                        },
                        ItemsToReceive =
                        {
                            new Item {AppId = 730, ContextId = "2", AssetId = "11"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "12"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "13"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "14"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "15"},
                            new Item {AppId = 730, ContextId = "2", AssetId = "16"},
                        },
                    }
                }
            };

            var userFindRes =
                Task.FromResult(new DatabaseModel.User(_offerMinmalInfo.SteamId, null, null, null, DateTime.Today, DateTime.Today, false));
            A.CallTo(() => _fakedUserService.FindAsync(_offerMinmalInfo.SteamId)).Returns(userFindRes);
            A.CallTo(() => _fakedGrpcServiceFactory.GetSteamServiceClient(A<ISteamInventoryCacheManager>._)).Returns(_fakedSteamServiceClient);

            A.CallTo(() => _fakedSteamServiceClient.GetPlayerInfoAsync(new GetPlayerInfoRequest
            {
                SteamId = _offerMinmalInfo.SteamId
            })).Returns(Task.FromResult(new GetPlayerInfoResponse
            {
                PlayerInfo = new PlayerInfo
                {
                    Avatar = "",
                    PersonaName = ""
                }
            }));
        }

        [Fact]
        public async Task SendSellRakeOfferWasAcceptedSuccess()
        {
            _offerMinmalInfo.StatusCode = 6;
            _offerMinmalInfo.OfferSend.SteamOffer.Message = "sendSellRakeOffer";
            
            var offerManager = new OfferManager(
                _fakedSteamHubConnection,
                _fakedOfferService,
                new RpcSteamListener(_loggerDummy),
                _loggerDummy,
                _fakedGrpcServiceFactory,
                _fakedUserService,
                _fakedRepoServiceFactory
            );

            await offerManager.HandleOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedRakeItemRepo.SetAsSold(A<List<string>>.That.Matches(l => l.Count == 6))).MustHaveHappened();
            A.CallTo(() => _fakedSteamHubConnection.SendOfferStatusToUser(A<OfferStatusRequest>._,A<string>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public async Task OfferStatusReturnsError()
        {
            _offerMinmalInfo.StatusCode = 6;
            _offerMinmalInfo.Error = new OfferStatusError
            {
                Message = "Error"
            };
            
            var offerManager = new OfferManager(
                _fakedSteamHubConnection,
                _fakedOfferService,
                new RpcSteamListener(_loggerDummy),
                _loggerDummy,
                _fakedGrpcServiceFactory,
                _fakedUserService,
                _fakedRepoServiceFactory
            );

            await offerManager.HandleOffer(_offerMinmalInfo);
            
            A.CallTo(() => _fakedSteamHubConnection.SendErrorMessageRelatedToOurApi(A<string>._,A<string>._)).MustHaveHappened();
            A.CallTo(() => _fakedRakeItemRepo.SetAsSold(A<List<string>>._)).MustNotHaveHappened();
            A.CallTo(() => _fakedSteamHubConnection.SendOfferStatusToUser(A<OfferStatusRequest>._,A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task OfferWasSentToUserNofityOnStatusSuccessTest()
        {
            _offerMinmalInfo.StatusCode = 1;

            var offerManager = new OfferManager(
                _fakedSteamHubConnection,
                _fakedOfferService,
                new RpcSteamListener(_loggerDummy),
                _loggerDummy,
                _fakedGrpcServiceFactory,
                _fakedUserService,
                _fakedRepoServiceFactory
            );

            await offerManager.HandleOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedSteamHubConnection.SendOfferStatusToUser(A<OfferStatusRequest>.That.Matches(request => request.StatusCode == 1),
                ("user Steamid"))).MustHaveHappened();
        }

        [Fact]
        public async Task OfferWillBeInEscrowNotifyUserSuccessTest()
        {
            _offerMinmalInfo.StatusCode = 3;

            var offerManager = new OfferManager(
                _fakedSteamHubConnection,
                _fakedOfferService,
                new RpcSteamListener(_loggerDummy),
                _loggerDummy,
                _fakedGrpcServiceFactory,
                _fakedUserService,
                _fakedRepoServiceFactory
            );

            await offerManager.HandleOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedSteamHubConnection.SendOfferStatusToUser(A<OfferStatusRequest>.That.Matches(request => request.StatusCode == 3),
                ("user Steamid"))).MustHaveHappened();
        }

        [Fact]
        public async Task UserAcceptedDepositOfferSuccessTest()
        {
            _offerMinmalInfo.StatusCode = 5;

            var offerManager = new OfferManager(
                _fakedSteamHubConnection,
                _fakedOfferService,
                new RpcSteamListener(_loggerDummy),
                _loggerDummy,
                _fakedGrpcServiceFactory,
                _fakedUserService,
                _fakedRepoServiceFactory
            );


            await offerManager.HandleOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedSteamServiceClient.GetPlayerInfoAsync(new GetPlayerInfoRequest
            {
                SteamId = _offerMinmalInfo.SteamId
            })).MustHaveHappened();

            A.CallTo(() => _fakedUserService.FindAsync(_offerMinmalInfo.SteamId)).MustHaveHappened();
            A.CallTo(() => _fakedUserService.UpdateUserInfoIfNeeded(A<DatabaseModel.User>._, A<DatabaseModel.User>._)).MustHaveHappened();

            A.CallTo(() => _fakedSteamHubConnection.SendOfferStatusToUser(A<OfferStatusRequest>.That.Matches(request => request.StatusCode == 5),
                ("user Steamid"))).MustHaveHappened();
            A.CallTo(() => _fakedOfferService.DepositSteamOfferAsync(A<OfferStatusRequest>._)).MustHaveHappened();
        }

        [Fact]
        public async Task UserAcceptedWithdrawOfferSuccessTest()
        {
            _offerMinmalInfo.StatusCode = 6;

            var offerManager = new OfferManager(
                _fakedSteamHubConnection,
                _fakedOfferService,
                new RpcSteamListener(_loggerDummy),
                _loggerDummy,
                _fakedGrpcServiceFactory,
                _fakedUserService,
                _fakedRepoServiceFactory
            );

            await offerManager.HandleOffer(_offerMinmalInfo);

            A.CallTo(() => _fakedSteamServiceClient.GetPlayerInfoAsync(new GetPlayerInfoRequest
            {
                SteamId = _offerMinmalInfo.SteamId
            })).MustHaveHappened();

            A.CallTo(() => _fakedUserService.FindAsync(_offerMinmalInfo.SteamId)).MustHaveHappened();
            A.CallTo(() => _fakedUserService.UpdateUserInfoIfNeeded(A<DatabaseModel.User>._, A<DatabaseModel.User>._)).MustHaveHappened();

            A.CallTo(() => _fakedSteamHubConnection.SendOfferStatusToUser(A<OfferStatusRequest>.That.Matches(request => request.StatusCode == 6),
                ("user Steamid"))).MustHaveHappened();
            A.CallTo(() => _fakedOfferService.WithdrawalSteamOffer(A<OfferStatusRequest>._)).MustHaveHappened();
        }
    }
}
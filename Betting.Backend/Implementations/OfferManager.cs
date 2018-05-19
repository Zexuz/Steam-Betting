using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Cache;
using Betting.Backend.Exceptions;
using Betting.Backend.Factories;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Backend.Websockets;
using Betting.Backend.Wrappers.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using Google.Protobuf.Collections;
using RpcCommunication;
using Item = RpcCommunication.Item;

namespace Betting.Backend.Implementations
{
    public class OfferManager : IOfferManager
    {
        private readonly ISteamHubConnections _steamHubConnections;
        private readonly IOfferService        _offerService;
        private readonly IUserService         _userService;

        private readonly ILogService<OfferManager>  _logger;
        private readonly ISteamServiceClientWrapper _steamServiceClient;
        private readonly IRakeItemRepoService       _rakeItemRepoService;

        public OfferManager(
            ISteamHubConnections steamHubConnections,
            IOfferService offerService,
            RpcSteamListener steamListener,
            ILogServiceFactory logger,
            IGrpcServiceFactory factory,
            IUserService userService,
            IRepoServiceFactory repoServiceFactory
        )
        {
            _rakeItemRepoService = repoServiceFactory.RakeItemRepoService;
            _steamServiceClient = factory.GetSteamServiceClient(new SteamInventoryCacheManager());
            _steamHubConnections = steamHubConnections;
            _offerService = offerService;
            _userService = userService;
            _logger = logger.CreateLogger<OfferManager>();

            steamListener.OnOfferStatusChanged += async (sender, offer) =>
            {
                _logger.Info($"Received statuscode {offer.StatusCode}");

                try
                {
                    await HandleOffer(offer);
                }
                catch (System.Exception e)
                {
                    _logger.Error(offer.SteamId, null, e, new Dictionary<string, object>
                    {
                        {"Offer", offer}
                    });
                }
            };
        }

        public async Task HandleOffer(OfferStatusRequest offer)
        {
            try
            {
                if (offer.DataCase == OfferStatusRequest.DataOneofCase.OfferSend && offer.OfferSend.SteamOffer.Message == "sendSellRakeOffer")
                {
                    var itemsSuccessfulySentToBot = offer.OfferSend.SteamOffer.ItemsToGive;
                    var assetIds = itemsSuccessfulySentToBot.Select(item => item.AssetId).ToList();

                    await _rakeItemRepoService.SetAsSold(assetIds);
                    return;
                }
            }
            catch (System.Exception e)
            {
                _logger.Error(offer.SteamId, null, e, new Dictionary<string, object>
                {
                    {"Offer", offer}
                });
                return;
            }

            if (offer.DataCase == OfferStatusRequest.DataOneofCase.Error)
            {
                await _steamHubConnections.SendErrorMessageRelatedToOurApi("Internal error processing offer, please try again later!", offer.SteamId);
                return;
            }

            FixSteamItemsBadNaming(offer);
            
            switch (offer.StatusCode)
            {
                case 5:
                    await HandleDeposit(offer);
                    break;
                case 6:
                    await HandleWithdrawal(offer);
                    break;
                case 14:
                case 16:
                case 19:
                case 20:
                    await RemoveCanceledOfferFromOfferTransaction(offer);
                    break;
            }

            var request = new OfferStatusRequest
            {
                StatusCode = offer.StatusCode,
                SteamId = offer.SteamId,
                OfferSend = offer.OfferSend
            };
            await _steamHubConnections.SendOfferStatusToUser(request, offer.SteamId);
        }

        private async Task RemoveCanceledOfferFromOfferTransaction(OfferStatusRequest offer)
        {
            if (offer.SendItem)
                await _offerService.RemoveCanceledWithdrawalSteamOffer(offer);
        }

        private async Task HandleWithdrawal(OfferStatusRequest offer)
        {
            await UpdateUserDataAsync(offer);

            try
            {
                await _offerService.WithdrawalSteamOffer(offer);
            }
            catch (SteamMarketNameFuckupException e)
            {
                e.Data.Add("steamId",offer.SteamId);
                _logger.Critical(e);
                await _steamHubConnections.SendErrorMessageRelatedToOurApi("Internal error processing offer, please try again later!", offer.SteamId);
            }
        }

        private async Task HandleDeposit(OfferStatusRequest offer)
        {
            await UpdateUserDataAsync(offer);
            try
            {
                await _offerService.DepositSteamOfferAsync(offer);
            }
            catch (CantCompleteSteamDeposit e)
            {
                _logger.Error(offer.SteamId, null, e, new Dictionary<string, object>
                {
                    {"Offer", offer}
                });
                await _steamHubConnections.SendErrorMessageRelatedToOurApi("Internal error processing offer, please try again later!", offer.SteamId);
            }
            catch (SteamMarketNameFuckupException e)
            {
                e.Data.Add("steamId",offer.SteamId);
                _logger.Critical(e);
                await _steamHubConnections.SendErrorMessageRelatedToOurApi("Internal error processing offer, please try again later!", offer.SteamId);
            }
        }

        private void FixSteamItemsBadNaming(OfferStatusRequest offer)
        {
            //Here we are fixing items beacuse steam is a SHITTY API.
            //Boots (Punk) ska göras om till Punk Boots

            if (offer.DataCase != OfferStatusRequest.DataOneofCase.Error)
            {
                RenameItems(offer.OfferSend.SteamOffer.ItemsToGive);
                RenameItems(offer.OfferSend.SteamOffer.ItemsToReceive);
            }
        }

        private static void RenameItems(RepeatedField<Item> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var itemName = items[i].MarketHashName;
                if (itemName == "Boots (Punk)")
                {
                    items[i].MarketHashName = "Punk Boots";
                }
            }
        }

        private async Task UpdateUserDataAsync(OfferStatusRequest offer)
        {
            try
            {
                var userTask = _userService.FindAsync(offer.SteamId);
                var steamPlayerTask = _steamServiceClient.GetPlayerInfoAsync(new GetPlayerInfoRequest
                {
                    SteamId = offer.SteamId
                });
                await Task.WhenAll(userTask, steamPlayerTask);
                if (steamPlayerTask.Result.DataCase == GetPlayerInfoResponse.DataOneofCase.Error)
                    throw new BadResponseException(steamPlayerTask.Result.Error.Message);

                var playerInfo = steamPlayerTask.Result.PlayerInfo;

                var newUserData = new DatabaseModel.User
                (
                    playerInfo.SteamId,
                    playerInfo.PersonaName,
                    playerInfo.Avatar,
                    userTask.Result.TradeLink,
                    userTask.Result.Created,
                    DateTime.Now,
                    userTask.Result.SuspendedFromQuote,
                    userTask.Result.Quote
                );
                await _userService.UpdateUserInfoIfNeeded(newUserData, userTask.Result);
            }
            catch (System.Exception e)
            {
                e.Data.Add("Offer", offer);
                _logger.Error(offer.SteamId, "OfferManager", e);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Managers.Impl;
using Betting.Backend.Managers.Interface;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Google.Protobuf.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
 
using RpcCommunication;
using Exception = System.Exception;
using Item = RpcCommunication.Item;

namespace Betting.WebApi.Controllers
{
    [Interception]
    [Route("api/[controller]")]
    public class SteamController : Controller
    {
        private readonly ISteamService              _steamService;
        private readonly IBetOrWithdrawQueueManager _betOrWithdrawQueueManager;
        private readonly IRepoServiceFactory        _repoServiceFactory;

        public SteamController
        (
            ISteamService steamService,
            IBetOrWithdrawQueueManager betOrWithdrawQueueManager,
            IRepoServiceFactory repoServiceFactory
        )
        {
            _steamService              = steamService;
            _betOrWithdrawQueueManager = betOrWithdrawQueueManager;
            _repoServiceFactory        = repoServiceFactory;
        }

        [HttpGet]
        [Authorize]
        [Route("profile")]
        public async Task<dynamic> Profile()
        {
            var steamId = User.GetSteamId();
            return await _steamService.GetPlayerInfoAsync(steamId);
        }

        [HttpPost]
        [Route("deposit")]
        public async Task<dynamic> MakeDepositOffer([FromBody] DepositOfferModel depositModel)
        {
            if (!TryValidateModel(depositModel))
            {
                return BadRequest(depositModel);
            }

            var steamId = User.GetSteamId();
            var items   = depositModel.Items.Select(item => new Item
            {
                AssetId   = item.AssetId,
                ContextId = item.ContextId,
                AppId     = item.AppId
            }).ToList();

            try
            {
                return await _steamService.MakeDepositOfferAsync(steamId, items);
            }
            catch (TradeLinkNotSetExeption)
            {
                return BadRequest("User does not have a valid tradelink");
            }
            catch (InventoryLimitExceeded e) {return BadRequest(e.Message);}
            catch (ItemDescriptionNotInDatabase e) {return BadRequest(e.Message);}
            catch (ItemInOfferNotMatchingLowestValueRuleExecption e) {return BadRequest(e.Message);}
            catch (NoBotInDatabaseException)
            {
                await _steamService.StartAllBotsAsync(new StartAllBotsRequest());
                throw;
            }
        }

        [HttpPost]
        [Route("withdraw")]
        public async Task<dynamic> MakeWithdrawOffer([FromBody] WithdrawOfferModel withdrawModel)
        {
            if (!TryValidateModel(withdrawModel))
            {
                return BadRequest(withdrawModel);
            }

            var steamId = User.GetSteamId();
            var items   = withdrawModel.Items.Select(item => new AssetAndDescriptionId
            {
                AssetId       = item.AssetId,
                DescriptionId = item.DescriptionId
            }).ToList();

            if (_betOrWithdrawQueueManager.DoesExist(steamId))
                return new StatusCodeResult(503);
            _betOrWithdrawQueueManager.Add(steamId, QueueAction.Withdraw);
            try
            {
                var res = await _steamService.MakeWithdrawOfferAsync(steamId, items);
                return res;
            }
            catch (TradeLinkNotSetExeption)
            {
                return BadRequest("User does not have a valid tradelink");
            }
            finally
            {
                _betOrWithdrawQueueManager.Remover(steamId);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("activewithdrawoffers")]
        public async Task<List<string>> ActiveSteamOffers()
        {
            var steamId = User.GetSteamId();
            var user    = _repoServiceFactory.UserRepoService.FindAsync(steamId);
            var offers  = await _repoServiceFactory.OfferTranascrionRepoService.FindActiveAsync(await user);
            return offers.Select(offer => offer.SteamOfferId).ToList();
        }


        ///  <summary>
        ///   Get the steam inventory for all enabled games.
        ///  </summary>
        /// <remarks>
        ///  </remarks>
        /// <response code="200">Returnes the list</response>
        ///  <returns>Returns a list of steam inventories</returns>
        [HttpGet]
        [Authorize]
        [Route("inventory")]
        [ProducesResponseType(typeof(List<Inventory>), 200)]
        public async Task<IActionResult> Invenotry()
        {
            var steamId        = User.GetSteamId();
            var csgoItemsTask = GetSteamInventory(steamId,730,"2");
            var pubgItemsTask = GetSteamInventory(steamId, 578080, "2");

            await Task.WhenAll(csgoItemsTask, pubgItemsTask);

            var invs = new List<Inventory>
            {
                new Inventory
                {
                    AppId = 730,
                    Name = "Counter-Strike: Global Offensive",
                    Items = csgoItemsTask.Result
                },
                new Inventory
                {
                    AppId = 578080,
                    Name = "PlayerUnknown's Battlegrounds",
                    Items = pubgItemsTask.Result
                }
            };

            return Ok(invs);
        }
        
        private async Task<List<SteamItemWrapper>> GetSteamInventory(string steamId, int appId, string contextId)
        {
            var steamInventory = await _steamService.GetPlayerSteamInventoryAsync(steamId, appId, contextId);

            switch (steamInventory.DataCase)
            {
                case GetPlayerSteamInventoryResponse.DataOneofCase.PlayerInventory:
                    return await PairItemDescriontionWithSteamInvenotry(steamInventory.PlayerInventory.Items);
                case GetPlayerSteamInventoryResponse.DataOneofCase.Error:
                    throw new Exception(steamInventory.Error.Message);
                case GetPlayerSteamInventoryResponse.DataOneofCase.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<List<SteamItemWrapper>> PairItemDescriontionWithSteamInvenotry(RepeatedField<Item> playerInventoryItems)
        {
            var names            = playerInventoryItems.Select(item => item.MarketHashName).ToList();
            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(names);

            var list = new List<SteamItemWrapper>();

            foreach (var item in playerInventoryItems)
            {
                var didAdd = false;
                foreach (var itemDescription in itemDescriptions)
                {
                    if (item.MarketHashName != itemDescription.Name) continue;
                    list.Add(new SteamItemWrapper(item, itemDescription.Value, itemDescription.Valid));
                    didAdd = true;
                    break;
                }

                if (!didAdd)
                    list.Add(new SteamItemWrapper(item, null, false));
            }

            return list;
        }
    }
    
    public class Inventory
    {
        public int                    AppId { get; set; }
        public string                 Name  { get; set; }
        public List<SteamItemWrapper> Items { get; set; }
    }
}
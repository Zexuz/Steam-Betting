using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.WebApi.Extensions;
using Betting.WebApi.Filters;
using Betting.WebApi.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthenticationProperties = Microsoft.AspNetCore.Authentication.AuthenticationProperties;


namespace Betting.WebApi.Controllers
{
    [Interception]
    [Authorize]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IRepoServiceFactory _repoServiceFactory;
        private          IItemService        _itemService;
        private readonly IUserService        _userService;


        public AccountController(IRepoServiceFactory repoServiceFactory, IItemService itemService, IUserService userService)
        {
            _repoServiceFactory = repoServiceFactory;
            _itemService = itemService;
            _userService = userService;
        }

        [HttpGet("transaction/{id}")]
        public async Task<dynamic> HistoryTransactionDetailed(int id)
        {
            var offer = await _repoServiceFactory.OfferTranascrionRepoService.FindAsync(id);
            var user = await _repoServiceFactory.UserRepoService.FindAsync(User.GetSteamId());

            if (offer.UserId != user.Id)
                return Unauthorized();

            var partner = await _repoServiceFactory.BotRepoService.FindAsync(offer.BotId);
            var itemsInOffer = await _repoServiceFactory.ItemInOfferTransactionRepoService.FindAsync(offer);
            var itemDescriptionIds = itemsInOffer.Select(i => i.ItemDescriptionId).ToList();
            var itemDescriptions = await _repoServiceFactory.ItemDescriptionRepoService.FindAsync(itemDescriptionIds);

            var items = new List<PlayerInventory.Item>();

            foreach (var itemDescription in itemDescriptions)
            {
                foreach (var item in itemsInOffer)
                {
                    if (item.ItemDescriptionId != itemDescription.Id) continue;

                    items.Add(new PlayerInventory.Item
                    {
                        AssetId = item.AssetId,
                        DescriptionId = item.ItemDescriptionId,
                        ImgUrl = itemDescription.ImageUrl,
                        Name = itemDescription.Name,
                        Value = item.Value
                    });
                }
            }

            var transaction = new TransactionDetailed
            {
                Id = offer.Id,
                Accepted = offer.Accepted,
                IsDeposit = offer.IsDeposit,
                BotId = partner.Id,
                SteamOfferId = offer.SteamOfferId,
                TotalValue = offer.TotalValue,
                ItemCount = itemsInOffer.Count,
                Items = items
            };
            return transaction;
        }


        [ProducesResponseType(typeof(List<TransactionBasic>), 200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet("transaction")]
        public async Task<dynamic> HistoryTransactions(int limit, int? from, string type)
        {
            if (limit > 20 || limit == 0)
                return new BadRequestObjectResult("limit need to be a in range 0 < limit <=20");
            if (from < 0)
                return new BadRequestObjectResult("from need to be a positive number");
            var steamId = User.GetSteamId();
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);
            var offers = await _repoServiceFactory.OfferTranascrionRepoService.FindAsync(user, limit, from);
            var listOfTransactions = new List<TransactionBasic>();
            foreach (var offer in offers)
            {
                var partner = await _repoServiceFactory.BotRepoService.FindAsync(offer.BotId);

                var transaction = new TransactionBasic
                {
                    Id = offer.Id,
                    Accepted = offer.Accepted,
                    IsDeposit = offer.IsDeposit,
                    BotId = partner.Id,
                    SteamOfferId = offer.SteamOfferId,
                    TotalValue = offer.TotalValue,
                    ItemCount = await _repoServiceFactory.ItemInOfferTransactionRepoService.GetItemCountInOffer(offer.Id)
                };

                listOfTransactions.Add(transaction);
            }

            return listOfTransactions;
        }

        [ProducesResponseType(typeof(WinsAndLosses), 200)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet("Match")]
        public async Task<dynamic> HistoryMatch(int limit, int? from, string type)
        {
            if (limit > 20 || limit == 0)
                return new BadRequestObjectResult("limit need to be a in range 0 < limit <=20");
            if (from.HasValue && from.Value < 0)
                return new BadRequestObjectResult("from need to be a positive number");
            var user = await _repoServiceFactory.UserRepoService.FindAsync(User.GetSteamId());
            if (type == "win")
                return new WinsAndLosses
                {
                    Wins = await _userService.GetMatchesUserWon(user, limit, from),
                    Loses = new List<MatchHistory>()
                };
            return await _userService.GetMatchHistoryForUser(user, limit, from);
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            var provider = await HttpContext.GetExternalProvidersAsync();
            var steamAuth = provider.ToArray()[0].DisplayName;
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = ControllerContext.HttpContext.Request.Scheme+ "://" + ControllerContext.HttpContext.Request.Host.Host
            }, steamAuth);
        }

        [HttpGet("logout")]
        [AllowAnonymous]
        public IActionResult Logout()
        {
            return SignOut(new AuthenticationProperties
            {
                RedirectUri = "http://" + ControllerContext.HttpContext.Request.Host.Host
            }, CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpGet("steamid")]
        public async Task<dynamic> SteamId()
        {
            var steamId = User.GetSteamId();
            var user = await _repoServiceFactory.UserRepoService.FindAsync(steamId);
            var userDetailed = new UserDetailed
            {
                ImageUrl = user.ImageUrl,
                Name = user.Name,
                SteamId = user.SteamId,
                Tradelink = user.TradeLink,
                Created = user.Created,
                LastActive = user.LastActive,
                Quote = user.Quote
            };
            return userDetailed;
        }

        [HttpGet("inventory")]
        public async Task<List<Item>> Inventory()
        {
            var userSteamId = User.GetSteamId();
            var user = await _repoServiceFactory.UserRepoService.FindAsync(userSteamId);
            return await _itemService.GetAvalibleItemsForUser(user);
        }

//        /// <summary>
//        /// Set user tradelink
//        /// </summary>
//        /// <param name="tradelink">Tradelink</param>
//        /// <remarks>Sets the current users tradelink</remarks>
//        /// <response code="400">Bad request</response>
//        /// <response code="500">Internal Server Error</response>
//        [Produces("application/json")]
//        [ProducesResponseType(typeof(WinnerSelected), 200)]
//        [ProducesResponseType(typeof(WinnerSelected), 400)]
        [Produces("application/json")]
        [HttpPost("tradelink")]
        public async Task<IActionResult> SetTradelink([FromBody] TradelinkModel model)
        {
            var steamId = User.GetSteamId();
            await _repoServiceFactory.UserRepoService.UpdateTradelinkAsync(steamId, model.Tradelink);
            return Ok();
        }

        [Produces("application/json")]
        [HttpPost("quote")]
        public async Task<IActionResult> SetQuote([FromBody] QuotekModel model)
        {
            var steamId = User.GetSteamId();
            try
            {
                await _repoServiceFactory.UserRepoService.UpdateQuoteAsync(steamId, model.Quote);
            }
            catch (Exception)
            {
                return new StatusCodeResult(403);
            }

            return Ok();
        }
    }
}
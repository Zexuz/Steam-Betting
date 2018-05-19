using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Exceptions;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using RpcCommunication;
using Item = RpcCommunication.Item;

namespace Betting.Backend.Services.Impl
{
    public class OfferService : IOfferService
    {
        private readonly ITransactionFactory                _transactionFactory;
        private readonly ILogService<OfferService>          _logService;
        private readonly IItemDescriptionRepoService        _itemDescRepoServcice;
        private readonly IOfferTranascrionRepoService       _offerRepoService;
        private readonly IItemRepoService                   _itemRepoService;
        private readonly IUserRepoService                   _userRepoService;
        private readonly IBotRepoService                    _botRepoService;
        private readonly IItemInOfferTransactionRepoService _itemInOfferTransactionRepoService;
        private readonly TimeSpan                           _steamLock;

        private struct OfferData
        {
            public DatabaseModel.OfferTransaction             OfferTransactions { get; set; }
            public List<DatabaseModel.ItemInOfferTransaction> ItemsInOffer      { get; set; }
            public List<DatabaseModel.Item>                   Items             { get; set; }
        }

        public OfferService(IRepoServiceFactory factory, ITransactionFactory transactionFactory, ILogServiceFactory logServiceFactory)
        {
            _itemDescRepoServcice              = factory.ItemDescriptionRepoService;
            _botRepoService                    = factory.BotRepoService;
            _userRepoService                   = factory.UserRepoService;
            _itemRepoService                   = factory.ItemRepoService;
            _offerRepoService                  = factory.OfferTranascrionRepoService;
            _itemInOfferTransactionRepoService = factory.ItemInOfferTransactionRepoService;
            _transactionFactory                = transactionFactory;
            _logService                        = logServiceFactory.CreateLogger<OfferService>();
            _steamLock                         = new TimeSpan(7,1,0,0);
        }

        public async Task DepositSteamOfferAsync(OfferStatusRequest request)
        {
            var offer                            = await CreateOfferData(request, true);
            offer.OfferTransactions.SteamOfferId = request.OfferSend.SteamOffer.Id;

            using (var transaction = _transactionFactory.BeginTransaction())
            {
                try
                {
                    await UpdateItemsTable(transaction, offer.Items, true);
                    await InsertTransactionOfferAndItemsInTransactionOffer(transaction, offer.OfferTransactions, offer.ItemsInOffer);
                }
                catch (CantCompleteTransaction e)
                {
                    transaction.Rollback();
                    throw new CantCompleteSteamDeposit("Error, can't complete transaction and therefore the deposit", e);
                }
            }
        }

        public async Task WithdrawalSteamOffer(OfferStatusRequest request)
        {
            var offer = await CreateOfferData(request, false);
            await UpdateItemsTable(null, offer.Items, false);
            var transOffer = await _offerRepoService.FindAsync(request.OfferSend.SteamOffer.Id);
            await _offerRepoService.AddAcceptedTimesptampToOffer(DateTime.Now, transOffer.Id);
        }

        public async Task<DatabaseModel.OfferTransaction> PrepareWithdrawlSteamOffer
        (
            List<Item> itemsInOfferRequest,
            DatabaseModel.Bot bot,
            DatabaseModel.User owner
        )
        {
            var offer                        = await CreateTransactionOfferAsync(itemsInOfferRequest, bot, owner, false);
            offer.OfferTransactions.Accepted = null;
            using (var transation = _transactionFactory.BeginTransaction())
            {
                return await InsertTransactionOfferAndItemsInTransactionOffer(transation, offer.OfferTransactions, offer.ItemsInOffer);
            }
        }

        public async Task RemoveCanceledWithdrawalSteamOffer(OfferStatusRequest request)
        {
            var offerTransaction = await _offerRepoService.FindAsync(request.OfferSend.SteamOffer.Id);
            await _itemInOfferTransactionRepoService.Remove(offerTransaction.Id);
            await _offerRepoService.Remove(offerTransaction.Id);
        }

        private async Task UpdateItemsTable(ITransactionWrapper transactionWrapper, List<DatabaseModel.Item> items, bool isDeposit)
        {
            if (isDeposit)
                await _itemRepoService.InsertAsync(items, transactionWrapper);
            else
                await _itemRepoService.DeleteAsync(items);
        }

        private async Task<OfferData> CreateOfferData(OfferStatusRequest request, bool isDeposit)
        {
            var bot                 = await _botRepoService.FindAsync(request.Bot.SteamId);
            var owner               = await _userRepoService.FindAsync(request.SteamId);
            var itemsInOfferRequest = isDeposit ? request.OfferSend.SteamOffer.ItemsToReceive : request.OfferSend.SteamOffer.ItemsToGive;
            return await CreateTransactionOfferAsync(itemsInOfferRequest.ToList(), bot, owner, isDeposit);
        }

        private async Task<OfferData> CreateTransactionOfferAsync
        (
            List<Item> itemsInOfferRequest,
            DatabaseModel.Bot bot,
            DatabaseModel.User owner,
            bool isDeposit
        )
        {
            var listOfNames = itemsInOfferRequest.Select(item => item.MarketHashName).ToList();

            var itemsDesc = await _itemDescRepoServcice.FindAsync(listOfNames);

            var databaseItems = new List<DatabaseModel.Item>();
            var itemsInOffer  = new List<DatabaseModel.ItemInOfferTransaction>();

            decimal sumOfItems = 0;
            foreach (var item in itemsInOfferRequest)
            {
                DatabaseModel.ItemDescription itemDesc;

                try
                {
                    itemDesc= itemsDesc.First(i => i.Name == item.MarketHashName);
                }
                catch (System.Exception e)
                {
                    var ex  = new SteamMarketNameFuckupException(item.MarketHashName,e);
                    ex.Data.Add("marketHashName",item.MarketHashName);
                    ex.Data.Add("itemDescriptions",itemsDesc);
                    _logService.Critical(ex);
                    throw ex;
                }
                
                var steamLockTime = DateTimeOffset.Now;
                
                if (item.AppId == 730)
                    steamLockTime = steamLockTime.Add(_steamLock);

                databaseItems.Add(new DatabaseModel.Item(item.AssetId, itemDesc.Id, bot.Id, owner.Id, steamLockTime));
                sumOfItems += itemDesc.Value;

                var itemInOffer = new DatabaseModel.ItemInOfferTransaction(int.MinValue, itemDesc.Id, item.AssetId, itemDesc.Value);
                itemsInOffer.Add(itemInOffer);
            }

            var offer = new DatabaseModel.OfferTransaction(owner.Id, bot.Id, sumOfItems, isDeposit, null, DateTime.Now);
            return new OfferData
            {
                Items             = databaseItems,
                ItemsInOffer      = itemsInOffer,
                OfferTransactions = offer
            };
        }

        private async Task<DatabaseModel.OfferTransaction> InsertTransactionOfferAndItemsInTransactionOffer
        (
            ITransactionWrapper transactionWrapper,
            DatabaseModel.OfferTransaction offerTransactions,
            List<DatabaseModel.ItemInOfferTransaction> itemsInOffer
        )
        {
            var offerInsertResponse = await _offerRepoService.InsertAsync(offerTransactions, transactionWrapper);

            foreach (var itemInOfferTransaction in itemsInOffer)
            {
                itemInOfferTransaction.OfferTransactionId = offerInsertResponse.Id;
            }

            await _itemInOfferTransactionRepoService.InsertAsync(itemsInOffer, transactionWrapper);
            transactionWrapper?.Commit();
            return offerInsertResponse;
        }
    }
}
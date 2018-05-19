using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public class BotService : IBotService
    {
        private readonly IItemRepoService _itemRepoService;
        private readonly IBotRepoService  _botRepoService;

        public BotService(IRepoServiceFactory fakedRepoServiceFactory)
        {
            _itemRepoService = fakedRepoServiceFactory.ItemRepoService;
            _botRepoService  = fakedRepoServiceFactory.BotRepoService;
        }

        public async Task<Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>> GetBotsWithWithdrawItems(List<AssetAndDescriptionId> ids)
        {
            var items = await _itemRepoService.FindAsync(ids);
            return await GroupItemByBotLocation(items);
        }

        public async Task<Stack<DatabaseModel.Bot>> GetAvalibleBotsForDeposit(DatabaseModel.User user, List<DatabaseModel.Item> usersItems)
        {
            var bots = new Stack<DatabaseModel.Bot>();
            bots.Push(await GetBotWithLeastItem());

            var dict = await GroupItemByBotLocation(usersItems);

            foreach (var key in dict.Keys)
            {
                bots.Push(key);
            }

            return bots;
        }

        private async Task<DatabaseModel.Bot> GetBotWithLeastItem()
        {
            var allAvalibleBots = await _botRepoService.GetAll();
            var items = await _itemRepoService.GetAll();

            var botIdsInUse = items.Select(item => item.LocationId).Distinct().ToList();
            var allAvalibleBotIds = allAvalibleBots.Select(bot => bot.Id).ToList();

            var botsNotInUse = allAvalibleBotIds.Except(botIdsInUse).ToList();
            if (botsNotInUse.Count > 0)
            {
                var bot = await _botRepoService.FindAsync(botsNotInUse.First());
                return bot;
            }
            
            try
            {
                var botId = items.GroupBy(item => item.LocationId)
                    .OrderBy(item => item.Count())
                    .First()
                    .Select(item => item.LocationId)
                    .First();
                var bot = await _botRepoService.FindAsync(botId);
                return bot;
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                return (await _botRepoService.GetAll()).First();
            }
            catch (InvalidOperationException)
            {
                throw new NoBotInDatabaseException("No bots is in the database");
            }
        }

        private async Task<Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>> GroupItemByBotLocation(List<DatabaseModel.Item> items)
        {
            var itemGroupByLocation = items
                .GroupBy(item => item.LocationId)
                .OrderBy(g => g.Count())
                .ToList();

            var botIds = itemGroupByLocation.Select(grouping => grouping.Key).ToList();

            if (botIds.Count == 0)
                return new Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>();

            var bots = await _botRepoService.FindAsync(botIds);

            if (bots.Count == 0)
                return new Dictionary<DatabaseModel.Bot, List<DatabaseModel.Item>>();

            return itemGroupByLocation.ToDictionary(gdc => bots.First(b => b.Id == gdc.Key), gdc => gdc.ToList());
        }
    }
}
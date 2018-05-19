using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Betting.Models.Models;
using Betting.Repository.Extensions;
using Betting.Repository.Helpers;

namespace Betting.Repository.Impl
{
    public static class SqlResultParser
    {
        private static readonly Dictionary<Type, Func<SqlDataReader, Task<object>>> Actions = InitDictionary();

        public static async Task<List<T>> GetListAsync<T>(this SqlResult result)
        {
            var list = new List<T>();
            using (var reader = result.Reader)
            {
                if (!reader.HasRows) return list;

                while (await reader.ReadAsync())
                {
                    list.Add((T) await Actions[typeof(T)].Invoke(reader));
                }
            }

            return list;
        }


        public static async Task<T> GetSingleAsync<T>(this SqlResult result)
        {
            using (var reader = result.Reader)
            {
                if (!reader.HasRows) return default(T);

                await reader.ReadAsync();

                return (T) await Actions[typeof(T)].Invoke(reader);
            }
        }

        public static Dictionary<Type, Func<SqlDataReader, Task<object>>> InitDictionary()
        {
            var dict = new Dictionary<Type, Func<SqlDataReader, Task<object>>>
            {
                [typeof(DatabaseModel.User)] = async reader =>
                    new DatabaseModel.User(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.User.SteamId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.User.Name)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.User.ImageUrl)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.User.TradeLink), true),
                        await reader.ReadAsync<DateTime>(nameof(DatabaseModel.User.Created)),
                        await reader.ReadAsync<DateTime>(nameof(DatabaseModel.User.LastActive)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.User.SuspendedFromQuote)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.User.Quote), true),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.User.Id))
                    ),
                [typeof(DatabaseModel.Match)] = async reader =>
                    new DatabaseModel.Match(
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Match.RoundId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Match.Salt)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Match.Hash)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Match.Percentage)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Match.Status)),
                        await reader.ReadAsync<DateTime?>(nameof(DatabaseModel.Match.TimerStarted), true),
                        await reader.ReadAsync<int?>(nameof(DatabaseModel.Match.WinnerId), true),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Match.SettingId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Match.GameModeId)),
                        await reader.ReadAsync<DateTime>(nameof(DatabaseModel.Match.Created)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Match.Id))
                    ),
                [typeof(DatabaseModel.Item)] = async reader =>
                    new DatabaseModel.Item(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Item.AssetId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Item.DescriptionId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Item.LocationId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Item.OwnerId)),
                        await reader.ReadAsync<DateTimeOffset>(nameof(DatabaseModel.Item.ReleaseTime)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Item.Id))
                    ),
                [typeof(DatabaseModel.RakeItem)] = async reader =>
                    new DatabaseModel.RakeItem(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.RakeItem.AssetId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.RakeItem.DescriptionId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.RakeItem.LocationId)),
                        await reader.ReadAsync<DateTime>(nameof(DatabaseModel.RakeItem.Received)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.RakeItem.MatchId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.RakeItem.GameModeId)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.RakeItem.IsSold)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.RakeItem.Id))
                    ),
                [typeof(DatabaseModel.ItemDescription)] = async reader =>
                    new DatabaseModel.ItemDescription(
                        ItemDescriptionHelper.FromDatabase(await reader.ReadAsync<string>(nameof(DatabaseModel.ItemDescription.Name))),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.ItemDescription.Value)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.ItemDescription.AppId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.ItemDescription.ContextId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.ItemDescription.ImageUrl)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.ItemDescription.Valid)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemDescription.Id))
                    ),
                [typeof(DatabaseModel.Bot)] = async reader =>
                    new DatabaseModel.Bot(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Bot.SteamId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Bot.Name)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Bot.Id))
                    ),
                [typeof(DatabaseModel.Bet)] = async reader =>
                    new DatabaseModel.Bet(
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Bet.UserId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Bet.MatchId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Bet.GameModeId)),
                        await reader.ReadAsync<DateTime>(nameof(DatabaseModel.Bet.Created)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Bet.Id))
                    ),
                [typeof(DatabaseModel.ItemBetted)] = async reader =>
                    new DatabaseModel.ItemBetted(
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemBetted.BetId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemBetted.DescriptionId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.ItemBetted.AssetId)),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.ItemBetted.Value)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemBetted.Id))
                    ),
                [typeof(DatabaseModel.OfferTransaction)] = async reader =>
                    new DatabaseModel.OfferTransaction(
                        await reader.ReadAsync<int>(nameof(DatabaseModel.OfferTransaction.UserId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.OfferTransaction.BotId)),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.OfferTransaction.TotalValue)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.OfferTransaction.IsDeposit)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.OfferTransaction.SteamOfferId), true),
                        await reader.ReadAsync<DateTime?>(nameof(DatabaseModel.OfferTransaction.Accepted), true),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.OfferTransaction.Id))
                    ),
                [typeof(DatabaseModel.ItemInOfferTransaction)] = async reader =>
                    new DatabaseModel.ItemInOfferTransaction(
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemInOfferTransaction.OfferTransactionId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemInOfferTransaction.ItemDescriptionId)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.ItemInOfferTransaction.AssetId)),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.ItemInOfferTransaction.Value)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.ItemInOfferTransaction.Id))
                    ),
                [typeof(DatabaseModel.Level)] = async reader =>
                    new DatabaseModel.Level(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Level.Name)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.Level.Chat)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.Level.Ticket)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.Level.Admin)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Level.Id))
                    ),
                [typeof(DatabaseModel.Staff)] = async reader =>
                    new DatabaseModel.Staff(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.Staff.SteamId)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Staff.Level)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Staff.Id))
                    ),
                [typeof(DatabaseModel.Settings)] = async reader =>
                    new DatabaseModel.Settings(
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Settings.InventoryLimit)),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.Settings.ItemValueLimit)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Settings.SteamInventoryCacheTimerInSec)),
                        await reader.ReadAsync<DateTime>(nameof(DatabaseModel.Settings.UpdatedPricingTime)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.Settings.NrOfLatestChatMessages))
                    ),
                [typeof(DatabaseModel.GameMode)] = async reader =>
                    new DatabaseModel.GameMode(
                        await reader.ReadAsync<string>(nameof(DatabaseModel.GameMode.Type)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.GameMode.CurrentSettingId)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.GameMode.IsEnabled)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.GameMode.Id))
                    ),
                [typeof(DatabaseModel.JackpotSetting)] = async reader =>
                    new DatabaseModel.JackpotSetting(
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.JackpotSetting.Rake)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.JackpotSetting.TimmerInMilliSec)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.JackpotSetting.ItemsLimit)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.JackpotSetting.MaxItemAUserCanBet)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.JackpotSetting.MinItemAUserCanBet)),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.JackpotSetting.MaxValueAUserCanBet)),
                        await reader.ReadAsync<decimal>(nameof(DatabaseModel.JackpotSetting.MinValueAUserCanBet)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.JackpotSetting.DraftingTimeInMilliSec)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.JackpotSetting.AllowCsgo)),
                        await reader.ReadAsync<bool>(nameof(DatabaseModel.JackpotSetting.AllowPubg)),
                        await reader.ReadAsync<string>(nameof(DatabaseModel.JackpotSetting.DraftingGraph)),
                        await reader.ReadAsync<int>(nameof(DatabaseModel.JackpotSetting.Id))
                    ),
                [typeof(int)] = reader => Task.FromResult((object) reader.GetInt32(0)),
                [typeof(string)] = reader => Task.FromResult((object) reader.GetString(0))
            };
            return dict;
        }
    }
}
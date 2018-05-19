using System;
using Dapper.Contrib.Extensions;

namespace Betting.Models.Models
{
    public static class DatabaseModel
    {
        //everythhing that is NOT NULL in database should be passed as a constructor, everything else should be possible
        //to set with a property get/set method.
        public class User
        {
            public int      Id                 { get; set; }
            public string   SteamId            { get; set; }
            public string   Name               { get; set; }
            public string   ImageUrl           { get; set; }
            public string   TradeLink          { get; set; }
            public string   Quote              { get; set; }
            public DateTime Created            { get; set; }
            public DateTime LastActive         { get; set; }
            public bool     SuspendedFromQuote { get; set; }

            public User()
            {
            }

            public User(
                string steamId,
                string name,
                string imageUrl,
                string tradeLink,
                DateTime created,
                DateTime lastActive,
                bool suspendedFromQuote,
                string quote = null,
                int id = 0
            )
            {
                SteamId = steamId;
                Name = name;
                ImageUrl = imageUrl;
                TradeLink = tradeLink;
                Created = created;
                LastActive = lastActive;
                SuspendedFromQuote = suspendedFromQuote;
                Quote = quote;
                Id = id;
            }
        }

        public class Bet
        {
            public int      Id         { get; set; }
            public int      UserId     { get; set; }
            public int      MatchId    { get; set; }
            public int      GameModeId { get; set; }
            public DateTime Created    { get; set; }

            public Bet()
            {
            }

            public Bet(int userId, int matchId, int gameModeId, DateTime created, int id = 0)
            {
                Id = id;
                UserId = userId;
                MatchId = matchId;
                Created = created;
                GameModeId = gameModeId;
            }
        }

        public class Bot
        {
            public int    Id      { get; set; }
            public string SteamId { get; set; }
            public string Name    { get; set; }

            public Bot(string steamId, string name, int id = 0)
            {
                Id = id;
                SteamId = steamId;
                Name = name;
            }
        }

        public class Item
        {
            public int            Id            { get; }
            public string         AssetId       { get; }
            public int            DescriptionId { get; }
            public int            LocationId    { get; }
            public int            OwnerId       { get; }
            public DateTimeOffset ReleaseTime   { get; }

            public Item(string assetId, int descriptionId, int locationId, int ownerId, DateTimeOffset releaseTime, int id = 0)
            {
                Id = id;
                AssetId = assetId;
                DescriptionId = descriptionId;
                LocationId = locationId;
                OwnerId = ownerId;
                ReleaseTime = releaseTime;
            }
        }

        public class RakeItem
        {
            public int    Id            { get; set; }
            public string AssetId       { get; set; }
            public int    DescriptionId { get; set; }
            public int    LocationId    { get; set; }
            public int    MatchId       { get; set; }
            public bool   IsSold        { get; set; }
            public int    GameModeId    { get; set; }

            public DateTime Received { get; set; }

            public RakeItem()
            {
            }

            public RakeItem(string assetId, int descriptionId, int locationId, DateTime received, int matchId, int gameModeId, bool isSold = false,
                            int id = 0)
            {
                Id = id;
                AssetId = assetId;
                DescriptionId = descriptionId;
                LocationId = locationId;
                Received = received;
                MatchId = matchId;
                IsSold = isSold;
                GameModeId = gameModeId;
            }
        }

        public class ItemBetted
        {
            public int Id            { get; }
            public int BetId         { get; set; }
            public int DescriptionId { get; }

            public string  AssetId { get; }
            public decimal Value   { get; set; }

            public ItemBetted(int betId, int descriptionId, string assetId, decimal value, int id = 0)
            {
                Id = id;
                BetId = betId;
                DescriptionId = descriptionId;
                AssetId = assetId;
                Value = value;
            }
        }

        public class ItemDescription
        {
            public int     Id        { get; }
            public string  Name      { get; }
            public string  ImageUrl  { get; }
            public decimal Value     { get; }
            public string  AppId     { get; }
            public string  ContextId { get; }
            public bool    Valid     { get; }

            public ItemDescription(string name, decimal value, string appId, string contextId, string imageUrl, bool valid, int id = 0)
            {
                Id = id;
                Name = name;
                Value = value;
                AppId = appId;
                ContextId = contextId;
                ImageUrl = imageUrl;
                Valid = valid;
            }

            public override string ToString()
            {
                return $"Name {Name}, Value {Value}, AppId {AppId}";
            }
        }

        public class OfferTransaction
        {
            public int       Id           { get; set; }
            public int       UserId       { get; set; }
            public int       BotId        { get; set; }
            public decimal   TotalValue   { get; set; }
            public bool      IsDeposit    { get; set; }
            public string    SteamOfferId { get; set; }
            public DateTime? Accepted     { get; set; }

            public OfferTransaction(int userId, int botId, decimal totalValue, bool isDeposit, string steamOfferId, DateTime? accepted, int id = 0)
            {
                Id = id;
                UserId = userId;
                BotId = botId;
                TotalValue = totalValue;
                IsDeposit = isDeposit;
                SteamOfferId = steamOfferId;
                Accepted = accepted;
            }
        }

        public class ItemInOfferTransaction
        {
            public int     Id                 { get; set; }
            public int     OfferTransactionId { get; set; }
            public int     ItemDescriptionId  { get; set; }
            public string  AssetId            { get; set; }
            public decimal Value              { get; set; }

            public ItemInOfferTransaction(int offerTransactionId, int itemDescriptionId, string assetId, decimal value, int id = 0)
            {
                Id = id;
                OfferTransactionId = offerTransactionId;
                ItemDescriptionId = itemDescriptionId;
                Value = value;
                AssetId = assetId;
            }
        }

        [Table(nameof(Match))]
        public class Match
        {
            public int      Id         { get; set; }
            public int      RoundId    { get; set; }
            public string   Salt       { get; set; }
            public string   Hash       { get; set; }
            public string   Percentage { get; set; }
            public DateTime Created    { get; set; }
            public int      Status     { get; set; }
            public int?     WinnerId   { get; set; }
            public int      SettingId  { get; set; }
            public int      GameModeId { get; set; }

            public DateTime? TimerStarted { get; set; }

            public Match()
            {
            }

            public Match
            (
                int roundId,
                string salt,
                string hash,
                string percentage,
                int status,
                DateTime? timerStarted,
                int? winnerId,
                int settingId,
                int gameModeId,
                DateTime created,
                int id = 0)
            {
                SettingId = settingId;
                GameModeId = gameModeId;
                Id = id;
                RoundId = roundId;
                Salt = salt;
                Hash = hash;
                Created = created;
                Percentage = percentage;
                Status = status;
                TimerStarted = timerStarted;
                WinnerId = winnerId;
            }
        }

        public class Level
        {
            public string Name   { get; set; }
            public bool   Chat   { get; set; }
            public bool   Ticket { get; set; }
            public bool   Admin  { get; set; }
            public int    Id     { get; set; }

            public Level(string name, bool chat, bool ticket, bool admin, int id = 0)
            {
                Name = name;
                Chat = chat;
                Ticket = ticket;
                Admin = admin;
                Id = id;
            }
        }

        public class Staff
        {
            public int    Id      { get; set; }
            public string SteamId { get; set; }
            public int    Level   { get; set; }

            public Staff(string steamId, int level, int id = 0)
            {
                Id = id;
                SteamId = steamId;
                Level = level;
            }
        }

        [Table(nameof(JackpotSetting))]
        public class JackpotSetting
        {
            public int     Id                     { get; set; }
            public decimal Rake                   { get; set; }
            public int     TimmerInMilliSec       { get; set; }
            public int     ItemsLimit             { get; set; }
            public int     MaxItemAUserCanBet     { get; set; }
            public int     MinItemAUserCanBet     { get; set; }
            public decimal MaxValueAUserCanBet    { get; set; }
            public decimal MinValueAUserCanBet    { get; set; }
            public int     DraftingTimeInMilliSec { get; set; }
            public bool    AllowCsgo              { get; set; }
            public bool    AllowPubg              { get; set; }
            public string  DraftingGraph          { get; set; }

            public JackpotSetting()
            {
            }

            public JackpotSetting
            (
                decimal rake,
                int timmerInMilliSec,
                int itemsLimit,
                int maxItemAUserCanBet,
                int minItemAUserCanBet,
                decimal maxValueAUserCanBet,
                decimal minValueAUserCanBet,
                int draftingTimeInMs,
                bool allowCsgo,
                bool allowPubg,
                string draftingGraph,
                int id = 0
            )
            {
                TimmerInMilliSec = timmerInMilliSec;
                ItemsLimit = itemsLimit;
                MaxItemAUserCanBet = maxItemAUserCanBet;
                MinItemAUserCanBet = minItemAUserCanBet;
                MaxValueAUserCanBet = maxValueAUserCanBet;
                MinValueAUserCanBet = minValueAUserCanBet;
                DraftingTimeInMilliSec = draftingTimeInMs;
                DraftingGraph = draftingGraph;
                AllowCsgo = allowCsgo;
                AllowPubg = allowPubg;
                Rake = rake;
                Id = id;
            }
        }

        [Table(nameof(GameMode))]
        public class GameMode : Entity
        {
            public string Type             { get; set; }
            public int    CurrentSettingId { get; set; }
            public bool   IsEnabled        { get; set; }

            public GameMode()
            {
            }

            public GameMode(string type, int currentSettingId, bool isEnabled = true, int id = 0)
            {
                Id = id;
                Type = type;
                IsEnabled = isEnabled;
                CurrentSettingId = currentSettingId;
            }
        }

        public class Settings
        {
            public int      InventoryLimit                { get; set; }
            public decimal  ItemValueLimit                { get; set; }
            public int      SteamInventoryCacheTimerInSec { get; set; }
            public DateTime UpdatedPricingTime            { get; set; }
            public int      NrOfLatestChatMessages        { get; set; }

            public Settings
            (
                int inventoryLimit,
                decimal itemValueLimit,
                int steamInventoryCacheTimerInSec,
                DateTime updatedPricingTime,
                int nrOfLatestChatMessages
            )
            {
                InventoryLimit = inventoryLimit;
                ItemValueLimit = itemValueLimit;
                SteamInventoryCacheTimerInSec = steamInventoryCacheTimerInSec;
                UpdatedPricingTime = updatedPricingTime;
                NrOfLatestChatMessages = nrOfLatestChatMessages;
            }
        }

        [Table(nameof(CoinFlip))]
        public class CoinFlip : Entity
        {
            public string RoundId       { get; set; }
            public string Salt          { get; set; }
            public string Hash          { get; set; }
            public string Percentage    { get; set; }
            public int    Status        { get; set; }
            public int?   WinnerId      { get; set; }
            public int    CreatorUserId { get; set; }

            public bool      CreatorIsHead { get; set; }
            public int       SettingId     { get; set; }
            public int       GameModeId    { get; set; }
            public DateTime? TimerStarted  { get; set; }
            public DateTime  Created       { get; set; }
        }

        public abstract class Entity
        {
            public int Id { get; set; }

            [Computed]
            public bool IsNew => Id == default(int);
        }
    }
}
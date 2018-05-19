using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Factories
{
    public interface IRepoServiceFactory
    {
        IBetRepoService                    BetRepoService                    { get; }
        IItemDescriptionRepoService        ItemDescriptionRepoService        { get; }
        IItemBettedRepoService             ItemBettedRepoService             { get; }
        IUserRepoService                   UserRepoService                   { get; }
        IMatchRepoService                  MatchRepoService                  { get; }
        IItemRepoService                   ItemRepoService                   { get; }
        IBotRepoService                    BotRepoService                    { get; }
        IItemInOfferTransactionRepoService ItemInOfferTransactionRepoService { get; }
        IOfferTranascrionRepoService       OfferTranascrionRepoService       { get; }
        ISettingRepoService                SettingRepoService                { get; }
        IRakeItemRepoService               RakeItemRepoService               { get; }
        IStaffRepoService                  StaffRepoService                  { get; }
        ILevelRepoService                  LevelRepoService                  { get; }
        IGameModeRepoService               GameModeRepoService               { get; }
        IJackpotSettingRepo                JackpotSettingRepo                { get; }
        ICoinFlipMatchRepoService          CoinFlipMatchRepoService          { get; }
    }
}
using Betting.Repository.Interfaces;

namespace Betting.Repository.Factories
{
    public interface IQueryFactory
    {
        IBetQueries                    BetQueries                    { get; }
        IBotQueries                    BotQueries                    { get; }
        IItemQueries                   ItemQueries                   { get; }
        IItemDescriptionQueries        ItemDescriptonQueries         { get; }
        IItemBetQueries                ItemBetQueries                { get; }
        IMatchQueries                  MatchQueries                  { get; }
        IUserQueries                   UserQueries                   { get; }
        IItemInOfferTransactionQueries ItemInOfferTransactionQueries { get; }
        IOfferTransationQueries        OfferTransationQueries        { get; }
        IRakeItemQueries               RakeItemQueries               { get; }
        IStaffQueries                  StaffQueries                  { get; }
        ILevelQueries                  LevelQueries                  { get; }
    }
}
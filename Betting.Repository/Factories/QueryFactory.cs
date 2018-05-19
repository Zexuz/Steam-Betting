using Betting.Repository.Impl;
using Betting.Repository.Interfaces;

namespace Betting.Repository.Factories
{
    public class QueryFactory : IQueryFactory
    {
        public IBetQueries                    BetQueries                    => new BetQueries();
        public IBotQueries                    BotQueries                    => new BotQueries();
        public IItemQueries                   ItemQueries                   => new ItemQueries();
        public IItemDescriptionQueries        ItemDescriptonQueries         => new ItemDescriptionQueries();
        public IItemBetQueries                ItemBetQueries                => new ItemBetQueries();
        public IMatchQueries                  MatchQueries                  => new MatchQueries();
        public IUserQueries                   UserQueries                   => new UserQueries();
        public IItemInOfferTransactionQueries ItemInOfferTransactionQueries => new ItemInOfferTransactionQueries();
        public IOfferTransationQueries        OfferTransationQueries        => new OfferTransationQueries();
        public IRakeItemQueries               RakeItemQueries               => new RakeItemQueries();
        public IStaffQueries                  StaffQueries                  => new StaffQueries();
        public ILevelQueries                  LevelQueries                  => new LevelQueries();
    }
}
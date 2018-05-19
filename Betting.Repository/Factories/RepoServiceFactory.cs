using Betting.Repository.Services.Interfaces;

namespace Betting.Repository.Factories
{
    public class RepoServiceFactory : IRepoServiceFactory
    {
        public IBetRepoService                    BetRepoService                    { get; }
        public IItemDescriptionRepoService        ItemDescriptionRepoService        { get; }
        public IItemBettedRepoService             ItemBettedRepoService             { get; }
        public IUserRepoService                   UserRepoService                   { get; }
        public IMatchRepoService                  MatchRepoService                  { get; }
        public IItemRepoService                   ItemRepoService                   { get; }
        public IBotRepoService                    BotRepoService                    { get; }
        public IItemInOfferTransactionRepoService ItemInOfferTransactionRepoService { get; }
        public IOfferTranascrionRepoService       OfferTranascrionRepoService       { get; }
        public ISettingRepoService                SettingRepoService                { get; }
        public IRakeItemRepoService               RakeItemRepoService               { get; }
        public IStaffRepoService                  StaffRepoService                  { get; }
        public ILevelRepoService                  LevelRepoService                  { get; }
        public IGameModeRepoService               GameModeRepoService               { get; }
        public IJackpotSettingRepo                JackpotSettingRepo                { get; }
        public ICoinFlipMatchRepoService          CoinFlipMatchRepoService          { get; }

        public RepoServiceFactory
        (
            IBetRepoService betRepoService,
            IItemDescriptionRepoService descriptionRepoService,
            IItemBettedRepoService itemBettedRepoService,
            IUserRepoService userRepoService,
            IMatchRepoService matchRepoService,
            IItemRepoService itemRepoService,
            IBotRepoService botRepoService,
            IItemInOfferTransactionRepoService inOfferTransactionRepoService,
            IOfferTranascrionRepoService offerTranascrionRepoService,
            ISettingRepoService settingRepoService,
            IRakeItemRepoService rakeItemRepoService,
            IStaffRepoService staffRepoService,
            ILevelRepoService levelRepoService,
            IGameModeRepoService gameModeRepoService,
            IJackpotSettingRepo jackpotSettingRepo,
            ICoinFlipMatchRepoService coinFlipMatchRepoService)
        {
            ItemInOfferTransactionRepoService = inOfferTransactionRepoService;
            OfferTranascrionRepoService = offerTranascrionRepoService;
            SettingRepoService = settingRepoService;
            RakeItemRepoService = rakeItemRepoService;
            StaffRepoService = staffRepoService;
            LevelRepoService = levelRepoService;
            GameModeRepoService = gameModeRepoService;
            JackpotSettingRepo = jackpotSettingRepo;
            CoinFlipMatchRepoService = coinFlipMatchRepoService;
            BetRepoService = betRepoService;
            ItemDescriptionRepoService = descriptionRepoService;
            ItemBettedRepoService = itemBettedRepoService;
            UserRepoService = userRepoService;
            MatchRepoService = matchRepoService;
            ItemRepoService = itemRepoService;
            BotRepoService = botRepoService;
        }
    }
}
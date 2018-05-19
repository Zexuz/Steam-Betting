namespace Discord.Backend.Enum
{
    public enum Event
    {
        GlobalExceptionError = 0,
        CoinFlipCreate       = 1,
        CoinFlipJoin         = 2,
        ChatMessage          = 3,
        JackpotUserBetted    = 4,
        UserLoginFirstTime   = 5,
        JackpotWinner        = 6,
        CoinFlipWinner       = 7,
        TicketCrated         = 8,
    }
}
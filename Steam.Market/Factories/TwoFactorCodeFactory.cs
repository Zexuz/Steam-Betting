namespace Steam.Market.Factories
{
    internal static class TwoFactorCodeFactory
    {
        internal static string Create(string sharedSecret, long timestamp)
        {
            var twoFactorGenerator = new SteamTwoFactorGenerator
            {
                SharedSecret = sharedSecret
            };
            return twoFactorGenerator.GenerateSteamGuardCodeForTime(timestamp);
        }
    }
}
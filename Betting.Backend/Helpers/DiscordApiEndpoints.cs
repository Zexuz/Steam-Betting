namespace Betting.Backend.Helpers
{
    public static class DiscordApiEndpoints
    {
        public static string Connections => "https://discordapp.com/api/users/@me/connections";
        public static string Profile     => "https://discordapp.com/api/users/@me";
        public static string BaseUrl     => "https://discordapp.com/";
        public static string Token       => "https://discordapp.com/api/oauth2/token";
    }
}
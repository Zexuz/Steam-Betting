using Newtonsoft.Json;

namespace Steam.Market.Models.Resources
{
    internal class TransferParameters
    {
        [JsonProperty(PropertyName = "auth")]
        internal string Auth { get; set; }

        [JsonProperty(PropertyName = "remember_login")]
        internal bool RememberLogin { get; set; }

        [JsonProperty(PropertyName = "steamid")]
        internal string SteamId { get; set; }

        [JsonProperty(PropertyName = "token")]
        internal string Token { get; set; }

        [JsonProperty(PropertyName = "token_secure")]
        internal string TokenSecure { get; set; }

        [JsonProperty(PropertyName = "webcookie")]
        internal string WebCookie { get; set; }
    }
}
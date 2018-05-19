using Newtonsoft.Json;
using Shared.Shared.Web;

namespace Betting.Backend.Resources
{
    public class DiscordProfileResource: Parser<DiscordProfileResource>
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("mfa_enabled")]
        public bool MfaEnabled { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("discriminator")]
        public string Discriminator { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
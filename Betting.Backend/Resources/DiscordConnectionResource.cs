using Newtonsoft.Json;
using Shared.Shared.Web;

namespace Betting.Backend.Resources
{
    public class DiscordConnectionResource: Parser<DiscordConnectionResource>
    {
        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("show_activity")]
        public bool ShowActivity { get; set; }

        [JsonProperty("friend_sync")]
        public bool FriendSync { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("visibility")]
        public long Visibility { get; set; }
    }
}
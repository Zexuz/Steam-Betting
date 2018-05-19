using Newtonsoft.Json;

namespace Steam.Market.Models.Resources
{
    internal class GetRsaKeyResource
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("publickey_mod")]
        public string PublicKeyMod { get; set; }

        [JsonProperty("publickey_exp")]
        public string PublicKeyExp { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("token_gid")]
        public string TokenGid { get; set; }
    }
}
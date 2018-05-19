using Newtonsoft.Json;

namespace Steam.Market.Models.Resources
{
    internal class OffsetResource
    {
        [JsonProperty("response")]
        internal OffsetParameters OffsetParameters { get; set; }
    }
}
using Newtonsoft.Json;

namespace Steam.Market.Models.Resources
{
    internal class OffsetParameters
    {
        [JsonProperty("server_time")]
        internal string ServerTime { get; set; }

        [JsonProperty("skew_tolerance_seconds")]
        internal string SkewToleranceSeconds { get; set; }

        [JsonProperty("large_time_jink")]
        internal string LargeTimeJink { get; set; }

        [JsonProperty("probe_frequency_seconds")]
        internal int ProbeFrequencySeconds { get; set; }

        [JsonProperty("adjusted_time_probe_frequency_seconds")]
        internal int AdjustedTimeProbeFrequencySeconds { get; set; }

        [JsonProperty("hint_probe_frequency_seconds")]
        internal int HintProbeFrequencySeconds { get; set; }

        [JsonProperty("sync_timeout")]
        internal int SyncTimeout { get; set; }

        [JsonProperty("try_again_seconds")]
        internal int TryAgainSeconds { get; set; }

        [JsonProperty("max_attempts")]
        internal int MaxAttempts { get; set; }
    }
}
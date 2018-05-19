using System.Collections.Generic;
using Newtonsoft.Json;

namespace Steam.Market.Models.Resources
{
    internal class DoLoginResource
    {
        [JsonProperty(PropertyName = "success")]
        internal bool Success { get; set; }

        [JsonProperty(PropertyName = "requires_twofactor")]
        internal bool RequiresTwoFactor { get; set; }

        [JsonProperty(PropertyName = "login_complete")]
        internal bool LoginComplete { get; set; }

        [JsonProperty(PropertyName = "transfer_urls")]
        internal List<string> TransferUrls { get; set; }

        [JsonProperty(PropertyName = "transfer_parameters")]
        internal TransferParameters TransferParameters { get; set; }

        [JsonProperty(PropertyName = "clear_password_field")]
        internal bool ClearPasswordField { get; set; }

        [JsonProperty(PropertyName = "captcha_needed")]
        internal bool CaptchaNeeded { get; set; }

        [JsonProperty(PropertyName = "captcha_gid")]
        internal long CaptchaGid { get; set; }

        [JsonProperty(PropertyName = "message")]
        internal string Message { get; set; }
    }
}
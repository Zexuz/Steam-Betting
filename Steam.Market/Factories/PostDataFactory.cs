using System;
using System.Collections.Generic;
using Steam.Market.Models.Resources;

namespace Steam.Market.Factories
{
    internal static class PostDataFactory
    {
        internal static Dictionary<string, string> CreateGetRsaKeyData(string username)
        {
            var content = new Dictionary<string, string>
            {
                {"username", username},
                {"donotcache", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()}
            };
            return content;
        }

        internal static Dictionary<string, string> CreateTransferData(DoLoginResource doLoginResponse)
        {
            var content = new Dictionary<string, string>
            {
                {"auth", doLoginResponse.TransferParameters.Auth},
                {"rememberlogin", doLoginResponse.TransferParameters.RememberLogin.ToString()},
                {"steamid", doLoginResponse.TransferParameters.SteamId},
                {"token", doLoginResponse.TransferParameters.Token},
                {"tokensecure", doLoginResponse.TransferParameters.TokenSecure},
            };
            return content;
        }

        internal static Dictionary<string, string> CreateDoLoginData(string username, string encryptedPassword, string rsaTimestamp,
                                                                     string twoFactorCode)
        {
            var content = new Dictionary<string, string>
            {
                {"username", username},
                {"password", encryptedPassword},
                {"captchagid", "-1"},
                {"captchatext", ""},
                {"rememberlogin", "true"},
                {"loginfriendlyname", ""},
                {"emailauth", ""},
                {"emailsteamid", ""},
                {"rsatimestamp", Uri.EscapeDataString(rsaTimestamp)},
                {"donotcache", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()},
                {"twofactorcode", twoFactorCode}
            };
            return content;
        }
    }
}
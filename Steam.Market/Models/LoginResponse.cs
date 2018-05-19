using System.Net;

namespace Steam.Market.Models
{
    public class LoginResponse
    {
        public string          SessionId             { get; set; }
        public string          SteamCountry          { get; set; }
        public string          SteamLogin            { get; set; }
        public string          SteamRememberLogin    { get; set; }
        public string          SteamLanguage         { get; set; }
        public string          SteamLoginSecure      { get; set; }
        public string          SteamMachineAuthvalue { get; set; }
        public long            SteamCommunityId      { get; set; }
        public CookieContainer LoginCookies          { get; set; }
        public LoginError      Error                 { get; set; }
    }
}
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Steam.Market.Models;

namespace Steam.Market.Factories
{
    internal static class LoginResponseFactory
    {
        internal static LoginResponse Create(CookieContainer cookieContainer)
        {
            var loginResponse = new LoginResponse();

            var responseCookies = cookieContainer.GetCookies(new Uri("https://steamcommunity.com")).Cast<Cookie>().ToList();
          
            loginResponse.SteamCountry = responseCookies.FirstOrDefault(e => e.Name == "steamCountry").Value;
            loginResponse.SteamLogin = responseCookies.FirstOrDefault(e => e.Name == "steamLogin").Value;
//          loginResponse.SteamRememberLogin = responseCookies.FirstOrDefault(e => e.Name == "steamRememberLogin").Value;
//          loginResponse.SteamLanguage = responseCookies.FirstOrDefault(e => e.Name == "Steam_Language").Value;
            loginResponse.SessionId = responseCookies.FirstOrDefault(e => e.Name == "sessionid").Value;
            loginResponse.SteamLoginSecure = responseCookies.FirstOrDefault(e => e.Name == "steamLoginSecure").Value;
            loginResponse.SteamMachineAuthvalue = responseCookies.FirstOrDefault(e => e.Name.Contains("steamMachineAuth")).Value;
            loginResponse.LoginCookies = cookieContainer;
            
            var steam64Id = long.Parse(Regex.Split(Regex.Split(loginResponse.SteamLogin, "=")[0], "%")[0]);
            loginResponse.SteamCommunityId = steam64Id;
            
            return loginResponse;
        }
    }
}
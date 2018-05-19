using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Betting.Backend.Helpers;
using Betting.Backend.Resources;
using Betting.Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Shared.Shared.Web;

namespace Betting.Backend.Services.Impl
{
    public class DiscordAuthAuthService : IDiscordAuthService
    {
        public string RedirectUrl { get; }
        public string AuthUrl     { get; }

        private string ClientId     { get; }
        private string ClientSecret { get; }

        public DiscordAuthAuthService(IConfiguration configuration)
        {
            var discordConfig = configuration.GetSection("Discord");

            ClientId = discordConfig.GetSection("ClientId").Value;
            ClientSecret = discordConfig.GetSection("ClientSecret").Value;
            RedirectUrl = discordConfig.GetSection("RedirectUri").Value;

            var baseUrl = DiscordApiEndpoints.BaseUrl;
            AuthUrl = $"{baseUrl}oauth2/authorize?response_type=code&client_id={ClientId}&redirect_uri={RedirectUrl}&scope=identify";
        }


        public async Task<DiscordConnectionResource[]> GetConnections(string accessToken)
        {
            return await MakeGetAsync<DiscordConnectionResource[]>(accessToken, DiscordApiEndpoints.Connections);
        }

        public async Task<DiscordProfileResource> GetProfileInfo(string accessToken)
        {
            return await MakeGetAsync<DiscordProfileResource>(accessToken, DiscordApiEndpoints.Profile);
        }

        public bool IsValid(DiscordConnectionResource connectionResource, string steamId)
        {
            return connectionResource.Verified && connectionResource.Id == steamId;
        }

        public DiscordConnectionResource GetSteamConnection(DiscordConnectionResource[] connectionResource)
        {
            return connectionResource.FirstOrDefault(a => a.Type == "steam");
        }

        public async Task<DiscordAuthResource> GetDiscordAuthResource(string code, string encodedUrl)
        {
            var url = DiscordApiEndpoints.Token + $"?grant_type=authorization_code&code={code}&redirect_uri={encodedUrl}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Basic " + Base64Encode($"{ClientId}:{ClientSecret}"));
            var res = await new HttpClient().SendAsync(request);
            var resString = await res.Content.ReadAsStringAsync();

            var resObj = DiscordAuthResource.FromJson(resString);
            return resObj;
        }

        //todo disabled this for easier integration to discord (but at the cost of securite/discord validation.)
//        public async Task<DiscordConnectionResource> GetValidSteamIdFromDiscord(string code, string steamId)
//        {
//            var resObj = await GetDiscordAuthResource(code, WebUtility.UrlEncode(RedirectUrl));
//            var connections = await GetConnections(resObj.AccessToken);
//
//            var steam = GetSteamConnection(connections);
////            if (steam == null || IsValid(steam, steamId)) 
//            if (steam == null)
//            {
//                throw new Exception("This account is not verified on discord or the steamId does not match");     
//            }
//
//            return steam;
//        }

        private async Task<T> MakeGetAsync<T>(string accessToken, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Authorization = AuthenticationHeaderValue.Parse($"Bearer {accessToken}");
            var res = await new HttpClient().SendAsync(request);
            var resString = await res.Content.ReadAsStringAsync();

            var resObj = Parser<T>.FromJson(resString);
            return resObj;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
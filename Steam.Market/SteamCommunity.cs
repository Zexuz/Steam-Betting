using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Shared.Shared.Web;
using Steam.Market.Factories;
using Steam.Market.Helper;
using Steam.Market.Models;
using Steam.Market.Models.Resources;

namespace Steam.Market
{
    public class SteamCommunityManager : ISteamCommunityManager
    {
        public bool UserIsLoggedIn { get; private set; }
        
        private readonly IJsonRequestParser _jsonRequestParser;
        private readonly string             _baseUrl = "https://steamcommunity.com/";
        private          User               _user;
        private          LoginResponse      _loginResponse;

        public SteamCommunityManager(IJsonRequestParser jsonRequestParser, IConfiguration configuration)
        {
            _jsonRequestParser = jsonRequestParser;

            var steamcommunitySection = configuration.GetSection("SteamCommunity");

            _user = new User
            {
                Username = steamcommunitySection.GetSection("Username").Value,
                Password = steamcommunitySection.GetSection("Password").Value,
                SharedSecret = steamcommunitySection.GetSection("SharedSecret").Value
            };
        }


        public async Task<PriceHistoryResource> GetPriceHistoryResource(int appId, string marketHashName)
        {
            var url = $"market/pricehistory/?country=US&currency=3&appid={appId}&market_hash_name={marketHashName}";

            var headers = new Dictionary<string, List<string>>
            {
                {"Cookie", new List<string> {_loginResponse.SteamLogin}}
            };
            
            var requestMessage = new RequestMessage(HttpMethod.Get, _baseUrl + url, headers);

            var jsonObject = await _jsonRequestParser.ExecuteAsJObject(requestMessage);

            var priceHistory = new PriceHistoryResource
            {
                Success = (bool) jsonObject["success"],
            };

            priceHistory.Prices = jsonObject["prices"].Select(json => new PriceHistoryResource.Price
            {
                Time = DateTimeParser.ConvertSteamTime(json[0].Value<string>()),
                MedianPrice = json[1].Value<double>(),
                AmountSold = json[2].Value<int>(),
            }).ToList();

            return priceHistory;
        }

        public async Task<bool> Login()
        {
            var twoFactorCode = await GetTwoFactorCode(_user.SharedSecret);

            var getRsaKeyResource = await GetRsaKey(_user.Username);
            var doLoginResource = await DoLogin(getRsaKeyResource, _user.Password, _user.Username, twoFactorCode);

            await Transfer(doLoginResource);
            _loginResponse = await SetSession();

            if (_loginResponse.Error == null)
                UserIsLoggedIn = true;

            return _loginResponse.Error == null;
        }

        private async Task Transfer(DoLoginResource doLoginResource)
        {
            var content = PostDataFactory.CreateTransferData(doLoginResource);
            var requestMessage = new RequestMessage(HttpMethod.Post, _baseUrl + "login/transfer", body: content);
            await _jsonRequestParser.ExecuteAsVoid(requestMessage);
        }

        private async Task<LoginResponse> SetSession()
        {
            var requestMessage = new RequestMessage(HttpMethod.Get, _baseUrl + "my/home");
            await _jsonRequestParser.ExecuteAsVoid(requestMessage);
            return LoginResponseFactory.Create(_jsonRequestParser.CookieContainer);
        }

        private async Task<DoLoginResource> DoLogin(GetRsaKeyResource getRsaKeyResource, string password, string username, string twoFactorCode)
        {
            var encryptedPassword = Encrypter.Create(getRsaKeyResource, password);

            var content = PostDataFactory.CreateDoLoginData(username, encryptedPassword, getRsaKeyResource.Timestamp, twoFactorCode);

            var requestMessage = new RequestMessage(HttpMethod.Post, _baseUrl + "login/dologin", body: content);
            var doLoginResource = await _jsonRequestParser.ExecuteAsType<DoLoginResource>(requestMessage);
            return doLoginResource;
        }

        private async Task<string> GetTwoFactorCode(string sharedSecret)
        {
            var timestamp = long.Parse((await GetSteamOffset()).OffsetParameters.ServerTime);
            var twoFaCode = TwoFactorCodeFactory.Create(sharedSecret, timestamp);
            return twoFaCode;
        }

        private async Task<OffsetResource> GetSteamOffset()
        {
            var requestMessage = new RequestMessage(HttpMethod.Post, "http://api.steampowered.com/ITwoFactorService/QueryTime/v1/");
            return await _jsonRequestParser.ExecuteAsType<OffsetResource>(requestMessage);
        }

        private async Task<GetRsaKeyResource> GetRsaKey(string userName)
        {
            var body = new Dictionary<string, string>
            {
                {"donotcache", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()},
                {"username", userName},
            };

            var headers = new Dictionary<string, List<string>>();
            headers.Add("Origin", new List<string> {_baseUrl});
            headers.Add("User-Agent", new List<string>
            {
                "Mozilla /5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.181 Safari/537.36"
            });

            var requestMessage = new RequestMessage(HttpMethod.Post, _baseUrl + "login/getrsakey", headers, body);

            var rsaKeyResource = await _jsonRequestParser.ExecuteAsType<GetRsaKeyResource>(requestMessage);

            return rsaKeyResource;
        }

        struct User
        {
            public string Username     { get; set; }
            public string Password     { get; set; }
            public string SharedSecret { get; set; }
        }
    }
}
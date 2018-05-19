using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using Newtonsoft.Json;
using Shared.Shared.Web;

namespace Betting.Backend.Factories
{
    public class CsgoFastApi : IPricingService
    {
        private readonly IRepoServiceFactory        _repoServiceFactory;
        private readonly IHttpRequestService        _httpRequestService;
        private readonly ISettingRepoService        _settingRepoService;
        private readonly ISteamMarketScraperService _steamMarketScraperService;
        private readonly IJsonRequestParser         _jsonRequestParser;
        private readonly ILogService<CsgoFastApi>   _logger;
        private readonly string                     _apiKey;

        public CsgoFastApi
        (
            string steamLyticsApiKey,
            IRepoServiceFactory repoServiceFactory,
            ILogServiceFactory logServiceFactory,
            IHttpRequestService httpRequestService,
            ISettingRepoService settingRepoService,
            ISteamMarketScraperService steamMarketScraperService,
            IJsonRequestParser jsonRequestParser
        )
        {
            _apiKey = steamLyticsApiKey;
            _repoServiceFactory = repoServiceFactory;
            _logger = logServiceFactory.CreateLogger<CsgoFastApi>();
            _httpRequestService = httpRequestService;
            _settingRepoService = settingRepoService;
            _steamMarketScraperService = steamMarketScraperService;
            _jsonRequestParser = jsonRequestParser;
        }

        public async Task UpdatePricingForCsgoAsync()
        {
            var appId = 730;

            await UpdatePriceing("https://api.csgofast.com/price/all",appId);
            await GetNameAndImagesForCsgo();
            await RemoveBadItems();
            await UpdateSettingTable();
        }

        public async Task UpdatePricingForPubgAsync()
        {
            var appId = 578080;
            await UpdatePriceing("https://api.dapubg.com/price/all",appId);
            await GetNameAndImagesForPubg();
            await RemoveBadItems();
            await UpdateSettingTable();
        }

        public async Task UpdatePricingForAll()
        {
            await UpdatePricingForCsgoAsync();
            await UpdatePricingForPubgAsync();
        }


        private async Task RemoveBadItems()
        {
            await _repoServiceFactory.ItemDescriptionRepoService.RemoveItemsWithNoImage();
        }

        private async Task UpdateSettingTable()
        {
            var setting = await _settingRepoService.GetSettingsAsync();
            setting.UpdatedPricingTime = DateTime.Now;
            await _settingRepoService.SetSettingsAsync(setting);
        }

        private async Task UpdatePriceing(string url, int appId)
        {
            await _repoServiceFactory.ItemDescriptionRepoService.InvalidateItemForAppId(appId);
            var jObject = await _jsonRequestParser.ExecuteAsJObject(new RequestMessage(HttpMethod.Get, url));

            using (var cureser = jObject.GetEnumerator())
            {
                var index = 0;

                while (cureser.MoveNext())
                {
                    index++;
                    var name = cureser.Current.Key.ToString();
                    var price = decimal.Parse(cureser.Current.Value.ToString());

                    var itemDesc = new DatabaseModel.ItemDescription(name, Math.Round(price, 2), appId.ToString(), "2", "noImg",true);
                    if (index % 50 == 0)
                        _logger.Info($"Pricing Status {appId} {index + 1}/{jObject.Count}");
                    await _repoServiceFactory.ItemDescriptionRepoService.InsertOrUpdate(itemDesc);
                }
            }
        }

        private async Task GetNameAndImagesForCsgo()
        {
            var imageResponse = await _httpRequestService.MakeRequest($"http://api.csgo.steamlytics.xyz/v1/items?key={_apiKey}");

            var response = CsgoSteamlyticsResource.FromJson(imageResponse);
            var listCount = response.NumItems;
            var list = response.Items;
            var regex = new Regex("\\/\\/steamcommunity-a.akamaihd.net\\/economy\\/image\\/(.+)");

            var regExService = new RegexService();
            for (var index = 0; index < listCount; index++)
            {
                var item = list[index];
                var name = item.MarketHashName;
                if (string.IsNullOrEmpty(item.IconUrl)) continue;

                string regExMatch;
                try
                {
                    regExMatch = regExService.GetFirstGroupMatch(regex, item.IconUrl);
                }
                catch (Exception e)
                {
                    e.Data.Add("name", name);
                    throw;
                }

                if (index % 50 == 0)
                    _logger.Info($"Image Status CSGO {index + 1}/{listCount}");

                var imgUrl = regExMatch;
                await _repoServiceFactory.ItemDescriptionRepoService.UpdateImg(name, imgUrl);
            }
        }

        private async Task GetNameAndImagesForPubg()
        {
            var nameAndImages = await _steamMarketScraperService.Scrape(578080);

            var regex = new Regex("/economy\\/image\\/([^/]*)");

            var regExService = new RegexService();
            for (var index = 0; index < nameAndImages.Count; index++)
            {
                var item = nameAndImages[index];
                var name = item.Name;
                if (string.IsNullOrEmpty(item.ImageSrc)) continue;

                string regExMatch;
                try
                {
                    regExMatch = regExService.GetFirstGroupMatch(regex, item.ImageSrc);
                }
                catch (Exception e)
                {
                    e.Data.Add("name", name);
                    throw;
                }

                if (index % 50 == 0)
                    _logger.Info($"Image Status PUBG {index + 1}/{nameAndImages.Count}");

                var imgUrl = regExMatch;
                await _repoServiceFactory.ItemDescriptionRepoService.UpdateImg(name, imgUrl);
            }
        }

        private class OpSkinsResponse
        {
            [JsonProperty("op_7_day")]
            public decimal Op7Days { get; set; }

            [JsonProperty("op_30_day")]
            public decimal Op30Days { get; set; }
        }
    }
}
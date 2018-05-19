using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Betting.Backend.Exceptions;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Impl;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Betting.Backend.Factories
{
    public class OpSkinsApi : IPricingService
    {
        private readonly IRepoServiceFactory        _repoServiceFactory;
        private readonly IHttpRequestService        _httpRequestService;
        private readonly ISettingRepoService        _settingRepoService;
        private readonly ISteamMarketScraperService _steamMarketScraperService;
        private readonly ILogService<OpSkinsApi>    _logger;
        private readonly string                     _apiKey;

        public OpSkinsApi
        (
            string steamLyticsApiKey,
            IRepoServiceFactory repoServiceFactory,
            ILogServiceFactory logServiceFactory,
            IHttpRequestService httpRequestService,
            ISettingRepoService settingRepoService,
            ISteamMarketScraperService steamMarketScraperService
        )
        {
            _apiKey = steamLyticsApiKey;
            _repoServiceFactory = repoServiceFactory;
            _logger = logServiceFactory.CreateLogger<OpSkinsApi>();
            _httpRequestService = httpRequestService;
            _settingRepoService = settingRepoService;
            _steamMarketScraperService = steamMarketScraperService;
        }

        public async Task UpdatePricingForCsgoAsync()
        {
            var appId = 730;

            await UpdatePriceing(appId);
            await GetNameAndImagesForCsgo();
            await RemoveBadItems();
            await UpdateSettingTable();
        }

        public async Task UpdatePricingForPubgAsync()
        {
            var appId = 578080;

            await UpdatePriceing(appId);
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

        private async Task UpdatePriceing(int appId)
        {
            var pricingResponse = await _httpRequestService.MakeRequest($"https://api.opskins.com/IPricing/GetSuggestedPrices/v2/?appid={appId}");
            var obj = JObject.Parse(pricingResponse);

            var status = obj["status"];
            var response = obj["response"];

            if (status == null || status.Value<long>() != 1)
                throw new BadResponseException($"Statuscode is {status.Value<long>()}");

            var parsedResponse = response.ToObject<Dictionary<string, OpSkinsResponse>>();

            var index = 0;
            foreach (var opSkinsResponse in parsedResponse)
            {
                index++;
                var name = opSkinsResponse.Key;
                var op30 = opSkinsResponse.Value.Op30Days;
                var op7 = opSkinsResponse.Value.Op7Days;

                var lowestAvg = op30 > op7 ? op7 : op30;

                var itemDesc = new DatabaseModel.ItemDescription(name, Math.Round(lowestAvg / 100, 2), appId.ToString(), "2", "noImg",true);
                if (index % 50 == 0)
                    _logger.Info($"Pricing Status {appId} {index + 1}/{parsedResponse.Count}");
                await _repoServiceFactory.ItemDescriptionRepoService.InsertOrUpdate(itemDesc);
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
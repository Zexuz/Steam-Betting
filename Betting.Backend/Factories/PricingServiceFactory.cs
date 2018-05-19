using System;
using Betting.Backend.Interfaces;
using Betting.Backend.Services.Interfaces;
using Betting.Repository.Factories;
using Betting.Repository.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Shared.Shared.Web;

namespace Betting.Backend.Factories
{
    public class PricingServiceFactory : IPricingServiceFactory
    {
        private readonly IRepoServiceFactory _repoServiceFactory;
        private readonly ILogServiceFactory  _logServiceFactory;
        private readonly IHttpRequestService _httpRequestService;
        private readonly ISettingRepoService _settingRepoService;
        private readonly IConfiguration      _configuration;
        private readonly ISteamMarketScraperService _steamMarketScraperService;
        private readonly IJsonRequestParser _jsonRequestParser;

        public PricingServiceFactory(
            IRepoServiceFactory repoServiceFactory,
            ILogServiceFactory logServiceFactory,
            IHttpRequestService httpRequestService,
            ISettingRepoService settingRepoService,
            IConfiguration configuration,
            ISteamMarketScraperService steamMarketScraperService,
            IJsonRequestParser jsonRequestParser
        )
        {
            _repoServiceFactory = repoServiceFactory;
            _logServiceFactory = logServiceFactory;
            _httpRequestService = httpRequestService;
            _settingRepoService = settingRepoService;
            _configuration = configuration;
            _steamMarketScraperService = steamMarketScraperService;
            _jsonRequestParser = jsonRequestParser;
        }

        public IPricingService GetPricingService(PricingServices services)
        {
            var pricingSection = _configuration.GetSection("Pricing");
            var steamLyticsApiKey = pricingSection.GetSection("Steamlytics").Value;

            switch (services)
            {
                case PricingServices.OpSkins:
                    return new OpSkinsApi(steamLyticsApiKey, _repoServiceFactory, _logServiceFactory, _httpRequestService,_settingRepoService,_steamMarketScraperService);
                case PricingServices.CsgoFast:
                    return new CsgoFastApi(steamLyticsApiKey, _repoServiceFactory, _logServiceFactory, _httpRequestService,_settingRepoService,_steamMarketScraperService,_jsonRequestParser);
                default:
                    throw new ArgumentOutOfRangeException(nameof(services), services, null);
            }
        }
    }
}
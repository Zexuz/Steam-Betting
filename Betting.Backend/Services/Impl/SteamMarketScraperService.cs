using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using Betting.Backend.Resources;
using Betting.Backend.Services.Interfaces;
using Betting.Models.Models;
using Betting.Repository.Exceptions;

namespace Betting.Backend.Services.Impl
{
    public class SteamMarketScraperService : ISteamMarketScraperService
    {
        private readonly string _baseUrl = "http://steamcommunity.com/market/search/render/?";

        public int PageSize { get; set; }

        public SteamMarketScraperService()
        {
            PageSize = 100;
        }

        public async Task<List<SteamMarketItem>> Scrape(int appId)
        {
            var checkupRequest = await DoCheckupRequest(appId);
            if (!checkupRequest.Success) throw new NoneExpectedResultException("The API call responsed with a none success statuscode");

            var items = new List<SteamMarketItem>();

            for (int i = 0; i < (checkupRequest.TotalCount / PageSize) + 1; i++)
            {
                var query = new SteamMarketQuery(i*PageSize, PageSize, "quantity", appId);
                var request = await MakeRequest(query);
                if (!request.Success) throw new NoneExpectedResultException("The API call responsed with a none success statuscode");

                var document = ParseDocument(request.Results);

                var elements = document.QuerySelectorAll("a.market_listing_row_link");

                foreach (var element in elements)
                {
                    var img = element.FirstElementChild.FirstElementChild.GetAttribute("src");
                    var name = element.QuerySelector("div.market_listing_item_name_block span").TextContent;
                    items.Add(new SteamMarketItem
                    {
                        ImageSrc = img,
                        Name = name
                    });
                }
            }

            return items;
        }

        private static IHtmlDocument ParseDocument(string html)
        {
            var parser = new HtmlParser();
            var removedWhiteSpacesResult = html
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace("\t", "");
            var document = parser.Parse(removedWhiteSpacesResult);
            return document;
        }

        private async Task<SteamMarketResource> DoCheckupRequest(int appId)
        {
            var checkupQuery = new SteamMarketQuery(0, 1, "quantity", appId);
            var checkupRequest = await MakeRequest(checkupQuery);
            return checkupRequest;
        }

        private async Task<SteamMarketResource> MakeRequest(SteamMarketQuery query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _baseUrl + query.ToString());

            var res = await new HttpClient().SendAsync(request);
            var resString = await res.Content.ReadAsStringAsync();

            var resObj = SteamMarketResource.FromJson(resString);
            return resObj;
        }
    }
}
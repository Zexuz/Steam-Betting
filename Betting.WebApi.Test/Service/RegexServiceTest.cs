using System.Text.RegularExpressions;
using Betting.Backend.Exceptions;
using Betting.Backend.Services.Impl;
using Xunit;

namespace Betting.WebApi.Test.Service
{
    public class RegexServiceTest
    {
        private readonly Regex _regex;

        public RegexServiceTest()
        {
            _regex = new Regex(@".*/openid/id/([\d]{13,25})");
        }

        [InlineData("http://steamcommunity.com/openid/id/76561198077954112")]
        [InlineData("https://steamcommunity.com/openid/id/76561198077954112")]
        [InlineData("/openid/id/76561198077954112")]
        [Theory]
        public void RegexMatchesSuccessfully(string textToMatch)
        {
            var match = new RegexService().GetFirstGroupMatch(_regex, textToMatch);

            Assert.Equal("76561198077954112", match);
        }


        [Fact]
        public void RegexThrowsDueToNoMatch()
        {
            var textToMatch = "http://steamcommunity.com/openid/id/notASteamId";
            var exception = Record.Exception(() => new RegexService().GetFirstGroupMatch(_regex, textToMatch));

            Assert.IsType(typeof(RegexMatchNotFoundException), exception);
        }

        [Fact]
        public void RegexReturnsEmptyStringDueToNoMatch()
        {
            var textToMatch = "http://steamcommunity.com/openid/id/notASteamId";
            var match = new RegexService().GetFirstGroupMatch(_regex, textToMatch, false);

            Assert.Equal(string.Empty, match);
        }
    }
}
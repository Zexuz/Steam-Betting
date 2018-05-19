using Betting.Models;
using Betting.Repository.Helpers;
using Xunit;

namespace Betting.Backend.Test
{
    public class EnumConverterTest
    {
        [Fact]
        public void MatchIsClosedReturnsIntSucces()
        {
            Assert.Equal(0, MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Closed));
        }

        [Fact]
        public void MatchIsOpenReturnsIntSucces()
        {
            Assert.Equal(1, MatchStatusHelper.GetIntFromMatchStatus(MatchStatus.Open));
        }

        [Fact]
        public void MatchIsClosedReturnsEnumSucces()
        {
            Assert.Equal(MatchStatus.Closed, MatchStatusHelper.GetMatchStatusFromInt(0));
        }

        [Fact]
        public void MatchIsOpenReturnsEnumSucces()
        {
            Assert.Equal(MatchStatus.Open, MatchStatusHelper.GetMatchStatusFromInt(1));
        }
    }
}
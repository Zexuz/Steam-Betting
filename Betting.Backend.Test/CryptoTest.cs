using System.Globalization;
using Betting.Backend.Services.Impl;
using Xunit;

namespace Betting.Backend.Test
{
    public class CryptoTest
    {
        [Fact]
        public void HashCreatedSuccessfullyTest()
        {
            var winPercentage = 49.548644548454;

            var hashService = new HashService();
            var retHash     = hashService.CreateBase64Sha512Hash(winPercentage.ToString(CultureInfo.InvariantCulture), "k6F9P8HZ7eiQwtOSPA45wfKmSkAX")
                .ToLower();
            Assert.Equal(
                "5663bed17ab0419ed7fa3e606f208f7e3ae6fe60a1f563cac897883fc41a9cab17144f1c283f4be5c8dd8f840c4defdb69ed4735ee566dc1ad52f668d7e0601c",
                retHash);
        }

        [Fact]
        public void HashCreatedSuccessfullyTest2()
        {
            var winPercentage = "xD";

            var hashService = new HashService();
            var retHash     = hashService.CreateBase64Sha512Hash(winPercentage, "dx").ToLower();
            Assert.Equal(
                "10345f72beeb9d538f165f5e8198f17b689d1dd6077cf87c79a5fbfc9b2ffdee6ed9d04005d61a6536c93e6406302b23dd2eca06c4ffc58ddabb2e9907bbceda",
                retHash);
        }
    }
}
using System;
using System.Globalization;
using System.Linq;
using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public class RandomService : IRandomService
    {
        private readonly int _maximumSaltLength;

        public RandomService(int maximumSaltLength)
        {
            _maximumSaltLength = maximumSaltLength;
        }

        public double GetRandomDoubleBetwine(double minimum, double maximum)
        {
            var random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public string GeneratePercentage()
        {
            var percentage = GetRandomDoubleBetwine(0.00000000001, 99.99999999999);;
            var percentageAsString = percentage.ToString(CultureInfo.InvariantCulture);
            return percentageAsString;
        }


        public string GenerateSalt()
        {
            return RandomString(44);
        }
        
        public string GenerateNewGuidAsString()
        {
            return Guid.NewGuid().ToString();
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[new Random().Next(s.Length)]).ToArray());
        }
    }
}
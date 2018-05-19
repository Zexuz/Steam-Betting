using System;
using System.Security.Cryptography;
using System.Text;

namespace Steam.Market.Factories
{
    internal class SteamTwoFactorGenerator
    {
        internal                string SharedSecret;
        private static readonly byte[] SteamGuardCodeTranslations = { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };

        internal string GenerateSteamGuardCodeForTime(long timestamp)
        {
            if (string.IsNullOrEmpty(SharedSecret))
            {
                return "";
            }

            var sharedSecretArray = Convert.FromBase64String(SharedSecret);
            var timeArray = new byte[8];

            timestamp /= 30L;

            for (var i = 8; i > 0; i--)
            {
                timeArray[i - 1] = (byte)timestamp;
                timestamp >>= 8;
            }

            var hmacGenerator = new HMACSHA1 { Key = sharedSecretArray };
            var hashedData = hmacGenerator.ComputeHash(timeArray);
            var codeArray = new byte[5];
            try
            {
                var b = (byte)(hashedData[19]                                                                                                          & 0xF);
                var codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

                for (var i = 0; i < 5; ++i)
                {
                    codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                    codePoint /= SteamGuardCodeTranslations.Length;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return Encoding.UTF8.GetString(codeArray).ToLower();
        }
    }
}
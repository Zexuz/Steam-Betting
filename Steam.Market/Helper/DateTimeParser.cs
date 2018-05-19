using System;
using System.Globalization;

namespace Steam.Market.Helper
{
    public static class DateTimeParser
    {
        public static DateTime ConvertSteamTime(string stringDate)
        {
            var dateTime = DateTime.ParseExact(stringDate.Replace(": +0",""), "MMM dd yyyy HH", CultureInfo.InvariantCulture);
            return dateTime;
        }
    }
}
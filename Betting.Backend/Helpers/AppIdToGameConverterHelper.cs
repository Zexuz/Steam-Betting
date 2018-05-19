using Betting.Models.Models;

namespace Betting.Backend.Helpers
{
    public static class AppIdToGameConverterHelper
    {
        public static bool IsPubgItem(DatabaseModel.ItemDescription item)
        {
            return item.AppId == 578080.ToString();
        }

        public static bool IsCsgoItem(DatabaseModel.ItemDescription item)
        {
            return item.AppId == 730.ToString();
        }
    }
}
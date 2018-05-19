using System.Collections.Generic;

namespace Betting.Repository.Helpers
{
    public static class ItemDescriptionHelper
    {
        private static Dictionary<string, string> dict = new Dictionary<string, string>
        {
            {"\u2605", @"\u2605"},
            {"\u58F1", @"\u58F1"},
            {"\u5F10", @"\u5F10"},
            {"\u9F8D", @"\u9F8D"},
            {"\u738b", @"\u738b"},
        };

        public static string ToDatabase(string name)
        {
            foreach (var kvp in dict)
            {
                name = name.Replace(kvp.Key, kvp.Value);
            }

            return name;
        }

        public static string FromDatabase(string name)
        {
            foreach (var kvp in dict)
            {
                name = name.Replace(kvp.Value, kvp.Key);
            }

            return name;
        }
    }
}
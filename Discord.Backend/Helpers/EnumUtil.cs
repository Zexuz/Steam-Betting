using System.Collections.Generic;
using System.Linq;
using Discord.Backend.Enum;

namespace Discord.Backend.Helpers
{
    public static class EnumUtil
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return System.Enum.GetValues(typeof(T)).Cast<T>();
        }
        
        public class Item
        {
            public string Name { get; set; }
            public int    Nr   { get; set; }

            public Item(Event @event)
            {
                Name = @event.ToString();
                Nr = (int) @event;
            }

            public override string ToString()
            {
                return $"Nr: {Nr}, Name: {Name}";
            }
        }
    }
}
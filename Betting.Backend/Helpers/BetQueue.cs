using System.Collections.Generic;
using Betting.Models.Models;

namespace Betting.Backend.Helpers
{
    internal class BetQueue
    {
        public Queue<Item> Items { get; }

        public BetQueue()
        {
            Items = new Queue<Item>();
        }

        public void Add(Item item)
        {
            Items.Enqueue(item);
        }


        public Item Next()
        {
            return Items.Dequeue();
        }

        public bool IsEmpty()
        {
            return Items.Count == 0;
        }

        internal class Item
        {
            public List<AssetAndDescriptionId> AssetAndDescriptionIds { get; set; }
            public int                         RoundId                { get; set; }
            public string                      SteamId                { get; set; }
            public DatabaseModel.GameMode      GamMode                { get; set; }
        }
    }
}
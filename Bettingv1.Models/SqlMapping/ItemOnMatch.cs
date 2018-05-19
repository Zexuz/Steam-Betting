using System.ComponentModel.DataAnnotations.Schema;

namespace Bettingv1.Models.SqlMapping
{
    public class ItemOnMatch
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("owner")]
        public string OwnerSteamId { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("matchid")]
        public int BettedOnRoundId { get; set; }

        [Column("icon")]
        public string ImageUrl { get; set; }
    }
}
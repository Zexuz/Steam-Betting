using System.ComponentModel.DataAnnotations.Schema;

namespace Bettingv1.Models.SqlMapping
{
    public class User
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("id")]
        public string SteamId { get; set; }

        [Column("avatar")]
        public string ImageUrl { get; set; }
    }
}
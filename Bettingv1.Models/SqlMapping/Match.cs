using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bettingv1.Models.SqlMapping
{
    public class Match
    {
        [Column("winner")]
        public string WinnerSteamId { get; set; }

        [Column("id")]
        public int RoundId { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("created")]
        public DateTime Created { get; set; }
    }
}
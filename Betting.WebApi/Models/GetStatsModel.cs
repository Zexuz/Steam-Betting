using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Betting.WebApi.Models
{
    public class GetStatsModel
    {
        /// <summary>
        /// From when
        /// </summary>
        [Required]
        public DateTime Start { get; set; }

        /// <summary>
        /// To when (but not include)
        /// </summary>
        [Required]
        public DateTime End { get; set; }

        /// <summary>
        /// Intervall span, 3600sec = 1 hour span.
        /// </summary>
        [Required]
        public int LenghtInSec { get; set; }
    }


    public class StatsResponse
    {
        public DateTime                         Start    { get; set; }
        public DateTime                         End      { get; set; }
        public string                           Text { get; set; }
        public List<Dictionary<string, object>> Values   { get; set; }
    }
}
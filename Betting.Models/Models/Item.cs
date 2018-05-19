using System;

namespace Betting.Models.Models
{
    public class Item
    {
        public User           Owner         { get; set; }
        public string         Name          { get; set; }
        public string         IconUrl       { get; set; }
        public decimal        Value         { get; set; }
        public string         AssetId       { get; set; }
        public int            DescriptionId { get; set; }
        public int            Id            { get; set; }
        public string         AppId         { get; set; }
        public bool           TookAsRake    { get; set; }
        public bool           Valid         { get; set; }
        public DateTimeOffset ReleaseTime   { get; set; }

        public double TimeUntillRelease =>
            (ReleaseTime - DateTimeOffset.Now).TotalSeconds > 0
                ? (ReleaseTime - DateTimeOffset.Now).TotalSeconds
                : 0;
    }
}
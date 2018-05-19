namespace Betting.Models.Models
{
    public class CreateCoinFlipSettingModel
    {
        public bool   AllowCsgo { get; set; }
        public bool   AllowPubg { get; set; }
        public int    MaxItem   { get; set; }
        public int    MinItem   { get; set; }
        public int    Diff      { get; set; }
        public string PreHash   { get; set; }
    }
}
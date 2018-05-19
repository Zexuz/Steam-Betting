namespace Betting.Backend.Resources
{
    public class SteamMarketQuery
    {
        public int    Start      { get; }
        public int    Count      { get; }
        public string SortColumn { get; }
        public int    AppId      { get; }
        public string SortDir    { get; }

        public SteamMarketQuery(int start, int count, string sortColumn, int appId, string sortDir = "desc")
        {
            Start = start;
            Count = count;
            SortColumn = sortColumn;
            AppId = appId;
            SortDir = sortDir;
        }

        public override string ToString()
        {
            return $"start={Start}&count={Count}&sort_column={SortColumn}&sort_dir={SortDir}&appid={AppId}";
        }
    }
}
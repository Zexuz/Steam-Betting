
using RpcCommunication;

namespace Betting.WebApi.Models
{
    public class SteamItemWrapper
    {
        public Item     Item  { get; }
        public decimal? Value { get; }
        public bool     Valid { get; }

        public SteamItemWrapper(Item item, decimal? value, bool valid)
        {
            Item = item;
            Value = value;
            Valid = valid;
        }
    }
}
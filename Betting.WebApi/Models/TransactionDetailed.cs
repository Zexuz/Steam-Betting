using System.Collections.Generic;

namespace Betting.WebApi.Models
{
    public class TransactionDetailed : TransactionBasic
    {
        public List<PlayerInventory.Item> Items { get; set; }
    }
}
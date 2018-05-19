using System.Collections.Generic;
using Bettingv1.Models.SqlMapping;

namespace Bettingv1.Models
{
    public class UserBet
    {
        public User            User  { get; set; }
        public List<ItemModel> Items { get; set; }

        public UserBet()
        {
            Items = new List<ItemModel>();
        }
    }
}
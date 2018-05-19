using System;

namespace Betting.Backend.Exceptions
{
    public class InventoryLimitExceeded : Exception
    {
        public InventoryLimitExceeded(string s) : base(s)
        {
        }
    }
}
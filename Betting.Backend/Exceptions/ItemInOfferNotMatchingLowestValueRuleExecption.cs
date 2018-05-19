using System;

namespace Betting.Backend.Exceptions
{
    public class ItemInOfferNotMatchingLowestValueRuleExecption : Exception
    {
        public ItemInOfferNotMatchingLowestValueRuleExecption(string s) : base(s)
        {
        }
    }
}
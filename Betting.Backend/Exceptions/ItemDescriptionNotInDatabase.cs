using System;

namespace Betting.Backend.Exceptions
{
    public class ItemDescriptionNotInDatabase : Exception
    {
        public ItemDescriptionNotInDatabase(string s) : base(s)
        {
        }
    }
}
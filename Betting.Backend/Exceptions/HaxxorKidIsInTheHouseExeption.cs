using System;

namespace Betting.Backend.Exceptions
{
    public class HaxxorKidIsInTheHouseExeption : Exception
    {
        public HaxxorKidIsInTheHouseExeption(string s) : base(s)
        {
        }
    }
}
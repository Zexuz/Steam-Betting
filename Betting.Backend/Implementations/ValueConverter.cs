using System;
using Betting.Backend.Exceptions;
using Betting.Backend.Interfaces;

namespace Betting.Backend.Implementations
{
    public class ValueConverter : IValueConverter
    {
        public int Convert(double d)
        {
            if (BitConverter.GetBytes(decimal.GetBits((decimal) d)[3])[2] > 2)
                throw new InvalidValueException("Value can only have 2 decimal points!");

            return (int) (d * 100);
        }
    }
}
using Betting.Backend.Exceptions;
using Betting.Backend.Implementations;
using Xunit;

namespace Betting.Backend.Test
{
    public class ValueConverterTest
    {
        [Theory]
        [InlineData(2.5)]
        [InlineData(2.50)]
        [InlineData(2.500000)]
        public void ConvertValueSuccessfully(double value)
        {
            var valueConverter = new ValueConverter();
            var res            = valueConverter.Convert(value);

            Assert.Equal(250, res);
        }

        [Theory]
        [InlineData(5484.54)]
        [InlineData(5484.54000)]
        [InlineData(5484.5400000)]
        public void ConvertAnotherValueSuccessfully(double value)
        {
            var valueConverter = new ValueConverter();
            var res            = valueConverter.Convert(value);

            Assert.Equal(548454, res);
        }

        [Theory]
        [InlineData(0.54)]
        [InlineData(0.54000)]
        [InlineData(0.5400000)]
        public void ConvertAnother1ValueSuccessfully(double value)
        {
            var valueConverter = new ValueConverter();
            var res            = valueConverter.Convert(value);

            Assert.Equal(54, res);
        }

        [Theory]
        [InlineData(1.545)]
        [InlineData(1.54050)]
        [InlineData(1.54000005)]
        public void ConvertAnotherValueThrowsException(double value)
        {
            var valueConverter = new ValueConverter();
            var ex             = Assert.Throws<InvalidValueException>(() => valueConverter.Convert(value));

            Assert.IsType<InvalidValueException>(ex);
        }
    }
}
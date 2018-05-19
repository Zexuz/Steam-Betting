namespace Bettingv1.Models.SqlMapping
{
    public static class StringToDecimalMapper
    {
        public static decimal? ToNullableDecimal(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            value = value.Replace(".", ",");
            decimal.TryParse(value,out var parsedValue);
            return parsedValue;
        }
        
        public static decimal ToDecimal(string value)
        {
            value = value.Replace(".", ",");
            decimal.TryParse(value,out var parsedValue);
            return parsedValue;
        }
    }
}
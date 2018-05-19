using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Betting.Repository.Extensions
{
    public static class SqlReaderExtension
    {
        public static async Task<T> ReadAsync<T>(this SqlDataReader reader, string fieldName, bool canBeNull = false, T defaultValue = default(T))
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(fieldName));

            int idx = reader.GetOrdinal(fieldName);

            if (await reader.IsDBNullAsync(idx))
                if (!canBeNull)
                    throw new ArgumentNullException(nameof(idx), $"The field {fieldName} can't be null, but it is ");
                else
                    return defaultValue;

            var value = await reader.GetFieldValueAsync<T>(idx);

            if (value == null)
                throw new ArgumentNullException(nameof(value), $"The field {fieldName} can't be null, but it is ");

            return value;
        }
    }
}
using System.Runtime.InteropServices;

namespace Betting.Repository.Test
{
    public class ConnectionStringsForTest
    {
        public string GetConnectionString(string databaseName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return $"Server=DESKTOP-5UP1TEB;Database={databaseName};Trusted_Connection=True;";

            return $"Server=localhost;Database={databaseName};User Id=SA;Password=RoBBaN96;";
        }
    }
}
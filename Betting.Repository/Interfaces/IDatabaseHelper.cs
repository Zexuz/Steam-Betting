using System.Threading.Tasks;

namespace Betting.Repository.Interfaces
{
    public interface IDatabaseHelper
    {
        Task<bool> DoesDatabaseExist();
        void       ResetDatabase();
        int        CreateDatabase();
        int        DropDatabase();
        int        CreateTables();
    }
}
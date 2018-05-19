using System.Threading.Tasks;

namespace Betting.Backend.Managers.Interface
{
    public interface IRpcManager
    {
        void Start();
        Task ShutdownAsync();
    }
}
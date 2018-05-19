using System.Threading.Tasks;

namespace Betting.Backend.Services.Interfaces
{
    public interface IPricingService
    {
        Task UpdatePricingForCsgoAsync();
        Task UpdatePricingForPubgAsync();
        Task UpdatePricingForAll();
    }
}
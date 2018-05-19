using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Factories
{
    public interface IPricingServiceFactory
    {
        IPricingService GetPricingService(PricingServices services);
    }

    public enum PricingServices
    {
        OpSkins,
        CsgoFast
    }
}
namespace Betting.Backend.Services.Interfaces
{
    public interface IRandomService
    {
        double GetRandomDoubleBetwine(double minimum, double maximum);
        string GeneratePercentage();
        string GenerateSalt();
        string GenerateNewGuidAsString();
    }
}
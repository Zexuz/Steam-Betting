using System.Text.RegularExpressions;

namespace Betting.Backend.Services.Interfaces
{
    public interface IRegexService
    {
        string GetFirstGroupMatch(Regex regex, string text, bool throwIfNotFound = true);
        string GetNGroupMatch(int n, Regex regex, string text, bool throwIfNotFound = true);
    }
}
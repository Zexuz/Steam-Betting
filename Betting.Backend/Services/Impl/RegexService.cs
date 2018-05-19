using System.Text.RegularExpressions;
using Betting.Backend.Exceptions;
using Betting.Backend.Services.Interfaces;

namespace Betting.Backend.Services.Impl
{
    public class RegexService : IRegexService
    {
        public string GetFirstGroupMatch(Regex regex, string text, bool throwIfNotFound = true)
        {
            return GetNGroupMatch(1, regex, text, throwIfNotFound);
        }

        public string GetNGroupMatch(int n, Regex regex, string text, bool throwIfNotFound = true)
        {
            var match = regex.Match(text);
            var value = match.Groups[n].Value;

            if (string.IsNullOrEmpty(value) && throwIfNotFound)
                throw new RegexMatchNotFoundException($"The match on {text} with regex {regex} was not successfull");

            return value;
        }
    }
}
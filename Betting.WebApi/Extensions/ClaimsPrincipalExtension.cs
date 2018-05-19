using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Betting.Backend.Services.Impl;

namespace Betting.WebApi.Extensions
{
    public static class ClaimsPrincipalExtension
    {
        public static string GetSteamId(this ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var regEx      = new Regex(@".*/openid/id/([\d]{13,25})");
            var steamClaim = user.Claims.ToArray()[0].Value;

             return new RegexService().GetFirstGroupMatch(regEx, steamClaim);
        }
    }
}
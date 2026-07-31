using System.Security.Claims;
using System.Security.Principal;

namespace Application.Host.Models
{
    public static class AuthExtension
    {
        public static int GetUserId(this ClaimsIdentity? identity)
        {
            return GetIntValue(identity, ClaimTypes.NameIdentifier);
        }

        public static int GetUserId(this IIdentity? identity)
        {
            var claimIdentity = identity as ClaimsIdentity;
            return claimIdentity?.GetUserId() ?? 0;
        }


        public static int GetIntValue(this IIdentity? identity, string name)
        {
            var claimIdentity = identity as ClaimsIdentity;
            if (claimIdentity == null)
                return 0;

            var claim = claimIdentity.FindFirst(name);
            if (claim != null && int.TryParse(claim.Value, out var d))
            {
                return d;
            }
            return 0;
        }
        public static string? GetStringValue(this IIdentity? identity, string name)
        {
            var claimIdentity = identity as ClaimsIdentity;
            if (claimIdentity == null)
                return null;

            var claim = claimIdentity.FindFirst(name);
            if (claim != null)
            {
                return claim.Value;
            }
            return null;
        }
    }
}

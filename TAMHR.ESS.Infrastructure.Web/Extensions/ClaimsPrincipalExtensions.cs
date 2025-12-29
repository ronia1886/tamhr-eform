using System.Linq;
using System.Security.Claims;

namespace TAMHR.ESS.Infrastructure.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetClaim(this ClaimsPrincipal claimsPrincipal, string type)
        {
            var value = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == type)?.Value;

            return value;
        }
    }
}

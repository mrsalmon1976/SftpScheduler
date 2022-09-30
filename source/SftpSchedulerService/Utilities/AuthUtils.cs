using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Security.Claims;

namespace SftpSchedulerService.Utilities
{
    public class AuthUtils
    {
        public bool UserHasRole(HttpContext httpContext, string role)
        {
            var user = httpContext.User;
            if (user == null)
            {
                return false;
            }

            return user.Claims.Any(x => x.Type == ClaimTypes.Role && x.Value == role);
        }

        public bool UserHasAtLeastOneRole(HttpContext httpContext, string[] roles)
        {
            var user = httpContext.User;
            if (user == null)
            {
                return false;
            }

            var claimTypes = user.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
            if (roles.Intersect(claimTypes).Any())
            {
                return true;
            }

            return false;
        }

    }
}

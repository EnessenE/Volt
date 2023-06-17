using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Volt.Models;

namespace Volt.Controllers
{
    public class ExtendedController : Controller
    {
        protected Account GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                return new Account
                {
                    Username = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value,
                };
            }
            return null;
        }

    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Controllers
{
    //TODO: Middleware
    public class ExtendedController : Controller
    {
        private readonly IAccountContext _accountContext;
        public ExtendedController(IAccountContext accountContext)
        {
            _accountContext = accountContext;
        }

        protected Account GetCurrentUser()
        {
            Account? foundAccount = null;
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                var rawUserId = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;

                if (rawUserId != null && Guid.TryParse(rawUserId, out var userId))
                {
                    foundAccount = _accountContext.GetAccount(userId);
                }
            }

            if (foundAccount != null)
            {
                return foundAccount;
            }
            //TODO: replace with a real exception
            throw new InvalidOperationException("Account not found");
        }
    }
}

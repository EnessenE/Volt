using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Volt.Interfaces;
using Volt.Models;
using Volt.Models.Login;

namespace Volt.Controllers
{
    [ApiController]
    [Route("login")]
    public class LoginController : ExtendedController
    {
        private readonly IAccountContext _accountContext;
        private readonly IConfiguration _config;

        public LoginController(IAccountContext accountContext, IConfiguration config)
        {
            _accountContext = accountContext;
            _config = config;
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login([FromBody] LoginRequest userLogin)
        {
            var user = Authenticate(userLogin);
            if (user != null)
            {
                var loginResult = new LoginResult()
                {
                    Discriminator = user.Discriminator,
                    Id = user.Id,
                    PrivateKey = "aa",
                    Username = user.Username
                };
                var token = GenerateToken(loginResult);
                return Ok(token);
            }
            return Forbid();
        }

        // To generate token
        private string GenerateToken(LoginResult user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.Username),
                new Claim(ClaimTypes.UserData,user.Id.ToString()),
                new Claim(ClaimTypes.Role,"user"),
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddDays(8),
                signingCredentials: credentials);


            return new JwtSecurityTokenHandler().WriteToken(token);

        }
        
        private Account Authenticate(LoginRequest userLogin)
        {
            var currentUser = _accountContext.GetAccounts().FirstOrDefault(x => x.Username.ToLowerInvariant() ==
                userLogin.Username.ToLowerInvariant() && x.Password == userLogin.Password);
            if (currentUser != null)
            {
                return currentUser;
            }
            return null;
        }
    }
}

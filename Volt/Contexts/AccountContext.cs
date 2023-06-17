using System.Security.Principal;
using Volt.Interfaces;
using Volt.Models;
using Volt.Models.Signup;

namespace Volt.Contexts
{

    public class AccountContext : IAccountContext
    {
        private List<Account> _accounts = new List<Account>()
        {
            new Account()
            {
                Discriminator = 1,
                Id = Guid.NewGuid(),
                Username = "Steph",
                Password = "steph"
            },
            new Account()
            {
                Discriminator = 916,
                Id = Guid.NewGuid(),
                Username = "Enes",
                Password = "enes"
            }
        };

        private readonly ILogger<AccountContext> _logger;

        public AccountContext(ILogger<AccountContext> logger)
        {
            _logger = logger;
        }

        public List<Account> GetAccounts()
        {
            return _accounts;
        }

        public Account RegisterAccount(SignupRequest signupRequest)
        {
            _logger.LogInformation("New account request: {request}", signupRequest.Username);
            var randomGenerator = new Random();

            var newAccount = new Account()
            {
                Discriminator = randomGenerator.Next(1, 1000000),
                Id = Guid.NewGuid(),
                Password = signupRequest.EncryptedPassword,
                Username = signupRequest.Username
            };
            _accounts.Add(newAccount);

            _logger.LogInformation("New account created: {acc}", newAccount);

            return newAccount;
        }

        public Account? GetAccount(Guid id)
        {
            _logger.LogDebug("Retrieving {acc}", id);
            var foundAccount = _accounts.FirstOrDefault(item => item.Id.Equals(id));
            _logger.LogInformation("Found account {acc}", foundAccount);
            return foundAccount;
        }
    }
}

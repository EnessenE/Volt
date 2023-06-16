using Volt.Interfaces;
using Volt.Models;

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

        public List<Account> GetAccounts()
        {
            return _accounts;
        }

        public Account RegisterAccount(SignupRequest signupRequest)
        {
            throw new NotImplementedException();
        }
    }
}

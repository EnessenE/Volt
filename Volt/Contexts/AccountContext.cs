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
                Username = "Steph"
            },
            new Account()
            {
                Discriminator = 916,
                Id = Guid.NewGuid(),
                Username = "Enes"
            }
        };

        public List<Account> GetAccounts()
        {
            return _accounts;
        }
    }
}

using Volt.Models;

namespace Volt.Interfaces
{
    public interface IAccountContext
    {
        List<Account> GetAccounts();
        Account RegisterAccount(SignupRequest signupRequest);
    }
}

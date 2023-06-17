using Volt.Models;
using Volt.Models.Signup;

namespace Volt.Interfaces
{
    public interface IAccountContext
    {
        List<Account> GetAccounts();
        Account RegisterAccount(SignupRequest signupRequest);
        Account? GetAccount(Guid id);
    }
}

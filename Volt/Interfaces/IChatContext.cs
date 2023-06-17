using Volt.Models;

namespace Volt.Interfaces
{
    public interface IChatContext
    {
        Task Save(ChatMessage chatMessage);
        Task Delete(ChatMessage chatMessage);
        Task Update(ChatMessage chatMessage);
        Task<Chat?> GetChat(List<Account> members);
    }

}

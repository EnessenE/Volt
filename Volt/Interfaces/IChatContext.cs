using Volt.Models;

namespace Volt.Interfaces
{
    public interface IChatContext
    {
        Task<DirectChat?> Save(ChatMessage chatMessage);
        Task<DirectChat?> Create(DirectChat chat);
        Task Delete(Guid chatId, ChatMessage chatMessage);
        Task Update(Guid chatId, ChatMessage chatMessage);
        Task<DirectChat?> GetChat(List<Account> members);
        Task<DirectChat?> GetChat(Guid chatId);
        Task<List<DirectChat>> GetUserChats(Guid account);
    }

}

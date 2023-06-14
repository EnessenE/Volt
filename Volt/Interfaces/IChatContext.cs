using Volt.Models;

namespace Volt.Interfaces
{
    public interface IChatContext
    {
        Task Save(ChatMessage chatMessage);
        void Delete(ChatMessage chatMessage);
        void Update(ChatMessage chatMessage);

        Task<Chat?> GetChat(Account sender, Account receiver);

    }

}

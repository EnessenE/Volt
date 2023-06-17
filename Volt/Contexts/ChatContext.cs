using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Contexts
{
    public class ChatContext : IChatContext
    {
        private readonly List<Chat> _chats;
        private readonly ILogger<ChatContext> _logger;

        public ChatContext(ILogger<ChatContext> logger)
        {
            _logger = logger;
            _chats = new List<Chat>();
        }

        public async Task Save(ChatMessage chatMessage)
        {
            var chat = await FindChat(chatMessage);
            if (chat == null)
            {
                var newChat = new Chat()
                {
                    Messages = new List<ChatMessage>(),
                    Members = new List<Account>() { chatMessage.Sender, chatMessage.Receiver },
                };
                _chats.Add(newChat);
                chat = newChat;

            }
            chatMessage.Id = Guid.NewGuid();

            await AddMessageToChat(chatMessage, chat);

            _logger.LogInformation("Added {msg}", chatMessage);

        }

        private Task AddMessageToChat(ChatMessage chatMessage, Chat chat)
        {
            chat.Messages.Add(chatMessage);
            return Task.CompletedTask;
        }

        public async Task Delete(ChatMessage chatMessage)
        {
            var chat = await FindChat(chatMessage);
            if (chat != null)
            {
                var chatToDelete = FindChatMessage(chatMessage, chat);
                if (chatToDelete != null)
                {
                    chat.Messages.Remove(chatToDelete);
                    _logger.LogInformation("Deleted {msg}", chatMessage);
                }
                else
                {
                    _logger.LogInformation("Couldn't find corresponding chat for {chatMessage}", chatMessage);
                }
            }

        }

        public async Task Update(ChatMessage chatMessage)
        {
            var chat = await FindChat(chatMessage);
            if (chat != null)
            {
                var chatToUpdate = FindChatMessage(chatMessage, chat);
                if (chatToUpdate != null)
                {
                    chatToUpdate.LastUpdated = DateTime.UtcNow;
                    _logger.LogInformation("Deleted {msg}", chatMessage);
                }
                else
                {
                    _logger.LogInformation("Couldn't find corresponding chat for {chatMessage}", chatMessage);
                }
            }
        }

        public Task<Chat?> GetChat(List<Account> members)
        {
            var remainingChats = _chats;
            foreach (var member in members)
            {
                if (remainingChats.Any())
                {
                    remainingChats = remainingChats.Where(chat => chat.Members.Where(member => member.Id.Equals(member.Id)) != null).ToList();
                }
            }

            return Task.FromResult(remainingChats.FirstOrDefault());
        }

        private async Task<Chat?> FindChat(ChatMessage chatMessage)
        {
            return await GetChat(new List<Account>() { chatMessage.Receiver, chatMessage.Sender });
        }

        private ChatMessage? FindChatMessage(ChatMessage chatMessage, Chat chat)
        {
            return chat.Messages.Where(message => message.Id == chatMessage.Id)?.First();
        }
    }
}
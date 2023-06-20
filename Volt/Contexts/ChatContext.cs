using Microsoft.AspNetCore.Identity;
using Volt.Helpers;
using Volt.Hubs;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Contexts
{
    public class ChatContext : IChatContext
    {
        private readonly List<DirectChat> _chats;
        private readonly ILogger<ChatContext> _logger;
        private ConnectionManager _connectionManager;

        public ChatContext(ILogger<ChatContext> logger, ConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _chats = new List<DirectChat>();
        }

        public async Task<DirectChat?> Save(ChatMessage chatMessage)
        {
            DirectChat directChat;

            directChat = await GetChat(chatMessage.ChatId.Value);
            chatMessage.Id = Guid.NewGuid();


            await AddMessageToChat(chatMessage, directChat);

            _logger.LogInformation("Added {msg}", chatMessage);
            return directChat;
        }

        public Task<DirectChat?> Create(DirectChat chat)
        {
            chat.Id = Guid.NewGuid();
            chat.Messages = new List<ChatMessage>();
            _chats.Add(chat);
            _logger.LogInformation("Created a new chat between {sender} and {receiver}", chat.Members[0],
                chat.Members[1]);

            chat.EncryptionKey = PasswordGenerator.GenerateRandomPassword(new PasswordOptions() { RequiredLength = 100 });
 
            return Task.FromResult(chat)!;
        }

        private Task AddMessageToChat(ChatMessage chatMessage, DirectChat directChat)
        {
            directChat.Messages.Add(chatMessage);
            return Task.CompletedTask;
        }

        public async Task Delete(Guid chatId, ChatMessage chatMessage)
        {
            var chat = await GetChat(chatId);
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
                    _logger.LogInformation("Couldn't find corresponding directChat for {chatMessage}", chatMessage);
                }
            }

        }

        public async Task Update(Guid chatId, ChatMessage chatMessage)
        {
            var chat = await GetChat(chatId);
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
                    _logger.LogInformation("Couldn't find corresponding directChat for {chatMessage}", chatMessage);
                }
            }
        }

        public Task<DirectChat?> GetChat(List<Account> members)
        {
            var remainingChats = _chats;
            foreach (var relevantMember in members)
            {
                if (remainingChats.Any())
                {
                    remainingChats = remainingChats.Where(chat => chat.Members.Any(member => member.Id == relevantMember.Id)).ToList();
                }
            }

            return Task.FromResult(remainingChats.FirstOrDefault());
        }

        public Task<DirectChat?> GetChat(Guid chatId)
        {
            return Task.FromResult(_chats.FirstOrDefault(chat => chat.Id == chatId));
        }

        public Task<List<DirectChat>> GetUserChats(Guid account)
        {
            var foundChats = _chats.Where(chat => chat.Members.Any(member => member.Id == account)).ToList();
            return Task.FromResult(foundChats);
        }


        private ChatMessage? FindChatMessage(ChatMessage chatMessage, DirectChat directChat)
        {
            return directChat.Messages.Where(message => message.Id == chatMessage.Id)?.First();
        }
    }
}
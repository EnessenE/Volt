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
            var chat = FindChat(chatMessage);
            if (chat == null)
            {
                var newChat = new Chat()
                {
                    Messages = new List<ChatMessage>(),
                    Sender = chatMessage.Sender,
                    Receiver = chatMessage.Receiver
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

        public void Delete(ChatMessage chatMessage)
        {
            var chat = FindChat(chatMessage);
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

        public void Update(ChatMessage chatMessage)
        {
            var chat = FindChat(chatMessage);
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

        public Task<Chat?> GetChat(Account sender, Account receiver)
        {
            var chat = FindChat(receiver, sender);

            return Task.FromResult(chat);
        }

        private Chat? FindChat(ChatMessage chatMessage)
        {
            return FindChat(chatMessage.Receiver, chatMessage.Sender);
        }

        private Chat? FindChat(Account receiver, Account sender)
        {
            var chat = _chats.Find(chat => chat.Receiver == receiver && chat.Sender == sender);
            if (chat == null)
            {
                _logger.LogInformation("Couldn't find chat between {send} > {rec}", sender, receiver);
            }
            return chat;
        }

        private ChatMessage? FindChatMessage(ChatMessage chatMessage, Chat chat)
        {
            return chat.Messages.Where(message => message.Id == chatMessage.Id)?.First();
        }
    }
}
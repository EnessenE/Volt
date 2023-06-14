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

        private byte[] _initVector =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
        };

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

        private async Task AddMessageToChat(ChatMessage chatMessage, Chat chat)
        {
            var password = chatMessage.Receiver.ToString() + chatMessage.Sender.ToString();

            var key = DeriveKeyFromPassword(password);

            var encryptedContent = await EncryptContent(key, chatMessage.Message);
            chatMessage.EncryptedMessage = encryptedContent;
            chat.Messages.Add(chatMessage);
        }


        private async Task<byte[]> EncryptContent(byte[] key, string unencryptedMessage)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = _initVector;


            using MemoryStream output = new();
            using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(unencryptedMessage));
            await cryptoStream.FlushFinalBlockAsync();

            var encryptedContent = Encoding.UTF8.GetString(output.ToArray());
            _logger.LogInformation("Encrypted: {msg} > {encrypted}", unencryptedMessage, encryptedContent);
            return output.ToArray();
        }

        public async Task<string> DecryptContent(byte[] key, byte[] encryptedContent)
        {
            using Aes aes = Aes.Create();
            aes.IV = _initVector;
            aes.Key = key;
            using MemoryStream input = new(encryptedContent);
            using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new();
            await cryptoStream.CopyToAsync(output);
            return Encoding.Unicode.GetString(output.ToArray());
        }

        private byte[] DeriveKeyFromPassword(string password)
        {
            var emptySalt = Array.Empty<byte>();
            var iterations = 1000;
            var desiredKeyLength = 16; // 16 bytes equal 128 bits.
            var hashMethod = HashAlgorithmName.SHA384;
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
                emptySalt,
                iterations,
                hashMethod,
                desiredKeyLength);
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
                    chatToUpdate.Message = chatMessage.Message;
                    chatToUpdate.LastUpdated = DateTime.UtcNow;
                    _logger.LogInformation("Deleted {msg}", chatMessage);
                }
                else
                {
                    _logger.LogInformation("Couldn't find corresponding chat for {chatMessage}", chatMessage);
                }
            }
        }

        public async Task<Chat?> GetChat(Account sender, Account receiver)
        {
            var chat = FindChat(receiver, sender);


            if (chat == null)
            {
                _logger.LogInformation("Couldn't find chat between {send} > {rec}", sender, receiver);
            }
            else
            {

                var password = receiver.ToString() + sender.ToString();
                var key = DeriveKeyFromPassword(password);

                var unencryptedChat = JsonSerializer.Serialize(chat);
                var clonedChat = JsonSerializer.Deserialize<Chat>(unencryptedChat);
                foreach (var chatMessage in clonedChat.Messages)
                {
                    chatMessage.Message = await DecryptContent(key, chatMessage.EncryptedMessage);
                }
            }
            return chat;
        }

        private Chat? FindChat(ChatMessage chatMessage)
        {
            return FindChat(chatMessage.Receiver, chatMessage.Sender);
        }

        private Chat? FindChat(Account receiver, Account sender)
        {
            return _chats.Find(chat => chat.Receiver == receiver && chat.Sender == sender);
        }

        private ChatMessage? FindChatMessage(ChatMessage chatMessage, Chat chat)
        {
            return chat.Messages.Where(message => message.Id == chatMessage.Id)?.First();
        }
    }
}
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        private readonly IChatContext _chatContext;
        private readonly IAccountContext _accountContext;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatContext chatContext, IAccountContext accountContext, ILogger<ChatHub> logger)
        {
            _chatContext = chatContext;
            _accountContext = accountContext;
            _logger = logger;
        }


        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;

            var user = GetCurrentUser();
            _logger.LogInformation("{acc} has connected", user);
            if (user.Connections == null)
            {
                user.Connections = new ConcurrentDictionary<string, ConnectionData>();
            }
            lock (user.Connections)
            {
                user.Connections.TryAdd(connectionId, new ConnectionData());
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {

            var user = GetCurrentUser();
            _logger.LogInformation("{acc} has gracefully disconnected", user);

            string connectionId = Context.ConnectionId;

            lock (user.Connections)
            {

                user.Connections.TryRemove(connectionId, out _);
            }

            return base.OnDisconnectedAsync(exception);
        }


        public async Task<DirectChat?> SendDirectChat(ChatMessage message)
        {
            var sender = GetCurrentUser();
            var receiver = _accountContext.GetAccount(message.Receiver.Id);

            var directChat = await _chatContext.GetChat(new List<Account>() { sender, receiver });
            if (receiver != null)
            {
                message.Receiver = receiver;
                message.Sender = sender;
                message.Created = DateTime.UtcNow;
                message.LastUpdated = null;

                if (directChat == null)
                {
                    _logger.LogInformation("No direct chat found between {sender} and {receiver}", sender, receiver);
                    directChat = await _chatContext.Create(new DirectChat()
                    {
                        Members = new List<Account>() { message.Receiver, message.Sender }
                    });

                }
                message.ChatId = directChat.Id;

                await _chatContext.Save(message);

                await NotifyAllRelevantParties(directChat, message);
                return directChat;
            }
            else
            {
                _logger.LogWarning("Receiver didn't exist: {id}", message.Receiver);
            }

            return null;
        }

        private async Task NotifyAllRelevantParties(DirectChat directChat, ChatMessage message)
        {
            foreach (var member in directChat.Members)
            {
                if (member.Connections != null)
                {
                    foreach (var userConnection in member.Connections)
                    {
                        var client = Clients.Client(userConnection.Key);
                        _logger.LogInformation("Notifying {member} - {con}", member, userConnection.Key);
                        Task.Run(async () => { await client.SendAsync("ReceiveChatMessage", message); });
                    }
                }

                else
                {
                    _logger.LogInformation("{acc} is currently not connected", member);
                }
            }

        }


        public async Task<DirectChat?> GetChat(Guid userId)
        {
            var directChat = await _chatContext.GetChat(new List<Account>() { GetCurrentUser(), new Account(){Id = userId} });
            return directChat;
        }

        public Task<List<Account>> SearchUser(string searchText)
        {
            _logger.LogInformation("Searching for {name}", searchText);
            var accounts = new List<Account>();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                accounts = _accountContext.GetAccounts().Where(account =>
                    account.Username.Contains(searchText, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }

            return Task.FromResult(accounts);
        }

        public Task<Account?> GetUser(Guid id)
        {
            _logger.LogInformation("Searching for {name}", id);
            var account = _accountContext.GetAccount(id);
            return Task.FromResult(account);
        }

        protected Account GetCurrentUser()
        {
            Account? foundAccount = null;
            var identity = Context.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                var rawUserId = userClaims.FirstOrDefault(x => x.Type == ClaimTypes.UserData)?.Value;

                if (rawUserId != null && Guid.TryParse(rawUserId, out var userId))
                {
                    foundAccount = _accountContext.GetAccount(userId);
                }
            }

            if (foundAccount != null)
            {
                return foundAccount;
            }
            //TODO: replace with a real exception
            throw new InvalidOperationException("Account not found");
        }
    }
}

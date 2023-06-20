using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Volt.Interfaces;
using Volt.Models;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace Volt.Hubs
{
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VoiceHub : Hub
    {
        private readonly IAccountContext _accountContext;
        private readonly ILogger<ChatHub> _logger;

        public VoiceHub(IAccountContext accountContext, ILogger<ChatHub> logger)
        {
            _accountContext = accountContext;
            _logger = logger;
        }


        public async Task SendAudio(string[] base64Array)
        {
            _logger.LogInformation("{acc} is talking: {size}!", GetCurrentUser(), base64Array.Length);

            try
            {
                foreach (string base64 in base64Array)
                {
                    // Convert each base64 string back to a Blob or perform any other required operations
                    byte[] bytes = Convert.FromBase64String(base64);
                    // ... do something with the bytes ...
                    // Broadcast the audio to all connected clients except the sender
                }
                await Clients.Others.SendAsync("ReceiveAudio", base64Array);
            }
            catch (FormatException)
            {
                _logger.LogError("Failed to decode it :(");
                // Handle invalid base64 data
            }
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;

            var user = GetCurrentUser();
            _logger.LogInformation("{acc} has connected to the voice server", user);
            if (user.VoiceConnections == null)
            {
                user.VoiceConnections = new ConcurrentDictionary<string, ConnectionData>();
            }

            lock (user.VoiceConnections)
            {
                user.VoiceConnections.TryAdd(connectionId, new ConnectionData());
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {

            var user = GetCurrentUser();
            _logger.LogInformation("{acc} has gracefully disconnected from voice", user);

            string connectionId = Context.ConnectionId;

            lock (user.VoiceConnections)
            {

                user.VoiceConnections.TryRemove(connectionId, out _);
            }

            return base.OnDisconnectedAsync(exception);
        }

        private async Task NotifyAllRelevantParties(DirectChat directChat, ChatMessage message)
        {
            foreach (var member in directChat.Members)
            {
                if (member.VoiceConnections != null)
                {
                    foreach (var userConnection in member.VoiceConnections)
                    {
                        var client = Clients.Client(userConnection.Key);
                        _logger.LogInformation("Notifying {member} - {con}", member, userConnection.Key);
                        Task.Run(async () => { await client.SendAsync("ReceiveChatMessage", message); });
                    }
                }

                else
                {
                    _logger.LogWarning("{acc} is currently not connected", member);
                }
            }

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
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Volt.Exceptions;
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


        public async Task SendAudioData(Dictionary<int, float> audioData)
        {
            _logger.LogInformation("{acc} is talking: {size}!", GetCurrentUser(), audioData.Count);

            // Process the audio data as per your requirements
            // For example, you can save it to a database, perform analysis, etc.

            // Broadcast the audio data to all other clients except the sender
            var audioInformation = new AudioInformation()
            {
                Speaker = GetCurrentUser(),
                AudioData = audioData
            };
            await Clients.Others.SendAsync("ReceiveAudioData", audioInformation);
        }

        public override async Task OnConnectedAsync()
        {
            string connectionId = Context.ConnectionId;

            var user = GetCurrentUser();
            _logger.LogInformation("{acc} has connected to the voice server", user);
            if (user.VoiceConnections.Any())
            {
                user.VoiceConnections = new ConcurrentDictionary<string, ConnectionData>();
            }

            user.VoiceConnections.TryAdd(connectionId, new ConnectionData());

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {

            var user = GetCurrentUser();
            _logger.LogInformation("{acc} has gracefully disconnected from voice", user);

            string connectionId = Context.ConnectionId;

            user.VoiceConnections.TryRemove(connectionId, out _);

            return base.OnDisconnectedAsync(exception);
        }

        private Task NotifyAllRelevantParties(DirectChat directChat, ChatMessage message)
        {
            foreach (var member in directChat.Members)
            {
                if (member.VoiceConnections.Any())
                {
                    foreach (var userConnection in member.VoiceConnections)
                    {
                        var client = Clients.Client(userConnection.Key);
                        _logger.LogInformation("Notifying {member} - {con}", member, userConnection.Key);
                        // TODO: actually monitor/track the chat being sent to other users
                        Task.Run(async () => { await client.SendAsync("ReceiveChatMessage", message); });
                    }
                }

                else
                {
                    _logger.LogWarning("{acc} is currently not connected", member);
                }
            }

            return Task.CompletedTask;
        }


        protected Account GetCurrentUser()
        {
            Account? foundAccount = null;
            var identity = Context.User?.Identity as ClaimsIdentity;
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

            throw new VoltAuthenticationException("Account not found");
        }
    }
}
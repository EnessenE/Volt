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

        public ChatHub(IChatContext chatContext, IAccountContext accountContext)
        {
            _chatContext = chatContext;
            _accountContext = accountContext;
        }

        public async Task SendChat(ChatMessage message)
        {
            var acc1 = GetCurrentUser();
            var acc2 = message.Receiver;
            message.Receiver = acc2;
            message.Sender = acc1;
            message.Created = DateTime.UtcNow;
            message.LastUpdated = null;
            await _chatContext.Save(message);
            await Task.Run(async () => await NotifyAllParties(message));
        }

        private async Task NotifyAllParties(ChatMessage message)
        {
            await Clients.Caller.SendAsync("ReceiveChatMessage", message);

        }


        public async Task<Chat?> GetChat()
        {
            var acc1 = _accountContext.GetAccounts()[0];
            var acc2 = _accountContext.GetAccounts()[1];
            var chat = await _chatContext.GetChat(new List<Account>(){acc1, acc2});
            return chat;
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

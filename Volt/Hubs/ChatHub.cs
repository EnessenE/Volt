using Microsoft.AspNetCore.SignalR;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Hubs
{
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
            var acc1 = _accountContext.GetAccounts()[0];
            var acc2 = _accountContext.GetAccounts()[1];
            message.Receiver = acc2;
            message.Sender = acc1;
            message.Created = DateTime.UtcNow;
            await _chatContext.Save(message);
        }


        public async Task GetChat()
        {
            var acc1 = _accountContext.GetAccounts()[0];
            var acc2 = _accountContext.GetAccounts()[1];
            var chat = await _chatContext.GetChat(acc1, acc2);
            await Clients.Caller.SendAsync("ReceiveChat", chat);
        }
    }
}

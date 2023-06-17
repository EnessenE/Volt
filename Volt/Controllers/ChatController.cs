﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("v1/chat")]
    public class ChatController : ExtendedController
    {
        private readonly IChatContext _chatContext;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatContext chatContext, ILogger<ChatController> logger, IAccountContext accountContext) : base(accountContext)
        {
            _chatContext = chatContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Chat?>> GetChat([FromQuery] List<Guid> guids)
        {
            var userGuid = GetCurrentUser().Id;
            if (guids.Contains(userGuid))
            {
                var targets = guids.Select(guid => new Account() { Id = guid });
                return Ok(await _chatContext.GetChat(targets.ToList()));
            }
            else
            {
                _logger.LogWarning("{user} tried retrieving an chat they weren't a part off!", GetCurrentUser().Username);
                return Forbid();
            }
        }

        [HttpGet("mine")]
        public async Task<ActionResult<List<Chat>>> GetChats()
        {
            var userGuid = GetCurrentUser().Id;
            return Ok(await _chatContext.GetUserChats(userGuid));
        }
    }
}

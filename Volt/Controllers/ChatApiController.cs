using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volt.Interfaces;
using Volt.Models;

namespace Volt.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("chats")]
    public class ChatApiController : ExtendedController
    {
        private readonly IChatContext _chatContext;
        private readonly ILogger<ChatApiController> _logger;

        public ChatApiController(IChatContext chatContext, ILogger<ChatApiController> logger)
        {
            _chatContext = chatContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<Chat?>> GetChat([FromQuery] List<Guid> guids)
        {
            if (guids.Contains(GetCurrentUser().Id))
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
    }
}

using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Volt.Controllers
{
    [Route("v1/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        [HttpGet]
        public ApiInformation Get()
        {
            return new ApiInformation();
        }
    }

    public class ApiInformation
    {
        public string Version { get; set; } = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
        public string Host { get; set; } = Environment.MachineName;
    }
}

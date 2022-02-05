using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConventionManagementService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthyController : ControllerBase
    {
        [HttpGet("ping")]
        public string Ping()
        {
            return "Pong";
        }

        [HttpPost("echo")]
        [Authorize]
        public string Echo(string message)
        {
            return message;
        }
    }
}

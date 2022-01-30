using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

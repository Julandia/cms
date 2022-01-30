using ConventionManagementService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ConventionManagementService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConventionsController : ControllerBase
    {
        private IConventionManager _ConventionManager;
        private readonly ILogger<ConventionsController> _Logger;

        public ConventionsController(IConventionManager conventionManager, ILogger<ConventionsController> logger)
        {
            _ConventionManager = conventionManager;
            _Logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public Task<IEnumerable<Convention>> Get([FromQuery] int max)
        {
            return _ConventionManager.GetConvetions(max);
        }

        // GET api/<ConventionController>/xxxx-xxxx-xxxx-xxxx
        [HttpGet("{id}")]
        [AllowAnonymous]
        public Task<Convention> Get(string id)
        {
            return _ConventionManager.GetConvention(id);
        }

        // POST api/<ConventionController>
        [HttpPost]
        [Authorize(Policy = "CRUD")]
        public Task<Convention> Post([FromBody]Convention convention)
        {
            return _ConventionManager.CreateConvention(convention);
        }

        // PUT api/<ConventionController>/5
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUD")]
        public Task<Convention> Put(string id, [FromBody] Convention convention)
        {
            return _ConventionManager.UpdateConvention(convention);
        }

        // DELETE api/<ConventionController>/xxxx-xxxx-xxxx-xxxx
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUD")]
        public async Task Delete(string id)
        {
            await _ConventionManager.DeleteConvention(id);
        }

        // Get api/<ConventionController>?max=10
        [HttpGet("userConvention/{userId}")]
        [Authorize]
        public async Task<IEnumerable<Convention>> Get([FromRoute]string userId, [FromQuery]int max)
        {
            return await _ConventionManager.GetConvetions(userId, max);
        }

        // Get api/<ConventionController>?max=10
        [HttpGet("registered/{userId}")]
        [Authorize]
        public async Task<IEnumerable<Convention>> GetRegisteredConventions(string userId)
        {
            return await _ConventionManager.GetRegisteredConvetions(userId);
        }

        // Post api/<ConventionController>/registerconvention
        [HttpPost("register/convention")]
        [Authorize]
        public async Task RegisterConvention([FromBody] RegisterParameter parameter)
        {
            await _ConventionManager.RegisterConvention(parameter.ConventionId, parameter.UserId, parameter.NumberOfParticipants);
        }

        // Post api/<ConventionController>/registerevent
        [HttpPost("register/event")]
        [Authorize]
        public async Task RegisterEvent([FromBody] RegisterParameter parameter)
        {
            await _ConventionManager.RegisterEvent(parameter.ConventionId, parameter.EventId, parameter.UserId, parameter.NumberOfParticipants);
        }

        public class RegisterParameter
        {
            public string ConventionId { get; set; }

            public string EventId { get; set; }

            public string UserId { get; set; }

            public int NumberOfParticipants { get; set; }
        }
    }
}

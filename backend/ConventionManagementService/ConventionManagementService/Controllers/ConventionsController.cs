using ConventionManagementService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Convention> Get([FromQuery] int max)
        {
            return _ConventionManager.GetConvetions(max);
        }

        // GET api/conventions/xxxx-xxxx-xxxx-xxxx
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public Task<Convention> Get(string id)
        {
            return _ConventionManager.GetConvention(id);
        }

        // POST api/conventions
        [HttpPost]
        [Authorize(Policy = "CRUD")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<Convention> Post([FromBody]Convention convention)
        {
            return _ConventionManager.CreateConvention(convention);
        }

        // PUT api/conventions/xxxx-xxxx-xxxx-xxxx
        [HttpPut("{id}")]
        [Authorize(Policy = "CRUD")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<Convention> Put(string id, [FromBody] Convention convention)
        {
            return _ConventionManager.UpdateConvention(convention);
        }

        // DELETE api/conventions/xxxx-xxxx-xxxx-xxxx
        [HttpDelete("{id}")]
        [Authorize(Policy = "CRUD")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task Delete(string id)
        {
            await _ConventionManager.DeleteConvention(id);
        }

        // Get api/conventions?max=10
        [HttpGet("userConvention/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Convention> Get([FromRoute]string userId, [FromQuery]int max)
        {
            return _ConventionManager.GetConvetions(userId, max);
        }

        // Get api/conventions?max=10
        [HttpGet("registered/{userId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Convention> GetRegisteredConventions(string userId)
        {
            return _ConventionManager.GetRegisteredConvetions(userId);
        }

        // Post api/conventions/register/convention
        [HttpPost("register/convention")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task RegisterConvention([FromBody] RegisterParameter parameter)
        {
            await _ConventionManager.RegisterConvention(parameter.ConventionId, parameter.UserId, parameter.NumberOfParticipants);
        }

        // Post api/conventions/register/event
        [HttpPost("register/event")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task RegisterEvent([FromBody] RegisterParameter parameter)
        {
            await _ConventionManager.RegisterEvent(parameter.ConventionId, parameter.EventId, parameter.UserId, parameter.NumberOfParticipants);
        }

        // POST api/conventions/populate
        [HttpPost("populate")]
        [Authorize(Policy = "CRUD")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public Task Populate([FromBody] Convention convention)
        {
            return _ConventionManager.PopulateData();
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

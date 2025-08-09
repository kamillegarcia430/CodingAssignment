// ParentsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VolunteerScheduler.Application.Commands.ParentCommandHandlers;
using VolunteerScheduler.Application.Queries.ParentQueries;

namespace VolunteerScheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ParentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateParentCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("Input is null.");
            }
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { parentId = id }, null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parents = await _mediator.Send(new GetAllParentsQuery());
            return Ok(parents);
        }

        [HttpGet("{parentId}")]
        public async Task<IActionResult> GetById(int parentId)
        {
            var parent = await _mediator.Send(new GetParentByIdQuery(parentId));
            return parent == null ? NotFound() : Ok(parent);
        }

        [HttpPut("{parentId}")]
        public async Task<IActionResult> Update(int parentId, [FromBody] UpdateParentCommand command)
        {
            if (parentId != command.ParentId) return BadRequest("ID mismatch.");
            var success = await _mediator.Send(command);
            return success ? Ok("Parent data successfully updated.") : NotFound();
        }

        [HttpDelete("{parentId}")]
        public async Task<IActionResult> Delete(int parentId)
        {
            var success = await _mediator.Send(new DeleteParentCommand(parentId));
            return success ? Ok("Parent data successfully deleted.") : NotFound();
        }
    }
}

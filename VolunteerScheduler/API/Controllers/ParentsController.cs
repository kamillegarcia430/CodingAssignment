// ParentsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VolunteerScheduler.Application.Commands.ParentCommandHandlers;
using VolunteerScheduler.Application.Queries.ParentQueries;
using VolunteerScheduler.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

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
        [SwaggerOperation(
            Summary = "Create a new parent",
            Description = "Creates a new parent with the provided details."
        )]
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
        [SwaggerOperation(
            Summary = "Get all parents",
            Description = "Retrieves a list of all parents."
        )]
        public async Task<IActionResult> GetAll()
        {
            var parents = await _mediator.Send(new GetAllParentsQuery());
            return Ok(parents);
        }

        [HttpGet("{parentId}")]
        [SwaggerOperation(
            Summary = "Get a parent by ID",
            Description = "Retrieves the parent details by their unique ID."
        )]
        public async Task<IActionResult> GetById(int parentId)
        {
            var parent = await _mediator.Send(new GetParentByIdQuery(parentId));
            return parent != null ? Ok(parent) : throw new KeyNotFoundException($"Parent with ID {parentId} does not exist.");
        }

        [HttpPut("{parentId}")]
        [SwaggerOperation(
            Summary = "Update parent details",
            Description = "Updates parent details by ID."
        )]
        public async Task<IActionResult> Update(int parentId, [FromBody] UpdateParentCommand command)
        {
            if (parentId != command.ParentId) return BadRequest("ID mismatch.");
            var success = await _mediator.Send(command);
            return success ? Ok("Parent data successfully updated.") : throw new KeyNotFoundException($"Parent with ID {parentId} does not exist.");
        }

        [HttpDelete("{parentId}")]
        [SwaggerOperation(
            Summary = "Delete a parent",
            Description = "Deletes a parent by their ID."
        )]
        public async Task<IActionResult> Delete(int parentId)
        {
            var success = await _mediator.Send(new DeleteParentCommand(parentId));
            return success ? Ok("Parent data successfully deleted.") : throw new KeyNotFoundException($"Parent with ID {parentId} does not exist.");
        }
    }
}

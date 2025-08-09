using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VolunteerScheduler.Application.Commands.TaskCommandHandler;
using VolunteerScheduler.Application.Queries;
using VolunteerScheduler.Application.Queries.TasksQueries;
using VolunteerScheduler.Domain.Results;

namespace VolunteerScheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteerTasksController : ControllerBase
    {
        private readonly IMediator _mediator;
        public VolunteerTasksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new volunteer task",
            Description = "Creates a volunteer task with the provided details."
        )]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
        { 
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllTasks), new { id }, null);
        }

        [HttpPost("{taskId}/claim")]
        [SwaggerOperation(
            Summary = "Claim a volunteer task by ID",
            Description = "Allows a parent to claim a task if slots are available."
        )]
        public async Task<IActionResult> ClaimTask(int taskId, [FromQuery] int parentId)
        {
            var result = await _mediator.Send(new ClaimTaskCommand(taskId, parentId));

            if (result.IsSuccess)
                return Ok("Task successfully claimed.");

            return result.Status switch
            {
                ClaimTaskStatus.OverlappingTask => Conflict(result.ErrorMessage),
                ClaimTaskStatus.TaskFullyBooked => Conflict(result.ErrorMessage),
                ClaimTaskStatus.AlreadyClaimed => Conflict(result.ErrorMessage),
                ClaimTaskStatus.TaskNotFound => NotFound(result.ErrorMessage),
                ClaimTaskStatus.ParentNotFound => NotFound(result.ErrorMessage),
                _ => StatusCode(500, result.ErrorMessage)
            };
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get available volunteer tasks",
            Description = "Retrieves a list of volunteer tasks that have available slots."
        )]
        public async Task<IActionResult> GetAvailableTasks()
        {
            var slots = await _mediator.Send(new GetAvailableTasksQuery());
            return Ok(slots);
        }

        [HttpGet("all")]
        [SwaggerOperation(
            Summary = "Get all volunteer tasks",
            Description = "Retrieves a list of all volunteer tasks regardless of availability."
        )]
        public async Task<IActionResult> GetAllTasks()
        {
            var slots = await _mediator.Send(new GetAllTasksQuery());
            return Ok(slots);
        }

        [HttpGet("teacher/{teacherId}")]
        [SwaggerOperation(
            Summary = "Get tasks created by a specific teacher",
            Description = "Retrieves volunteer tasks created by the specified teacher."
        )]
        public async Task<IActionResult> GetTasksByTeacher(int teacherId)
        {
            var tasks = await _mediator.Send(new GetTasksByTeacherQuery(teacherId));
            return Ok(tasks);
        }

        [HttpGet("parent/{parentId}")]
        [SwaggerOperation(
            Summary = "Get tasks claimed by a specific parent",
            Description = "Retrieves volunteer tasks claimed by the specified parent."
        )]
        public async Task<IActionResult> GetTasksByParent(int parentId)
        {
            var tasks = await _mediator.Send(new GetTasksByParentQuery(parentId));
            return Ok(tasks);
        }

        [HttpDelete("{taskId}")]
        [SwaggerOperation(
            Summary = "Delete a volunteer task",
            Description = "Deletes a task by ID if the requesting teacher is the creator."
        )]
        public async Task<IActionResult> DeleteTask(int taskId, [FromQuery] int teacherId)
        {
            var success = await _mediator.Send(new DeleteTaskCommand(taskId, teacherId));
            return success ? Ok("Task successfully deleted.") : Forbid("You are not the creator of this task.");
        }

        [HttpPut("update")]
        [SwaggerOperation(
            Summary = "Update a volunteer task",
            Description = "Updates task details if the requesting teacher is the creator."
        )]
        public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskCommand command)
        {
            var success = await _mediator.Send(command);
            return success ? Ok("Task successfully updated.") : Forbid("You are not the creator of this task or input is invalid.");
        }

        [HttpPost("{taskId}/cancel")]
        public async Task<IActionResult> CancelTaskForParent(int taskId, [FromQuery] int parentId)
        {
            var success = await _mediator.Send(new CancelTaskForParentCommand(taskId, parentId));
            if (!success)
                return NotFound(new { message = "Task or parent not found, or parent is not participating." });

            return Ok(new { message = "Task participation cancelled successfully." });
        }
    }
}

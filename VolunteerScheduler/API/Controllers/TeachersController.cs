using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using VolunteerScheduler.Application.Commands.TeacherCommandHandlers;
using VolunteerScheduler.Application.Queries.TeacherQueries;

namespace VolunteerScheduler.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TeachersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a new teacher",
            Description = "Creates a new teacher with the provided details."
        )]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherCommand command)
        {
            var id = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetTeacherById), new { teacherId = id }, null);
        }

        [HttpGet("{teacherId}")]
        [SwaggerOperation(
            Summary = "Get a teacher by ID",
            Description = "Retrieves the teacher details by their unique ID."
        )]
        public async Task<IActionResult> GetTeacherById(int teacherId)
        {
            var teacher = await _mediator.Send(new GetTeacherByIdQuery(teacherId));
            if (teacher == null)
                return NotFound();

            return Ok(teacher);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all teachers",
            Description = "Retrieves a list of all teachers."
        )]
        public async Task<IActionResult> GetAllTeachers()
        {
            var teachers = await _mediator.Send(new GetAllTeachersQuery());
            return Ok(teachers);
        }

        [HttpPut("{teacherId}")]
        [SwaggerOperation(
            Summary = "Update a teacher",
            Description = "Updates teacher details by ID."
        )]
        public async Task<IActionResult> UpdateTeacher(int teacherId, [FromBody] UpdateTeacherCommand command)
        {
            if (teacherId != command.TeacherId)
                return BadRequest("ID mismatch.");

            var success = await _mediator.Send(command);
            return success ? Ok("Teacher data successfully updated.") : NotFound();
        }

        [HttpDelete("{teacherId}")]
        [SwaggerOperation(
            Summary = "Delete a teacher",
            Description = "Deletes a teacher by their ID."
        )]
        public async Task<IActionResult> DeleteTeacher(int teacherId)
        {
            var success = await _mediator.Send(new DeleteTeacherCommand(teacherId));
            return success ? Ok("Teacher data successfully deleted.") : NotFound();
        }
    }
}
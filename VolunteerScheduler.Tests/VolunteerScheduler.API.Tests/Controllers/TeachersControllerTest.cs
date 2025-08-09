using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VolunteerScheduler.API.Controllers;
using VolunteerScheduler.Application.Commands.TeacherCommandHandlers;
using VolunteerScheduler.Application.Queries.TeacherQueries;
using VolunteerScheduler.Domain.Entities;
using Xunit;

namespace VolunteerScheduler.API.Tests.Controllers
{

    public class TeachersControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly TeachersController _controller;

        public TeachersControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new TeachersController(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateTeacher_ShouldReturnCreatedAtAction_WithNewTeacherId()
        {
            var newTeacherId = 5;
            var command = new CreateTeacherCommand("Teacher Name");

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newTeacherId);

            var result = await _controller.CreateTeacher(command);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetTeacherById), createdAtResult.ActionName);
            Assert.Equal(newTeacherId, createdAtResult.RouteValues["teacherId"]);
            Assert.Null(createdAtResult.Value);
        }

        [Fact]
        public async Task GetTeacherById_ShouldReturnOk_WhenTeacherExists()
        {
            var teacherId = 1;
            var teacher = new Teacher { TeacherId = teacherId, Name = "John" };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTeacherByIdQuery>(q => q.Id == teacherId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(teacher);

            var result = await _controller.GetTeacherById(teacherId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTeacher = Assert.IsType<Teacher>(okResult.Value);
            Assert.Equal(teacherId, returnedTeacher.TeacherId);
        }

        [Fact]
        public async Task GetTeacherById_ShouldReturnNotFound_WhenTeacherDoesNotExist()
        {
            var teacherId = 999;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetTeacherByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Teacher?)null);

            var result = await _controller.GetTeacherById(teacherId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllTeachers_ShouldReturnOk_WithListOfTeachers()
        {
            var teachers = new List<Teacher>
        {
            new Teacher { TeacherId = 1, Name = "Teacher A" },
            new Teacher { TeacherId = 2, Name = "Teacher B" }
        };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllTeachersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(teachers);

            var result = await _controller.GetAllTeachers();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTeachers = Assert.IsAssignableFrom<List<Teacher>>(okResult.Value);
            Assert.Equal(teachers.Count, returnedTeachers.Count);
        }

        [Fact]
        public async Task UpdateTeacher_ShouldReturnBadRequest_WhenIdMismatch()
        {
            int routeId = 1;
            var command = new UpdateTeacherCommand(2, "Updated Name");

            var result = await _controller.UpdateTeacher(routeId, command);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch.", badRequest.Value);
        }

        [Fact]
        public async Task UpdateTeacher_ShouldReturnOk_WhenUpdateSucceeds()
        {
            int teacherId = 1;
            var command = new UpdateTeacherCommand(teacherId, "Updated Name");

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateTeacher(teacherId, command);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Teacher data successfully updated.", okResult.Value);
        }

        [Fact]
        public async Task UpdateTeacher_ShouldReturnNotFound_WhenUpdateFails()
        {
            int teacherId = 1;
            var command = new UpdateTeacherCommand(teacherId, "Updated Name");

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateTeacher(teacherId, command);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteTeacher_ShouldReturnOk_WhenDeleteSucceeds()
        {
            int teacherId = 1;

            _mediatorMock
                .Setup(m => m.Send(It.Is<DeleteTeacherCommand>(cmd => cmd.TeacherId == teacherId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.DeleteTeacher(teacherId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Teacher data successfully deleted.", okResult.Value);
        }

        [Fact]
        public async Task DeleteTeacher_ShouldReturnNotFound_WhenDeleteFails()
        {
            int teacherId = 1;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteTeacherCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.DeleteTeacher(teacherId);

            Assert.IsType<NotFoundResult>(result);
        }
    }

}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VolunteerScheduler.API.Controllers;
using VolunteerScheduler.Application.Commands.TaskCommandHandler;
using VolunteerScheduler.Application.Queries;
using VolunteerScheduler.Application.Queries.TasksQueries;
using VolunteerScheduler.Domain.Results;
using VolunteerScheduler.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace VolunteerScheduler.API.Tests.Controllers
{

    public class VolunteerTasksControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly VolunteerTasksController _controller;

        public VolunteerTasksControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new VolunteerTasksController(_mediatorMock.Object);
        }

        [Fact]
        public async Task CreateTask_ReturnsCreatedAtAction_WithId()
        {
            var newTaskId = 123;
            var command = new CreateTaskCommand("title", System.DateTime.Now, System.DateTime.Now.AddHours(1), 5, 1);

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(newTaskId);

            var result = await _controller.CreateTask(command);

            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAllTasks), createdAtResult.ActionName);
            Assert.Equal(newTaskId, createdAtResult.RouteValues["id"]);
            Assert.Null(createdAtResult.Value);
        }

        [Theory]
        [InlineData(true, ClaimTaskStatus.Success)]
        [InlineData(false, ClaimTaskStatus.TaskFullyBooked)]
        [InlineData(false, ClaimTaskStatus.OverlappingTask)]
        [InlineData(false, ClaimTaskStatus.AlreadyClaimed)]
        [InlineData(false, ClaimTaskStatus.TaskNotFound)]
        [InlineData(false, ClaimTaskStatus.ParentNotFound)]
        [InlineData(false, ClaimTaskStatus.Error)]
        public async Task ClaimTask_ReturnsExpectedResult(bool isSuccess, ClaimTaskStatus status)
        {
            var taskId = 1;
            var parentId = 10;

            ClaimTaskResult result = isSuccess
                ? ClaimTaskResult.Success()
                : ClaimTaskResult.Failure(status, "Error message");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<ClaimTaskCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);

            var response = await _controller.ClaimTask(taskId, parentId);

            if (isSuccess)
            {
                var okResult = Assert.IsType<OkObjectResult>(response);
                Assert.Equal("Task successfully claimed.", okResult.Value);
            }
            else
            {
                switch (status)
                {
                    case ClaimTaskStatus.TaskFullyBooked:
                    case ClaimTaskStatus.OverlappingTask:
                    case ClaimTaskStatus.AlreadyClaimed:
                        var conflictResult = Assert.IsType<ConflictObjectResult>(response);
                        Assert.Equal("Error message", conflictResult.Value);
                        break;

                    case ClaimTaskStatus.TaskNotFound:
                    case ClaimTaskStatus.ParentNotFound:
                        var notFoundResult = Assert.IsType<NotFoundObjectResult>(response);
                        Assert.Equal("Error message", notFoundResult.Value);
                        break;

                    default:
                        var objectResult = Assert.IsType<ObjectResult>(response);
                        Assert.Equal(500, objectResult.StatusCode);
                        Assert.Equal("Error message", objectResult.Value);
                        break;
                }
            }
        }

        [Fact]
        public async Task GetAvailableTasks_ReturnsOkWithTasks()
        {
            var tasks = new List<VolunteerTask> {
            new VolunteerTask { Id = 1, Title = "Task 1" },
            new VolunteerTask { Id = 2, Title = "Task 2" }
        };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAvailableTasksQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            var result = await _controller.GetAvailableTasks();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<VolunteerTask>>(okResult.Value);
            Assert.Equal(tasks.Count, ((List<VolunteerTask>)returnedTasks).Count);
        }

        [Fact]
        public async Task GetAllTasks_ReturnsOkWithTasks()
        {
            var tasks = new List<VolunteerTask> {
            new VolunteerTask { Id = 1, Title = "Task 1" },
            new VolunteerTask { Id = 2, Title = "Task 2" }
        };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllTasksQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            var result = await _controller.GetAllTasks();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<VolunteerTask>>(okResult.Value);
            Assert.Equal(tasks.Count, ((List<VolunteerTask>)returnedTasks).Count);
        }

        [Fact]
        public async Task GetTasksByTeacher_ReturnsOkWithTasks()
        {
            var teacherId = 1;
            var tasks = new List<VolunteerTask> {
            new VolunteerTask { Id = 1, Title = "Task 1", CreatedByTeacherId = teacherId },
        };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTasksByTeacherQuery>(q => q.TeacherId == teacherId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            var result = await _controller.GetTasksByTeacher(teacherId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<VolunteerTask>>(okResult.Value);
            Assert.Single(returnedTasks);
        }

        [Fact]
        public async Task GetTasksByParent_ReturnsOkWithTasks()
        {
            var parentId = 2;
            var tasks = new List<VolunteerTask> {
            new VolunteerTask { Id = 1, Title = "Task 1", ParticipatingParents = new List<int> { parentId } },
        };

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetTasksByParentQuery>(q => q.ParentId == parentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tasks);

            var result = await _controller.GetTasksByParent(parentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<VolunteerTask>>(okResult.Value);
            Assert.Single(returnedTasks);
        }

        [Fact]
        public async Task DeleteTask_ReturnsOk_WhenSuccess()
        {
            int taskId = 1, teacherId = 10;

            _mediatorMock
                .Setup(m => m.Send(It.Is<DeleteTaskCommand>(cmd => cmd.TaskId == taskId && cmd.RequestedByTeacherId == teacherId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.DeleteTask(taskId, teacherId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task successfully deleted.", okResult.Value);
        }

        [Fact]
        public async Task DeleteTask_ReturnsForbid_WhenFailure()
        {
            int taskId = 1, teacherId = 10;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.DeleteTask(taskId, teacherId);

            var forbidResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateTask_ReturnsOk_WhenSuccess()
        {
            var command = new UpdateTaskCommand(1, "Updated", System.DateTime.Now, System.DateTime.Now.AddHours(1), 5, 10);

            _mediatorMock
                .Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.UpdateTask(command);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task successfully updated.", okResult.Value);
        }

        [Fact]
        public async Task UpdateTask_ReturnsForbid_WhenFailure()
        {
            var command = new UpdateTaskCommand(1, "Updated", System.DateTime.Now, System.DateTime.Now.AddHours(1), 5, 10);

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.UpdateTask(command);

            var forbidResult = Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CancelTaskForParent_ReturnsOk_WhenSuccess()
        {
            int taskId = 1, parentId = 10;

            _mediatorMock
                .Setup(m => m.Send(It.Is<CancelTaskForParentCommand>(cmd => cmd.TaskId == taskId && cmd.ParentId == parentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.CancelTaskForParent(taskId, parentId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            Assert.Equal("Task participation cancelled successfully.", dict["message"]);
        }

        [Fact]
        public async Task CancelTaskForParent_ReturnsNotFound_WhenFailure()
        {
            int taskId = 1, parentId = 10;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CancelTaskForParentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.CancelTaskForParent(taskId, parentId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            var json = System.Text.Json.JsonSerializer.Serialize(notFoundResult.Value);
            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.Equal("Task or parent not found, or parent is not participating.", dict["message"]);
        }
    }

}

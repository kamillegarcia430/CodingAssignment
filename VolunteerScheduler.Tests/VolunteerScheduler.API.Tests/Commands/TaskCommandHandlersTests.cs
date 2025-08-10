using Moq;
using VolunteerScheduler.Application.Commands.TaskCommandHandler;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Domain.Results;

namespace VolunteerScheduler.API.Tests.Commands
{
    public class TaskCommandHandlersTests
    {
        private readonly CancellationToken _ct = CancellationToken.None;

        #region CancelTaskCommandHandlers
        [Fact]
        public async Task CancelTaskForParent_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            taskRepoMock
                .Setup(r => r.CancelTaskForParentAsync(1, 2))
                .ReturnsAsync(true);

            var handler = new CancelTaskForParentCommandHandler(taskRepoMock.Object);

            var result = await handler.Handle(new CancelTaskForParentCommand(1, 2), _ct);

            Assert.True(result);
            taskRepoMock.Verify(r => r.CancelTaskForParentAsync(1, 2), Times.Once);
        }

        [Fact]
        public async Task CancelTaskForParent_ShouldReturnFalse_WhenRepositoryReturnsFalse()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            taskRepoMock
                .Setup(r => r.CancelTaskForParentAsync(1, 2))
                .ReturnsAsync(false);

            var handler = new CancelTaskForParentCommandHandler(taskRepoMock.Object);

            var result = await handler.Handle(new CancelTaskForParentCommand(1, 2), _ct);

            Assert.False(result);
        }

        #endregion

        #region ClaimTaskCommandHandlers
        [Fact]
        public async Task ClaimTask_ShouldReturnRepoResult()
        {
            var expectedResult = ClaimTaskResult.Success();

            var taskRepoMock = new Mock<ITaskRepository>();
            var parentRepoMock = new Mock<IParentRepository>();

            taskRepoMock
                .Setup(r => r.TryClaimTaskAsync(10, 20))
                .ReturnsAsync(expectedResult);

            var handler = new ClaimTaskCommandHandler(taskRepoMock.Object, parentRepoMock.Object);

            var result = await handler.Handle(new ClaimTaskCommand(10, 20), _ct);

            // Option 2 (xUnit 2.4.2+): structure comparison
            Assert.Equivalent(expectedResult, result);

            taskRepoMock.Verify(r => r.TryClaimTaskAsync(10, 20), Times.Once);
        }

        #endregion

        #region CreateTaskCommandHandler
        [Fact]
        public async Task CreateTask_ShouldThrow_WhenStartIsAfterEnd()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            var handler = new CreateTaskCommandHandler(taskRepoMock.Object);

            var cmd = new CreateTaskCommand("Test", DateTime.Now.AddHours(2), DateTime.Now, 3, 1);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(cmd, _ct));
            taskRepoMock.Verify(r => r.AddAsync(It.IsAny<VolunteerTask>(), _ct), Times.Never);
        }

        [Fact]
        public async Task CreateTask_ShouldThrow_WhenStartIsLessThanCurrentTime()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            var handler = new CreateTaskCommandHandler(taskRepoMock.Object);

            var cmd = new CreateTaskCommand("Test", DateTime.Now.AddHours(-2), DateTime.Now, 3, 1);

            await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(cmd, _ct));
            taskRepoMock.Verify(r => r.AddAsync(It.IsAny<VolunteerTask>(), _ct), Times.Never);
        }

        [Fact]
        public async Task CreateTask_ShouldAddTaskAndReturnId()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            taskRepoMock
                .Setup(r => r.AddAsync(It.IsAny<VolunteerTask>(), _ct))
                .Callback<VolunteerTask, CancellationToken>((task, _) => task.Id = 99)
                .Returns(Task.CompletedTask);

            var handler = new CreateTaskCommandHandler(taskRepoMock.Object);

            var cmd = new CreateTaskCommand("Title", DateTime.Now.AddHours(1), DateTime.Now.AddHours(2), 3, 1);
            var id = await handler.Handle(cmd, _ct);

            Assert.Equal(99, id);
            taskRepoMock.Verify(r => r.AddAsync(It.Is<VolunteerTask>(t =>
                t.Title == "Title" &&
                t.NumberOfAvailableSlots == 3 &&
                t.CreatedByTeacherId == 1
            ), _ct), Times.Once);
        }

        #endregion

        #region DeleteTaskCommandHandler

        [Fact]
        public async Task DeleteTask_ShouldThrow_WhenTaskNotFound()
        {
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((VolunteerTask)null);

            var handler = new DeleteTaskCommandHandler(repoMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new DeleteTaskCommand(1, 1), _ct));
        }

        [Fact]
        public async Task DeleteTask_ShouldThrow_WhenTeacherIsNotCreator()
        {
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new VolunteerTask { Id = 1, CreatedByTeacherId = 99 });

            var handler = new DeleteTaskCommandHandler(repoMock.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new DeleteTaskCommand(1, 1), _ct));
        }

        [Fact]
        public async Task DeleteTask_ShouldCallDelete_WhenAuthorized()
        {
            var task = new VolunteerTask { Id = 1, CreatedByTeacherId = 1 };
            var repoMock = new Mock<ITaskRepository>();
            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            var handler = new DeleteTaskCommandHandler(repoMock.Object);
            var result = await handler.Handle(new DeleteTaskCommand(1, 1), _ct);

            Assert.True(result);
            repoMock.Verify(r => r.DeleteAsync(task, _ct), Times.Once);
        }

        #endregion

        #region UpdateTaskCommandHandler

        [Fact]
        public async Task UpdateTask_ShouldThrow_WhenTeacherNotFound()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            var teacherRepoMock = new Mock<ITeacherRepository>();
            teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Teacher)null);

            var handler = new UpdateTaskCommandHandler(taskRepoMock.Object, teacherRepoMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new UpdateTaskCommand(1, "Title", DateTime.Now, DateTime.Now.AddHours(1), 3, 1), _ct));
        }

        [Fact]
        public async Task UpdateTask_ShouldThrow_WhenTaskNotFound()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            var teacherRepoMock = new Mock<ITeacherRepository>();
            teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher());

            taskRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((VolunteerTask)null);

            var handler = new UpdateTaskCommandHandler(taskRepoMock.Object, teacherRepoMock.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(new UpdateTaskCommand(1, "Title", DateTime.Now, DateTime.Now.AddHours(1), 3, 1), _ct));
        }

        [Fact]
        public async Task UpdateTask_ShouldThrow_WhenNotOwner()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            var teacherRepoMock = new Mock<ITeacherRepository>();

            teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher());
            taskRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new VolunteerTask { CreatedByTeacherId = 2 });

            var handler = new UpdateTaskCommandHandler(taskRepoMock.Object, teacherRepoMock.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                handler.Handle(new UpdateTaskCommand(1, "Title", DateTime.Now, DateTime.Now.AddHours(1), 3, 1), _ct));
        }

        [Fact]
        public async Task UpdateTask_ShouldThrow_WhenInvalidTimeRange()
        {
            var taskRepoMock = new Mock<ITaskRepository>();
            var teacherRepoMock = new Mock<ITeacherRepository>();

            teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher());
            taskRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new VolunteerTask { CreatedByTeacherId = 1 });

            var handler = new UpdateTaskCommandHandler(taskRepoMock.Object, teacherRepoMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                handler.Handle(new UpdateTaskCommand(1, "Title", DateTime.Now, DateTime.Now.AddHours(-1), 3, 1), _ct));
        }

        [Fact]
        public async Task UpdateTask_ShouldUpdate_WhenValid()
        {
            var task = new VolunteerTask { CreatedByTeacherId = 1 };
            var taskRepoMock = new Mock<ITaskRepository>();
            var teacherRepoMock = new Mock<ITeacherRepository>();

            teacherRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Teacher());
            taskRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(task);

            var handler = new UpdateTaskCommandHandler(taskRepoMock.Object, teacherRepoMock.Object);
            var cmd = new UpdateTaskCommand(1, "NewTitle", DateTime.Now, DateTime.Now.AddHours(1), 5, 1);

            var result = await handler.Handle(cmd, _ct);

            Assert.True(result);
            Assert.Equal("NewTitle", task.Title);
            Assert.Equal(5, task.NumberOfAvailableSlots);
            taskRepoMock.Verify(r => r.UpdateAsync(task, _ct), Times.Once);
        }

        #endregion
    }
}

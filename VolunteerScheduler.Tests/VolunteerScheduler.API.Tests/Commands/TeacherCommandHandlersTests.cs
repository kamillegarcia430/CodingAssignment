using Moq;
using VolunteerScheduler.Application.Commands.TeacherCommandHandlers;
using VolunteerScheduler.Application.Interfaces;
using TeacherEntity = VolunteerScheduler.Domain.Entities.Teacher;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.API.Tests.Commands
{ 
    public class TeacherCommandHandlersTests
    {
        private readonly Mock<ITeacherRepository> _mockRepo;

        public TeacherCommandHandlersTests()
        {
            _mockRepo = new Mock<ITeacherRepository>();
        }

        #region CreateTeacherCommandHandler Tests
        [Fact]
        public async Task CreateTeacherCommandHandler_Should_Add_Teacher_And_Return_Id()
        {
            // Arrange
            var handler = new CreateTeacherCommandHandler(_mockRepo.Object);
            var command = new CreateTeacherCommand("Ms. Lopez");
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<TeacherEntity>(), It.IsAny<CancellationToken>()))
                .Callback<TeacherEntity, CancellationToken>((t, ct) => t.TeacherId = 123) // Simulate DB generated Id
                .Returns(Task.CompletedTask);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(123, result);
            _mockRepo.Verify(r => r.AddAsync(It.Is<TeacherEntity>(t => t.Name == "Ms. Lopez"), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region DeleteTeacherCommandHandler Tests

        [Fact]
        public async Task DeleteTeacherCommandHandler_Should_Delete_Teacher_When_Exists_And_No_Ongoing_Tasks()
        {
            // Arrange
            var teacher = new TeacherEntity { TeacherId = 1, Name = "Mr. Smith", CreatedTasks = new List<VolunteerTask>() };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(teacher);
            _mockRepo.Setup(r => r.DeleteAsync(teacher, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new DeleteTeacherCommandHandler(_mockRepo.Object);
            var command = new DeleteTeacherCommand(1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.GetByIdAsync(1), Times.Once);
            _mockRepo.Verify(r => r.DeleteAsync(teacher, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTeacherCommandHandler_Should_Throw_When_Teacher_Not_Found()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TeacherEntity?)null);
            var handler = new DeleteTeacherCommandHandler(_mockRepo.Object);
            var command = new DeleteTeacherCommand(99);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Teacher with ID 99 does not exist", ex.Message);
        }

        [Fact]
        public async Task DeleteTeacherCommandHandler_Should_Throw_When_Teacher_Has_Ongoing_Tasks()
        {
            // Arrange
            var teacher = new TeacherEntity { TeacherId = 2, Name = "Ms. Reed", CreatedTasks = new List<VolunteerTask> { new VolunteerTask() } };
            _mockRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(teacher);

            var handler = new DeleteTeacherCommandHandler(_mockRepo.Object);
            var command = new DeleteTeacherCommand(2);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Teacher still has ongoing tasks", ex.Message);
        }

        #endregion

        #region UpdateTeacherCommandHandler Tests

        [Fact]
        public async Task UpdateTeacherCommandHandler_Should_Update_Teacher_When_Exists()
        {
            // Arrange
            var teacher = new TeacherEntity { TeacherId = 3, Name = "Old Name" };
            _mockRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(teacher);
            _mockRepo.Setup(r => r.UpdateAsync(teacher, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var handler = new UpdateTeacherCommandHandler(_mockRepo.Object);
            var command = new UpdateTeacherCommand(3, "New Name");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal("New Name", teacher.Name);
            _mockRepo.Verify(r => r.UpdateAsync(teacher, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTeacherCommandHandler_Should_Throw_When_Teacher_Not_Found()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TeacherEntity?)null);
            var handler = new UpdateTeacherCommandHandler(_mockRepo.Object);
            var command = new UpdateTeacherCommand(999, "Name");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("Teacher with ID 999 does not exist", ex.Message);
        }

        #endregion
    }
}

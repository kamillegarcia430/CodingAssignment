using Moq;
using VolunteerScheduler.Application.Commands.ParentCommandHandlers;
using VolunteerScheduler.Application.Interfaces;
using ParentEntity = VolunteerScheduler.Domain.Entities.Parent;

namespace VolunteerScheduler.API.Tests.Commands
{
    public class ParentCommandHandlersTests
    {
        private readonly Mock<IParentRepository> _mockRepo;

        public ParentCommandHandlersTests()
        {
            _mockRepo = new Mock<IParentRepository>();
        }

        #region CreateParentCommandHandlerTests 
        [Fact]
        public async Task CreateParentCommandHandler_ShouldAddParentAndReturnId()
        {
            // Arrange
            var handler = new CreateParentCommandHandler(_mockRepo.Object);
            var command = new CreateParentCommand("Test Parent");

            ParentEntity savedParent = null;
            _mockRepo
                .Setup(repo => repo.AddAsync(It.IsAny<ParentEntity>()))
                .Returns<ParentEntity>(parent =>
                {
                    // Simulate EF assigning the ParentId when saved
                    parent.ParentId = 123;
                    savedParent = parent;
                    return Task.CompletedTask;
                });

            // Act
            var resultId = await handler.Handle(command, CancellationToken.None);

            // Assert
            _mockRepo.Verify(repo => repo.AddAsync(It.IsAny<ParentEntity>()), Times.Once);

            Assert.NotNull(savedParent);
            Assert.Equal("Test Parent", savedParent.Name);
            Assert.Equal(123, resultId);
        }

        [Fact]
        public async Task CreateParentCommandHandler_Should_Throw_When_Name_Is_Invalid()
        {
            // Arrange
            var handler = new CreateParentCommandHandler(_mockRepo.Object);
            var invalidNames = new[] { null, "", "  " };

            foreach (var invalidName in invalidNames)
            {
                var command = new CreateParentCommand(invalidName!);

                // Act & Assert
                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await handler.Handle(command, CancellationToken.None));
            }
        }

        #endregion

        #region UpdateParentCommandHandlerTests

        [Fact]
        public async Task UpdateParentCommandHandler_ExistingParent_UpdatesAndReturnsTrue()
        {
            // Arrange
            var handler = new UpdateParentCommandHandler(_mockRepo.Object);
            var parentId = 1;
            var existingParent = new ParentEntity { ParentId = parentId, Name = "Old Name" };
            var newName = "New Name";

            _mockRepo.Setup(r => r.GetByIdAsync(parentId))
                     .ReturnsAsync(existingParent);
            _mockRepo.Setup(r => r.UpdateAsync(existingParent))
                     .Returns(Task.CompletedTask);

            var command = new UpdateParentCommand(parentId, newName);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            Assert.Equal(newName, existingParent.Name);
            _mockRepo.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockRepo.Verify(r => r.UpdateAsync(existingParent), Times.Once);
        }

        [Fact]
        public async Task UpdateParentCommandHandler_NonExistingParent_ThrowsKeyNotFoundException()
        {
            // Arrange
            var handler = new UpdateParentCommandHandler(_mockRepo.Object);
            var parentId = 99;
            var newName = "New Name";

            _mockRepo.Setup(r => r.GetByIdAsync(parentId))
                     .ReturnsAsync((ParentEntity?)null);

            var command = new UpdateParentCommand(parentId, newName);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal($"Parent with ID {parentId} does not exist.", ex.Message);
            _mockRepo.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<ParentEntity>()), Times.Never);
        }

        [Fact]
        public async Task UpdateParentCommandHandler_UpdateAsync_ThrowsException_Propagates()
        {
            // Arrange
            var handler = new UpdateParentCommandHandler(_mockRepo.Object);
            var parentId = 1;
            var existingParent = new ParentEntity { ParentId = parentId, Name = "Old Name" };
            var newName = "New Name";

            _mockRepo.Setup(r => r.GetByIdAsync(parentId))
                     .ReturnsAsync(existingParent);

            _mockRepo.Setup(r => r.UpdateAsync(existingParent))
                     .ThrowsAsync(new Exception("Database error"));

            var command = new UpdateParentCommand(parentId, newName);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal("Database error", ex.Message);
            _mockRepo.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockRepo.Verify(r => r.UpdateAsync(existingParent), Times.Once);
        }
        #endregion

        #region DeleteParentCommandHandlerTests

        [Fact]
        public async Task DeleteParentCommandHandler_ExistingParent_DeletesAndReturnsTrue()
        {
            // Arrange
            var handler = new DeleteParentCommandHandler(_mockRepo.Object);
            var parentId = 1;
            var parent = new ParentEntity { ParentId = parentId, Name = "Test Parent" };

            _mockRepo.Setup(r => r.GetByIdAsync(parentId))
                     .ReturnsAsync(parent);
            _mockRepo.Setup(r => r.DeleteAsync(parent))
                     .Returns(Task.CompletedTask);

            var command = new DeleteParentCommand(parentId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _mockRepo.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockRepo.Verify(r => r.DeleteAsync(parent), Times.Once);
        }

        [Fact]
        public async Task DeleteParentCommandHandler_NonExistingParent_ThrowsKeyNotFoundException()
        {
            // Arrange
            var handler = new DeleteParentCommandHandler(_mockRepo.Object);
            var parentId = 99;

            _mockRepo.Setup(r => r.GetByIdAsync(parentId))
                     .ReturnsAsync((ParentEntity?)null);

            var command = new DeleteParentCommand(parentId);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                handler.Handle(command, CancellationToken.None));

            Assert.Equal($"Parent with ID {parentId} does not exist.", ex.Message);
            _mockRepo.Verify(r => r.GetByIdAsync(parentId), Times.Once);
            _mockRepo.Verify(r => r.DeleteAsync(It.IsAny<ParentEntity>()), Times.Never);
        }
        #endregion
    }
}

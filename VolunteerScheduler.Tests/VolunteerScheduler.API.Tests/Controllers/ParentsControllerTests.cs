using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using VolunteerScheduler.API.Controllers;
using VolunteerScheduler.Application.Commands.ParentCommandHandlers;
using VolunteerScheduler.Application.Queries.ParentQueries;
using VolunteerScheduler.Domain.Entities;
using Moq.Language.Flow;

namespace VolunteerScheduler.API.Tests.Controllers
{

    public class ParentsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ParentsController _controller;

        public ParentsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ParentsController(_mediatorMock.Object);
        }

        [Fact]
        public async Task Create_ShouldReturnCreatedAtAction_WithNewId()
        {
            // Arrange
            var newParentId = 123;
            var command = new CreateParentCommand("John Doe");

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<CreateParentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newParentId);

            // Act
            var result = await _controller.Create(command);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);
            Assert.Equal(newParentId, createdAtResult.RouteValues["parentId"]);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOk_WithListOfParents()
        {
            // Arrange
            var parents = new List<Parent>
            {
                new Parent { ParentId = 1, Name = "Parent 1" },
                new Parent { ParentId = 2, Name = "Parent 2" }
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllParentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parents);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedParents = Assert.IsAssignableFrom<List<Parent>>(okResult.Value);
            Assert.Equal(parents.Count, returnedParents.Count);
        }

        [Fact]
        public async Task Create_ShouldThrowArgumentNullException_WhenCommandIsNull()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.Create(null!));
            Assert.Contains("Input is null", ex.Message);
        }

        [Fact]
        public async Task GetAll_ShouldReturnOkWithParents()
        {
            // Arrange
            var mediatorMock = new Mock<IMediator>();
            var expectedParents = new List<Parent>
            {
                new Parent { ParentId = 1, Name = "Parent 1" },
                new Parent { ParentId = 2, Name = "Parent 2" }
            };

            mediatorMock
                .Setup(m => m.Send(It.IsAny<GetAllParentsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedParents);

            var controller = new ParentsController(mediatorMock.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualParents = Assert.IsAssignableFrom<List<Parent>>(okResult.Value);
            Assert.Equal(expectedParents.Count, actualParents.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenParentFound()
        {

            // Arrange
            var parentId = 1;
            var parent = new Parent { ParentId = parentId, Name = "Parent 1" };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetParentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(parent);

            // Act
            var result = await _controller.GetById(parentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedParent = Assert.IsType<Parent>(okResult.Value);
            Assert.Equal(parentId, returnedParent.ParentId);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenParentNotFound()
        {
            // Arrange
            var parentId = 99;

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetParentByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Parent?)null);

            // Act
            var result = await _controller.GetById(parentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
        {
            // Arrange
            var parentId = 1;
            var command = new UpdateParentCommand(2, "New Name");

            // Act
            var result = await _controller.Update(parentId, command);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ID mismatch.", badRequest.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnOk_WhenUpdateSucceeds()
        {
            // Arrange
            var parentId = 1;
            var command = new UpdateParentCommand(parentId, "New Name");
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            // Act
            var result = await _controller.Update(parentId, command);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Parent data successfully updated.", okResult.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_WhenUpdateFails()
        {
            // Arrange
            var parentId = 1;
            var command = new UpdateParentCommand(parentId, "New Name");
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

            // Act
            var result = await _controller.Update(parentId, command);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenDeleteSucceeds()
        {
            // Arrange
            var parentId = 1;
            _mediatorMock.Setup(m => m.Send(It.Is<DeleteParentCommand>(c => c.ParentId == parentId), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(parentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Parent data successfully deleted.", okResult.Value);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_WhenDeleteFails()
        {
            // Arrange
            var parentId = 1;
            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteParentCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(false);

            // Act
            var result = await _controller.Delete(parentId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

}

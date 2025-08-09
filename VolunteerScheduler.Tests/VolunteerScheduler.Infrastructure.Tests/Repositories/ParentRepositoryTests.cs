using Microsoft.EntityFrameworkCore;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Infrastructure.Data;
using VolunteerScheduler.Infrastructure.Repositories;
using TeacherEntity = VolunteerScheduler.Domain.Entities.Teacher;
using ParentEntity = VolunteerScheduler.Domain.Entities.Parent;

namespace VolunteerScheduler.Infrastructure.Tests.Repositories
{
    public class ParentRepositoryTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllParentsWithClaimedTasks()
        {
            // Arrange
            var context = CreateDbContext(nameof(GetAllAsync_ShouldReturnAllParentsWithClaimedTasks));
            var parent1 = new Parent { ParentId = 1, Name = "Alice" };
            var parent2 = new Parent { ParentId = 2, Name = "Bob" };
            context.Parents.AddRange(parent1, parent2);
            await context.SaveChangesAsync();

            var repo = new ParentRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Name == "Alice");
            Assert.Contains(result, p => p.Name == "Bob");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnParent_WhenExists()
        {
            var context = CreateDbContext(nameof(GetByIdAsync_ShouldReturnParent_WhenExists));
            var parent = new Parent { ParentId = 1, Name = "Charlie" };
            context.Parents.Add(parent);
            await context.SaveChangesAsync();

            var repo = new ParentRepository(context);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Charlie", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            var context = CreateDbContext(nameof(GetByIdAsync_ShouldReturnNull_WhenNotFound));
            var repo = new ParentRepository(context);

            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddParent()
        {
            var context = CreateDbContext(nameof(AddAsync_ShouldAddParent));
            var repo = new ParentRepository(context);

            var parent = new Parent { ParentId = 1, Name = "Dana" };

            await repo.AddAsync(parent);

            var savedParent = await context.Parents.FindAsync(1);
            Assert.NotNull(savedParent);
            Assert.Equal("Dana", savedParent.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateParent()
        {
            var context = CreateDbContext(nameof(UpdateAsync_ShouldUpdateParent));
            var parent = new Parent { ParentId = 1, Name = "Eve" };
            context.Parents.Add(parent);
            await context.SaveChangesAsync();

            var repo = new ParentRepository(context);
            parent.Name = "Eve Updated";

            await repo.UpdateAsync(parent);

            var updatedParent = await context.Parents.FindAsync(1);
            Assert.Equal("Eve Updated", updatedParent.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveParent_WhenExists()
        {
            var context = CreateDbContext(nameof(DeleteAsync_ShouldRemoveParent_WhenExists));

            var task = new VolunteerTask
            {
                Id = 1,
                Title = "Clean School",
                ParticipatingParents = new List<int>()
            };

            var parent = new Parent { ParentId = 1, Name = "Frank", ClaimedTasks = new List<VolunteerTask> { task } };
            task.ParticipatingParents.Add(parent.ParentId);

            context.Parents.Add(parent);
            context.VolunteerTasks.Add(task);
            await context.SaveChangesAsync();

            var repo = new ParentRepository(context);

            await repo.DeleteAsync(parent);

            Assert.Null(await context.Parents.FindAsync(1));
            Assert.Empty(task.ParticipatingParents);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenParentNotFound()
        {
            var context = CreateDbContext(nameof(DeleteAsync_ShouldThrow_WhenParentNotFound));
            var repo = new ParentRepository(context);

            var fakeParent = new Parent { ParentId = 999, Name = "Ghost" };

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.DeleteAsync(fakeParent));
            Assert.Contains("does not exist", ex.Message);
        }
    }
}

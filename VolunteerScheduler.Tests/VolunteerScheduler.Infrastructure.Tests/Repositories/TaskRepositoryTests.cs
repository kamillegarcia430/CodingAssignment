using Microsoft.EntityFrameworkCore;      // For DbContext, UseNpgsql/UseSqlServer, SaveChangesAsync, etc.
using Microsoft.Extensions.DependencyInjection; // For ServiceCollection, AddDbContext, BuildServiceProvider
using Microsoft.EntityFrameworkCore.Diagnostics;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Domain.Results;
using VolunteerScheduler.Infrastructure.Data;
using VolunteerScheduler.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;

namespace VolunteerScheduler.Infrastructure.Tests.Repositories
{
    public class TaskRepositoryTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }


        #region AddAsync

        [Fact]
        public async Task AddAsync_ShouldAddTask_AndLinkToTeacher()
        {
            // Arrange
            var db = GetDbContext();
            var teacher = new Teacher { TeacherId = 1, Name = "Mr. Garcia", CreatedTasks = new List<VolunteerTask>() };
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var task = new VolunteerTask { Title = "Test Task", CreatedByTeacherId = 1 };

            // Act
            await repo.AddAsync(task, CancellationToken.None);

            // Assert
            var savedTask = await db.VolunteerTasks.FirstOrDefaultAsync();
            Assert.NotNull(savedTask);
            Assert.Single(teacher.CreatedTasks);
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenTeacherNotFound()
        {
            var db = GetDbContext();
            var repo = new TaskRepository(db);
            var task = new VolunteerTask { Title = "Test Task", CreatedByTeacherId = 99 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.AddAsync(task, CancellationToken.None));
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTask()
        {
            var db = GetDbContext();
            var task = new VolunteerTask { Id = 1, Title = "Task" };
            db.VolunteerTasks.Add(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.GetByIdAsync(1);

            Assert.Equal("Task", result?.Title);
        }

        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_ShouldRemoveTask()
        {
            var db = GetDbContext();
            var task = new VolunteerTask { Id = 1 , Title = "Sample Volunteer Task"};
            db.VolunteerTasks.Add(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            await repo.DeleteAsync(task, CancellationToken.None);

            Assert.Empty(db.VolunteerTasks);
        }
        #endregion

        #region GetAvailableTasksAsync
        [Fact]
        public async Task GetAvailableTasksAsync_ShouldReturnOnlyTasksWithSlots()
        {
            var db = GetDbContext();
            db.VolunteerTasks.AddRange(
                new VolunteerTask { Title = "Task 1", NumberOfAvailableSlots = 2 },
                new VolunteerTask { Title = "Task 2", NumberOfAvailableSlots = 0 }
            );
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.GetAvailableTasksAsync();

            Assert.Single(result);
            Assert.True(result[0].NumberOfAvailableSlots > 0);
        }

        #endregion

        #region GetParentTasksAsync

        [Fact]
        public async Task GetParentTasksAsync_ShouldReturnTasksForParent()
        {
            var db = GetDbContext();
            db.VolunteerTasks.Add(new VolunteerTask { Title = "Task 1", ParticipatingParents = new List<int> { 10 } });
            db.VolunteerTasks.Add(new VolunteerTask { Title = "Task 2", ParticipatingParents = new List<int> { 20 } });
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.GetParentTasksAsync(10);

            Assert.Single(result);
        }

        #endregion

        #region TryClaimTaskAsync

        [Fact]
        public async Task TryClaimTaskAsync_ShouldSucceed_WhenValid()
        {
            var db = GetDbContext();
            var task = new VolunteerTask
            {
                Id = 1,
                Title = "Task 1",
                NumberOfAvailableSlots = 2,
                ParticipatingParents = new List<int>()
            };
            var parent = new Parent { Name = "Mrs. Kim", ParentId = 5, ClaimedTasks = new List<VolunteerTask>() };
            db.VolunteerTasks.Add(task);
            db.Parents.Add(parent);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.TryClaimTaskAsync(1, 5);

            Assert.Equal(ClaimTaskStatus.Success, result.Status);
            Assert.Contains(5, task.ParticipatingParents);
            Assert.Single(parent.ClaimedTasks);
        }

        [Fact]
        public async Task TryClaimTaskAsync_ShouldFail_WhenTaskNotFound()
        {
            var db = GetDbContext();
            var repo = new TaskRepository(db);

            var result = await repo.TryClaimTaskAsync(999, 1);

            Assert.Equal(ClaimTaskStatus.TaskNotFound, result.Status);
        }

        [Fact]
        public async Task TryClaimTaskAsync_ShouldFail_WhenTaskFullyBooked()
        {
            var db = GetDbContext();
            var task = new VolunteerTask { Id = 1, Title = "Task 1", NumberOfAvailableSlots = 0, ParticipatingParents = new List<int>() };
            db.VolunteerTasks.Add(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.TryClaimTaskAsync(1, 1);

            Assert.Equal(ClaimTaskStatus.TaskFullyBooked, result.Status);
        }

        [Fact]
        public async Task TryClaimTaskAsync_ShouldFail_WhenAlreadyClaimed()
        {
            var db = GetDbContext();
            var task = new VolunteerTask { Id = 1, Title = "Task 1", NumberOfAvailableSlots = 2, ParticipatingParents = new List<int> { 1 } };
            db.VolunteerTasks.Add(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.TryClaimTaskAsync(1, 1);

            Assert.Equal(ClaimTaskStatus.AlreadyClaimed, result.Status);
        }

        [Fact]
        public async Task TryClaimTaskAsync_ShouldFail_WhenParentNotFound()
        {
            var db = GetDbContext();
            var task = new VolunteerTask
            {
                Id = 1,
                Title = "Task 1",
                NumberOfAvailableSlots = 2,
                ParticipatingParents = new List<int>()
            };
            db.VolunteerTasks.Add(task);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.TryClaimTaskAsync(1, 10);

            Assert.Equal(ClaimTaskStatus.ParentNotFound, result.Status);
        }

        [Fact]
        public async Task TryClaimTaskAsync_Should_Fail_When_Task_Overlaps()
        {
            // Arrange
            var db = GetDbContext();

            var parent = new Parent
            {
                ParentId = 1,
                Name = "John",
                ClaimedTasks = new List<VolunteerTask>
                {
                    new VolunteerTask
                    {
                        Id = 100,
                        Title = "Existing Task",
                        Start = new DateTime(2025, 8, 10, 10, 0, 0),
                        End = new DateTime(2025, 8, 10, 12, 0, 0)
                    }
                }
            };

            var overlappingTask = new VolunteerTask
            {
                Id = 200,
                Title = "New Overlapping Task",
                Start = new DateTime(2025, 8, 10, 11, 0, 0),
                End = new DateTime(2025, 8, 10, 13, 0, 0),
                NumberOfAvailableSlots = 1
            };

            db.Parents.Add(parent);
            db.VolunteerTasks.Add(overlappingTask);
            await db.SaveChangesAsync();

            // Act
            var repo = new TaskRepository(db);
            var result = await repo.TryClaimTaskAsync(overlappingTask.Id, parent.ParentId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ClaimTaskStatus.OverlappingTask, result.Status);
        }

        [Fact]
        public async Task TryClaimTaskAsync_Should_Succeed_When_No_Overlap()
        {
            // Arrange
            var db = GetDbContext();

            var parent = new Parent
            {
                ParentId = 1,
                Name = "John",
                ClaimedTasks = new List<VolunteerTask>
                {
                    new VolunteerTask
                    {
                        Id = 100,
                        Title = "Existing Task",
                        Start = new DateTime(2025, 8, 10, 10, 0, 0),
                        End = new DateTime(2025, 8, 10, 12, 0, 0)
                    }
                }
            };

            var nonOverlappingTask = new VolunteerTask
            {
                Id = 200,
                Title = "Non-overlapping Task",
                Start = new DateTime(2025, 8, 10, 13, 0, 0),
                End = new DateTime(2025, 8, 10, 15, 0, 0),
                NumberOfAvailableSlots = 1
            };

            db.Parents.Add(parent);
            db.VolunteerTasks.Add(nonOverlappingTask);
            await db.SaveChangesAsync();

            // Act
            var repo = new TaskRepository(db);
            var result = await repo.TryClaimTaskAsync(nonOverlappingTask.Id, parent.ParentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ClaimTaskStatus.Success, result.Status);
            
        }

        private AppDbContext CreateDbContext(SqliteConnection? connection = null)
        {
            // Use a shared in-memory connection for concurrency across multiple contexts
            var conn = connection ?? new SqliteConnection("DataSource=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(conn)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        [Fact]
        public async Task TryClaimTaskAsync_Should_Handle_30_Concurrent_Claims()
        {
            // Shared connection so that all contexts talk to the same in-memory DB
            var sharedConnection = new SqliteConnection("DataSource=:memory:");
            sharedConnection.Open();

            // Seed the database
            using (var seedContext = CreateDbContext(sharedConnection))
            {
                var task = new VolunteerTask
                {
                    Id = 1,
                    Title = "Sample Task",
                    NumberOfAvailableSlots = 5,
                    Start = DateTime.UtcNow,
                    End = DateTime.UtcNow.AddHours(1),
                    ParticipatingParents = new List<int>()
                };

                var parents = Enumerable.Range(1, 30)
                    .Select(i => new Parent { ParentId = i , Name = $"Parent{i}"})
                    .ToList();

                seedContext.VolunteerTasks.Add(task);
                seedContext.Parents.AddRange(parents);

                await seedContext.SaveChangesAsync();
            }

            // Run 30 concurrent claims
            var claimTasks = Enumerable.Range(1, 30)
                .Select(async i =>
                {
                    using var context = CreateDbContext(sharedConnection);
                    var repo = new TaskRepository(context);
                    return await repo.TryClaimTaskAsync(1, i);
                });

            var results = await Task.WhenAll(claimTasks);

            // Assert
            var successCount = results.Count(r => r.Status == ClaimTaskStatus.Success);
            var bookedCount = results.Count(r => r.Status == ClaimTaskStatus.TaskFullyBooked);

            Assert.Equal(5, successCount);
            Assert.Equal(25, bookedCount);
        }


        #endregion

        #region CancelTaskForParentAsync

        [Fact]
        public async Task CancelTaskForParentAsync_ShouldRemoveParent()
        {
            var db = GetDbContext();
            var task = new VolunteerTask {Id = 1, Title = "Cleaner", ParticipatingParents = new List<int> { 5 } };
            var parent = new Parent { ParentId = 5, Name = "Mrs. Kim" };
            db.VolunteerTasks.Add(task);
            db.Parents.Add(parent);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);
            var result = await repo.CancelTaskForParentAsync(1, 5);

            Assert.True(result);
            Assert.Empty(task.ParticipatingParents);
        }

        [Fact]
        public async Task CancelTaskForParentAsync_ShouldThrow_WhenParentNotFound()
        {
            var db = GetDbContext();
            var repo = new TaskRepository(db);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.CancelTaskForParentAsync(1, 5));
        }

        [Fact]
        public async Task CancelTaskForParentAsync_ShouldThrow_WhenTaskNotFound()
        {
            var db = GetDbContext();
            var parent = new Parent { ParentId = 5, Name = "Mrs. Kim" };
            db.Parents.Add(parent);
            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.CancelTaskForParentAsync(1, 5));
        }

        [Fact]
        public async Task CancelTaskForParentAsync_ShouldThrow_WhenParentNotParticipatingInTask()
        {
            // Arrange
            var db = GetDbContext();

            // Create a parent
            var parent = new Parent { ParentId = 5, Name = "Mrs. Kim" };
            db.Parents.Add(parent);

            // Create a task but don't add the parent to the ParticipatingParents collection
            var task = new VolunteerTask
            {
                Id = 1,
                Title = "Sample Task",
                ParticipatingParents = new List<int>()
            };
            db.VolunteerTasks.Add(task);

            await db.SaveChangesAsync();

            var repo = new TaskRepository(db);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.CancelTaskForParentAsync(1, 5));
        }

        #endregion
    }
}

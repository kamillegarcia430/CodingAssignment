using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Infrastructure.Data;
using VolunteerScheduler.Infrastructure.Repositories;

namespace VolunteerScheduler.Infrastructure.Tests.Repositories
{
    public class TeacherRepositoryTests
    {
        private AppDbContext CreateDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task AddAsync_ShouldAddTeacher()
        {
            // Arrange
            var context = CreateDbContext(nameof(AddAsync_ShouldAddTeacher));
            var repo = new TeacherRepository(context);
            var teacher = new Teacher { TeacherId = 1, Name = "Mr. Smith" };

            // Act
            await repo.AddAsync(teacher, CancellationToken.None);
            var teachers = await context.Teachers.ToListAsync();

            // Assert
            Assert.Single(teachers);
            Assert.Equal("Mr. Smith", teachers[0].Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnTeacherWithTasks()
        {
            // Arrange
            var context = CreateDbContext(nameof(GetByIdAsync_ShouldReturnTeacherWithTasks));
            var teacher = new Teacher
            {
                TeacherId = 1,
                Name = "Ms. Johnson",
                CreatedTasks = new List<VolunteerTask>
            {
                new VolunteerTask { Id = 101, Title = "Math Tutoring" }
            }
            };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var repo = new TeacherRepository(context);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Ms. Johnson", result!.Name);
            Assert.Single(result.CreatedTasks);
            Assert.Equal("Math Tutoring", result.CreatedTasks.First().Title);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyTeacher()
        {
            // Arrange
            var context = CreateDbContext(nameof(UpdateAsync_ShouldModifyTeacher));
            var teacher = new Teacher { TeacherId = 1, Name = "Old Name" };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var repo = new TeacherRepository(context);

            // Act
            teacher.Name = "New Name";
            await repo.UpdateAsync(teacher, CancellationToken.None);

            // Assert
            var updatedTeacher = await context.Teachers.FindAsync(1);
            Assert.Equal("New Name", updatedTeacher!.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveTeacher()
        {
            // Arrange
            var context = CreateDbContext(nameof(DeleteAsync_ShouldRemoveTeacher));
            var teacher = new Teacher { TeacherId = 1, Name = "To Delete" };
            context.Teachers.Add(teacher);
            await context.SaveChangesAsync();

            var repo = new TeacherRepository(context);

            // Act
            await repo.DeleteAsync(teacher, CancellationToken.None);

            // Assert
            Assert.Empty(context.Teachers);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllTeachersWithTasks()
        {
            // Arrange
            var context = CreateDbContext(nameof(GetAllAsync_ShouldReturnAllTeachersWithTasks));
            context.Teachers.AddRange(
                new Teacher
                {
                    TeacherId = 1,
                    Name = "Teacher A",
                    CreatedTasks = new List<VolunteerTask>
                    {
                    new VolunteerTask { Id = 201, Title = "Science Fair" }
                    }
                },
                new Teacher
                {
                    TeacherId = 2,
                    Name = "Teacher B",
                    CreatedTasks = new List<VolunteerTask>()
                }
            );
            await context.SaveChangesAsync();

            var repo = new TeacherRepository(context);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.CreatedTasks.Any(ct => ct.Title == "Science Fair"));
        }
    }
}

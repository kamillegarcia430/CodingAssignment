using System.Data;
using System.Threading.Tasks;
using System.Threading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Domain.Results;
using VolunteerScheduler.Infrastructure.Data;

namespace VolunteerScheduler.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(VolunteerTask task, CancellationToken cancellationToken)
        {
            // Attach the new task
            _context.VolunteerTasks.Add(task);

            // Find the teacher who created the task
            var teacher = await _context.Teachers
                .Include(t => t.CreatedTasks)
                .FirstOrDefaultAsync(t => t.TeacherId == task.CreatedByTeacherId, cancellationToken);


            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {task.CreatedByTeacherId} not found.");

            // Save task first so it gets an ID
            await _context.SaveChangesAsync(cancellationToken);

            // Add the task to teacher's claimed tasks
            teacher.CreatedTasks.Add(task);

            // Save teacher update
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<VolunteerTask?> GetByIdAsync(int taskId)
        {
            return await _context.VolunteerTasks
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task DeleteAsync(VolunteerTask task, CancellationToken cancellationToken)
        {
            _context.VolunteerTasks.Remove(task);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<VolunteerTask>> GetAvailableTasksAsync()
        {
            return await _context.VolunteerTasks.Where(s => s.NumberOfAvailableSlots > 0).ToListAsync();
        }

        public async Task<List<VolunteerTask>> GetAllTasksAsync()
        {
            return await _context.VolunteerTasks.ToListAsync();
        }

        public async Task<List<VolunteerTask>> GetParentTasksAsync(int parentId)
        {
            var teacher = await _context.Parents.FirstOrDefaultAsync(t => t.ParentId == parentId);

            if (teacher == null)
                throw new KeyNotFoundException($"Parent with ID {parentId} not found.");

            return await _context.VolunteerTasks
                .Where(task => task.ParticipatingParents.Any(p => p == parentId))
                .ToListAsync();
        }

        public async Task<List<VolunteerTask>> GetTasksCreatedByTeacherAsync(int teacherId)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.TeacherId == teacherId);

            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {teacherId} not found.");

            return await _context.VolunteerTasks
                .Where(task => task.CreatedByTeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<ClaimTaskResult> TryClaimTaskAsync(int taskId, int parentId)
        {
            // Each call creates its own transaction so that concurrency works properly
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                IQueryable<VolunteerTask> taskQuery;

                // Detect if we're on Postgres and apply row lock if so
                if (_context.Database.IsNpgsql())
                {
                    taskQuery = _context.VolunteerTasks
                        .FromSqlRaw(@"SELECT * FROM ""VolunteerTasks"" WHERE ""Id"" = {0} FOR UPDATE", taskId);
                }
                else
                {
                    // InMemory / other provider — no raw SQL
                    taskQuery = _context.VolunteerTasks.Where(t => t.Id == taskId);
                }

                var task = await taskQuery.FirstOrDefaultAsync();

                if (task == null)
                    return ClaimTaskResult.Failure(ClaimTaskStatus.TaskNotFound, "Task not found.");

                if (task.NumberOfAvailableSlots <= 0)
                    return ClaimTaskResult.Failure(ClaimTaskStatus.TaskFullyBooked, "Task is fully booked.");

                if (task.ParticipatingParents.Contains(parentId))
                    return ClaimTaskResult.Failure(ClaimTaskStatus.AlreadyClaimed, "You have already claimed this task.");

                var parent = await _context.Parents
                    .Include(p => p.ClaimedTasks)
                    .FirstOrDefaultAsync(p => p.ParentId == parentId);

                if (parent == null)
                    return ClaimTaskResult.Failure(ClaimTaskStatus.ParentNotFound, "Parent not found.");

                bool hasOverlap = parent.ClaimedTasks.Any(existingTask =>
                    existingTask.Start < task.End && task.Start < existingTask.End);

                if (hasOverlap)
                {
                    return ClaimTaskResult.Failure(
                        ClaimTaskStatus.OverlappingTask,
                        "You have already claimed another task that overlaps with this time."
                    );
                }

                // Apply claim
                parent.ClaimedTasks.Add(task);
                task.ParticipatingParents.Add(parentId);
                task.NumberOfAvailableSlots--;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ClaimTaskResult.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                return ClaimTaskResult.Failure(ClaimTaskStatus.TaskFullyBooked, "Task is fully booked.");
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg &&
                (pg.SqlState == "23505" || pg.SqlState == "40001"))
            {
                await transaction.RollbackAsync();
                return ClaimTaskResult.Failure(ClaimTaskStatus.TaskFullyBooked, "Task is fully booked.");
            }
            catch (PostgresException pg) when (pg.SqlState == "23505" || pg.SqlState == "40001")
            {
                await transaction.RollbackAsync();
                return ClaimTaskResult.Failure(ClaimTaskStatus.TaskFullyBooked, "Task is fully booked.");
            }
            catch
            {
                await transaction.RollbackAsync();
                return ClaimTaskResult.Failure(ClaimTaskStatus.Error, "An unexpected error occurred.");
            }
        }

        public async Task UpdateAsync(VolunteerTask task, CancellationToken cancellationToken)
        {
            _context.VolunteerTasks.Update(task);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> CancelTaskForParentAsync(int taskId, int parentId)
        {
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.ParentId == parentId);

            if (parent == null)
                throw new KeyNotFoundException("Parent not found");

            var task = await _context.VolunteerTasks
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) 
                throw new KeyNotFoundException($"Task with ID {taskId} does not exist.");

            if (!task.ParticipatingParents.Contains(parentId))
            {
                throw new InvalidOperationException($"Parent with ID {parentId} is not participating in task with ID {taskId}.");
            }

            task.ParticipatingParents.Remove(parentId);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}

using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Domain.Results;

namespace VolunteerScheduler.Application.Interfaces
{
    public interface ITaskRepository
    {
        Task AddAsync(VolunteerTask task, CancellationToken cancellationToken);
        Task<List<VolunteerTask>> GetAvailableTasksAsync();
        Task<List<VolunteerTask>> GetAllTasksAsync();
        Task<ClaimTaskResult> TryClaimTaskAsync(int taskId, int parentId);
        Task<List<VolunteerTask>> GetParentTasksAsync(int parentId);
        Task<List<VolunteerTask>> GetTasksCreatedByTeacherAsync(int teacherId);
        Task<VolunteerTask?> GetByIdAsync(int taskId);
        Task DeleteAsync(VolunteerTask task, CancellationToken cancellationToken);
        Task UpdateAsync(VolunteerTask task, CancellationToken cancellationToken);
        Task<bool> CancelTaskForParentAsync(int taskId, int parentId);
    }
}

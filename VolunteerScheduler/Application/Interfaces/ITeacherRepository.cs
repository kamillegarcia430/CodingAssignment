using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Interfaces
{
    public interface ITeacherRepository
    {
        Task AddAsync(Teacher teacher, CancellationToken cancellationToken);
        Task<Teacher?> GetByIdAsync(int id);
        Task UpdateAsync(Teacher teacher, CancellationToken cancellationToken);
        Task DeleteAsync(Teacher teacher, CancellationToken cancellationToken);
        Task<List<Teacher>> GetAllAsync();
    }
}

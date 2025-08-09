using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Interfaces
{
    public interface IParentRepository
    {
        Task<List<Parent>> GetAllAsync();
        Task<Parent?> GetByIdAsync(int id);
        Task AddAsync(Parent parent);
        Task UpdateAsync(Parent parent);
        Task DeleteAsync(Parent parent);
    }
}

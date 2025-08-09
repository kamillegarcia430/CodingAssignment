// ParentRepository.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Infrastructure.Data;

namespace VolunteerScheduler.Infrastructure.Repositories
{
    public class ParentRepository : IParentRepository
    {
        private readonly AppDbContext _context;

        public ParentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Parent>> GetAllAsync()
        {
            return await _context.Parents
                .Include(p => p.ClaimedTasks)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Parent?> GetByIdAsync(int id)
        {
            return await _context.Parents.Include(p => p.ClaimedTasks).FirstOrDefaultAsync(p => p.ParentId == id);
        }

        public async Task AddAsync(Parent parent)
        {
            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Parent parent)
        {
            _context.Parents.Update(parent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Parent parent)
        {
            // Load the parent with claimed tasks
            var trackedParent = await _context.Parents
                .Include(p => p.ClaimedTasks)
                .FirstOrDefaultAsync(p => p.ParentId == parent.ParentId);

            if (trackedParent == null)
                throw new KeyNotFoundException($"Parent with ID {parent.ParentId} does not exist.");

            // Remove parent from each task's ParticipatingParents
            foreach (var task in trackedParent.ClaimedTasks.ToList())
            {
                task.ParticipatingParents.Remove(trackedParent.ParentId);
            }

            _context.Parents.Remove(trackedParent);
            await _context.SaveChangesAsync();
        }
    }
}

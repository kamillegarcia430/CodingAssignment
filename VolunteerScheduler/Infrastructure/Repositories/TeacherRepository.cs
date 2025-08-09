using Microsoft.EntityFrameworkCore;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;
using VolunteerScheduler.Infrastructure.Data;

namespace VolunteerScheduler.Infrastructure.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _context;

        public TeacherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Teacher teacher, CancellationToken cancellationToken)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Teacher?> GetByIdAsync(int id)
        {
            return await _context.Teachers
                .Include(t => t.CreatedTasks)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TeacherId == id);
        }

        public async Task UpdateAsync(Teacher teacher, CancellationToken cancellationToken)
        {
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Teacher teacher, CancellationToken cancellationToken)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Teacher>> GetAllAsync()
        {
            return await _context.Teachers
                .Include(t => t.CreatedTasks)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Commands.TeacherCommandHandlers
{
    public record DeleteTeacherCommand(int TeacherId) : IRequest<bool>;
    public class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, bool>
    {
        private readonly ITeacherRepository _repo;

        public DeleteTeacherCommandHandler(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacher = await _repo.GetByIdAsync(request.TeacherId);
            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} does not exist.");
            if (teacher.CreatedTasks != null && teacher.CreatedTasks.Count > 0)
                throw new ArgumentException("Teacher still has ongoing tasks");

            await _repo.DeleteAsync(teacher, cancellationToken);
            return true;
        }
    }
}

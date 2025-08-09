using MediatR;
using VolunteerScheduler.Application.Interfaces;

namespace VolunteerScheduler.Application.Commands.TeacherCommandHandlers
{
    public record UpdateTeacherCommand(int TeacherId, string Name) : IRequest<bool>;
    public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, bool>
    {
        private readonly ITeacherRepository _repo;

        public UpdateTeacherCommandHandler(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacher = await _repo.GetByIdAsync(request.TeacherId);
            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} does not exist.");

            teacher.Name = request.Name;
            await _repo.UpdateAsync(teacher, cancellationToken);
            return true;
        }
    }
}

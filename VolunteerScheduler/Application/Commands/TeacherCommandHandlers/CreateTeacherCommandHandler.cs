using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Commands.TeacherCommandHandlers
{
    public record CreateTeacherCommand(string Name) : IRequest<int>;
    public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, int>
    {
        private readonly ITeacherRepository _repo;

        public CreateTeacherCommandHandler(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
        {
            var teacher = new Teacher { Name = request.Name };
            await _repo.AddAsync(teacher, cancellationToken);
            return teacher.TeacherId;
        }
    }
}

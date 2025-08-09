using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.TeacherQueries
{
    public record GetTeacherByIdQuery(int Id) : IRequest<Teacher?>;

    public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, Teacher?>
    {
        private readonly ITeacherRepository _repo;

        public GetTeacherByIdQueryHandler(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public async Task<Teacher?> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
        {
            var teacherResp = await _repo.GetByIdAsync(request.Id);
            if (teacherResp == null)
            {
                throw new KeyNotFoundException("Teacher not found.");
            }
            return new Teacher
            {
                Name = teacherResp.Name,
                TeacherId = teacherResp?.TeacherId ?? 0,
                CreatedTasks = teacherResp?.CreatedTasks ?? new List<VolunteerTask>(),
            };
        }
    }
}

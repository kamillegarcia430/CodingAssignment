using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.TeacherQueries
{
    public record GetAllTeachersQuery() : IRequest<List<Teacher>>;
    public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, List<Teacher>>
    {
        private readonly ITeacherRepository _repo;

        public GetAllTeachersQueryHandler(ITeacherRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Teacher>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetAllAsync();
        }
    }
}

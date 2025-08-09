using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.TasksQueries
{
    public record GetAllTasksQuery : IRequest<List<VolunteerTask>>;

    public class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, List<VolunteerTask>>
    {
        private readonly ITaskRepository _repo;

        public GetAllTasksQueryHandler(ITaskRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<VolunteerTask>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetAllTasksAsync();
        }
    }
}

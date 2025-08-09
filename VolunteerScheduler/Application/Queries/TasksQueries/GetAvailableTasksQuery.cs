using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.TasksQueries
{
    public record GetAvailableTasksQuery : IRequest<List<VolunteerTask>>;

    public class GetAvailableTasksQueryHandler : IRequestHandler<GetAvailableTasksQuery, List<VolunteerTask>>
    {
        private readonly ITaskRepository _repo;

        public GetAvailableTasksQueryHandler(ITaskRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<VolunteerTask>> Handle(GetAvailableTasksQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetAvailableTasksAsync();
        }
    }
}

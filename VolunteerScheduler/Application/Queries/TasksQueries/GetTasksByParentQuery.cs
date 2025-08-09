using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.TasksQueries
{
    public record GetTasksByParentQuery(int ParentId) : IRequest<List<VolunteerTask>>;

    public class GetTasksByParentQueryHandler : IRequestHandler<GetTasksByParentQuery, List<VolunteerTask>>
    {
        private readonly ITaskRepository _repo;

        public GetTasksByParentQueryHandler(ITaskRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<VolunteerTask>> Handle(GetTasksByParentQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _repo.GetParentTasksAsync(request.ParentId);

            return tasks.Select(t => new VolunteerTask
            {
                Id = t.Id,
                Title = t.Title,
                Start = t.Start,
                End = t.End,
                NumberOfAvailableSlots = t.NumberOfAvailableSlots,
                CreatedByTeacherId = t.CreatedByTeacherId,
                ParticipatingParents = t.ParticipatingParents.Select(p => p).ToList(),
            }).ToList();
        }
    }
}

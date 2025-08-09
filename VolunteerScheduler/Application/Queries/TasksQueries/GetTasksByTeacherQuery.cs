using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.TasksQueries
{
    public record GetTasksByTeacherQuery(int TeacherId) : IRequest<List<VolunteerTask>>;

    public class GetTasksByTeacherQueryHandler : IRequestHandler<GetTasksByTeacherQuery, List<VolunteerTask>>
    {
        private readonly ITaskRepository _repo;

        public GetTasksByTeacherQueryHandler(ITaskRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<VolunteerTask>> Handle(GetTasksByTeacherQuery request, CancellationToken cancellationToken)
        {
            var tasks = await _repo.GetTasksCreatedByTeacherAsync(request.TeacherId);

            return tasks.Select(t => new VolunteerTask
            {
                Id = t.Id,
                Title = t.Title,
                Start = t.Start,
                End = t.End,
                NumberOfAvailableSlots = t.NumberOfAvailableSlots,
                CreatedByTeacherId = t.CreatedByTeacherId,
                ParticipatingParents = t.ParticipatingParents.Select(p => p).ToList()
            }).ToList();
        }
    }
}

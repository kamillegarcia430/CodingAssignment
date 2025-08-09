using MediatR;
using VolunteerScheduler.Application.Interfaces;

namespace VolunteerScheduler.Application.Commands.TaskCommandHandler
{
    public record CancelTaskForParentCommand(int TaskId, int ParentId) : IRequest<bool>;
    public class CancelTaskForParentCommandHandler : IRequestHandler<CancelTaskForParentCommand, bool>
    {
        private readonly ITaskRepository _taskRepository;

        public CancelTaskForParentCommandHandler(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<bool> Handle(CancelTaskForParentCommand request, CancellationToken cancellationToken)
        {
            return await _taskRepository.CancelTaskForParentAsync(request.TaskId, request.ParentId);
        }
    }
}

using MediatR;
using VolunteerScheduler.Application.Interfaces;

namespace VolunteerScheduler.Application.Commands.TaskCommandHandler
{
    public record DeleteTaskCommand(int TaskId, int RequestedByTeacherId) : IRequest<bool>;
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, bool>
    {
        private readonly ITaskRepository _taskRepo;

        public DeleteTaskCommandHandler(ITaskRepository taskRepo)
        {
            _taskRepo = taskRepo;
        }

        public async Task<bool> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _taskRepo.GetByIdAsync(request.TaskId);

            if (task == null)
                throw new KeyNotFoundException($"Task with ID {request.TaskId} does not exist.");

            //Ownership check
            if (task.CreatedByTeacherId != request.RequestedByTeacherId)
                throw new UnauthorizedAccessException($"You are not authorized to update this task.");

            await _taskRepo.DeleteAsync(task, cancellationToken);
            return true;
        }
    }

}

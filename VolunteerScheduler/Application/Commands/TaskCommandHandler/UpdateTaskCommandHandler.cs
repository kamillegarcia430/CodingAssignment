using MediatR;
using VolunteerScheduler.Application.Interfaces;

namespace VolunteerScheduler.Application.Commands.TaskCommandHandler
{
    public record UpdateTaskCommand(
        int TaskId,
        string Title,
        DateTime Start,
        DateTime End,
        int NumberOfAvailableSlots,
        int RequestedByTeacherId
    ) : IRequest<bool>;

    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, bool>
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ITeacherRepository _teacherRepository;

        public UpdateTaskCommandHandler(ITaskRepository taskRepository, ITeacherRepository teacherRepository)
        {
            _taskRepository = taskRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<bool> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            // Validate teacher exists
            var teacher = await _teacherRepository.GetByIdAsync(request.RequestedByTeacherId);
            if (teacher == null)
                throw new KeyNotFoundException($"Teacher with ID {request.RequestedByTeacherId} does not exist.");

            // Validate task exists
            var task = await _taskRepository.GetByIdAsync(request.TaskId);
            if (task == null)
                throw new KeyNotFoundException($"Task with ID {request.TaskId} does not exist.");

            // Ensure the teacher is the creator of the task
            if (task.CreatedByTeacherId != request.RequestedByTeacherId)
                throw new UnauthorizedAccessException("You are not authorized to update this task.");

            // Validate task time range
            if (request.End <= request.Start)
                throw new ArgumentException("End time must be later than start time.");

            // Update the task
            task.Title = request.Title;
            task.Start = request.Start;
            task.End = request.End;
            task.NumberOfAvailableSlots = request.NumberOfAvailableSlots;

            await _taskRepository.UpdateAsync(task, cancellationToken);
            return true;
        }
    }
}

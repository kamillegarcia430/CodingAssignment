using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Results;

namespace VolunteerScheduler.Application.Commands.TaskCommandHandler
{
    public record ClaimTaskCommand(int TaskId, int ParentId) : IRequest<ClaimTaskResult>;

    public class ClaimTaskCommandHandler : IRequestHandler<ClaimTaskCommand, ClaimTaskResult>
    {
        private readonly ITaskRepository _repo;
        private readonly IParentRepository _parentRepository;

        public ClaimTaskCommandHandler(ITaskRepository repo, IParentRepository parentRepository )
        {
            _repo = repo;
            _parentRepository = parentRepository;
        }

        public async Task<ClaimTaskResult> Handle(ClaimTaskCommand request, CancellationToken cancellationToken)
        {

            return await _repo.TryClaimTaskAsync(request.TaskId, request.ParentId);
        }
    }
}

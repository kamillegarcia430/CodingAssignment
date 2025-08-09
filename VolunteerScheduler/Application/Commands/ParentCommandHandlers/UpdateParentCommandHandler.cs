using MediatR;
using VolunteerScheduler.Application.Interfaces;

namespace VolunteerScheduler.Application.Commands.ParentCommandHandlers
{
    public record UpdateParentCommand(int ParentId, string Name) : IRequest<bool>;
    public class UpdateParentCommandHandler : IRequestHandler<UpdateParentCommand, bool>
    {
        private readonly IParentRepository _repo;

        public UpdateParentCommandHandler(IParentRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(UpdateParentCommand request, CancellationToken cancellationToken)
        {
            var parent = await _repo.GetByIdAsync(request.ParentId);
            if (parent == null)
                throw new KeyNotFoundException($"Parent with ID {request.ParentId} does not exist.");

            // Apply updates
            parent.Name = request.Name;

            // Call repository to persist changes
            await _repo.UpdateAsync(parent);

            return true;
        }
    }
}

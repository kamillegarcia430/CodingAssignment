using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Commands.ParentCommandHandlers
{
    public record DeleteParentCommand(int ParentId) : IRequest<bool>;
    public class DeleteParentCommandHandler : IRequestHandler<DeleteParentCommand, bool>
    {
        private readonly IParentRepository _repo;

        public DeleteParentCommandHandler(IParentRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(DeleteParentCommand request, CancellationToken cancellationToken)
        {
            var parent = await _repo.GetByIdAsync(request.ParentId);
            if (parent == null)
                throw new KeyNotFoundException($"Parent with ID {request.ParentId} does not exist.");

            // Call repository to delete (repository only deletes, no checks)
            await _repo.DeleteAsync(parent);

            return true;
        }

    }
}

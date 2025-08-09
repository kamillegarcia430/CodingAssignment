using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Commands.ParentCommandHandlers
{
    public record CreateParentCommand(string Name) : IRequest<int>;
    public class CreateParentCommandHandler : IRequestHandler<CreateParentCommand, int>
    {
        private readonly IParentRepository _repo;

        public CreateParentCommandHandler(IParentRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> Handle(CreateParentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name cannot be empty");

            var parent = new Parent { Name = request.Name };
            await _repo.AddAsync(parent);
            return parent.ParentId;
        }
    }
}

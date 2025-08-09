using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.ParentQueries
{
    public record GetParentByIdQuery(int Id) : IRequest<Parent?>;

    public class GetParentByIdQueryHandler : IRequestHandler<GetParentByIdQuery, Parent?>
    {
        private readonly IParentRepository _repo;

        public GetParentByIdQueryHandler(IParentRepository repo)
        {
            _repo = repo;
        }

        public async Task<Parent?> Handle(GetParentByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetByIdAsync(request.Id);
        }
    }
}

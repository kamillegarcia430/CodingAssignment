using MediatR;
using VolunteerScheduler.Application.Interfaces;
using VolunteerScheduler.Domain.Entities;

namespace VolunteerScheduler.Application.Queries.ParentQueries
{
    public record GetAllParentsQuery() : IRequest<List<Parent>>;
    public class GetAllParentsQueryHandler : IRequestHandler<GetAllParentsQuery, List<Parent>>
    {
        private readonly IParentRepository _repo;

        public GetAllParentsQueryHandler(IParentRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Parent>> Handle(GetAllParentsQuery request, CancellationToken cancellationToken)
        {
            return await _repo.GetAllAsync();
        }
    }
}

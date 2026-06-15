using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Queries.Localidades
{
    public class GetAllQuery : IRequest<IList<Localidad>>
    {
    }

    public class GetAllHandler : IRequestHandler<GetAllQuery, IList<Localidad>>
    {
        private readonly ILocalidadesRepository _repo;

        public GetAllHandler(ILocalidadesRepository repo)
        {
            _repo = repo;
        }

        public async Task<IList<Localidad>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var results = await _repo.FindAllAsync();
            return results.ToList();
        }
    }
}

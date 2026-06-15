using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Queries.Afiliaciones
{
    public class GetAllQuery : IRequest<IList<Afiliacion>>
    {
        //
    }

    public class GetAllHandler : IRequestHandler<GetAllQuery, IList<Afiliacion>>
    {
        private readonly IAfiliacionesRepository _repo;

        public GetAllHandler(IAfiliacionesRepository repo)
        {
            _repo = repo;
        }


        public async Task<IList<Afiliacion>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var results = await _repo.FindAllAsync();
            return results.ToList();
        }
    }
}

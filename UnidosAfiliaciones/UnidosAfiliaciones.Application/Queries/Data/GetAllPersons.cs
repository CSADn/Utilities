using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Dtos;
using UnidosAfiliaciones.Application.Interfaces.Services;

namespace UnidosAfiliaciones.Application.Queries.Data
{
    public class GetAllPersonsQuery : IRequest<IList<PersonaDataDto>>
    {
        //
    }

    public class GetAllPersonsHandler : IRequestHandler<GetAllPersonsQuery, IList<PersonaDataDto>>
    {
        private readonly IDataService _service;

        public GetAllPersonsHandler(IDataService service)
        {
            _service = service;
        }


        public async Task<IList<PersonaDataDto>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
        {
            var results = await _service.FetchPersons();
            return results.ToList();
        }
    }
}

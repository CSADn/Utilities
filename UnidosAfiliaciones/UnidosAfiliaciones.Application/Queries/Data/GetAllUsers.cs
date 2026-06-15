using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Dtos;
using UnidosAfiliaciones.Application.Interfaces.Services;

namespace UnidosAfiliaciones.Application.Queries.Data
{
    public class GetAllUsersQuery : IRequest<IList<UsuarioDataDto>>
    {
        //
    }

    public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, IList<UsuarioDataDto>>
    {
        private readonly IDataService _service;

        public GetAllUsersHandler(IDataService service)
        {
            _service = service;
        }


        public async Task<IList<UsuarioDataDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var results = await _service.FetchUsers();
            return results.ToList();
        }
    }
}

using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Dtos;
using UnidosAfiliaciones.Application.Interfaces.Services;

namespace UnidosAfiliaciones.Application.Queries.Excel
{
    public class GetAllPersonsQuery : IRequest<IList<PersonaExcelDto>>
    {
        //
    }

    public class GetAllPersonsHandler : IRequestHandler<GetAllPersonsQuery, IList<PersonaExcelDto>>
    {
        private readonly IExcelService _service;

        public GetAllPersonsHandler(IExcelService service)
        {
            _service = service;
        }


        public async Task<IList<PersonaExcelDto>> Handle(GetAllPersonsQuery request, CancellationToken cancellationToken)
        {
            var results = await Task.Run(() => _service.DownloadAndFetch());
            return results.ToList();
        }
    }
}

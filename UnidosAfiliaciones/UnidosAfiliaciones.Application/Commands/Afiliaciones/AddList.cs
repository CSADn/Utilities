using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Commands.Afiliaciones
{
    public class AddListCommand : IRequest
    {
        public IList<Afiliacion> Afiliaciones { get; set; }
        public IList<AfiliacionDatos> AfiliacionesDatos { get; set; }
    }

    public class AddListCommandHandler : IRequestHandler<AddListCommand>
    {
        private readonly IAfiliacionesRepository _afiliaciones;
        private readonly IAfiliacionesDatosRepository _afiliacionesDatos;


        public AddListCommandHandler(IAfiliacionesRepository afiliaciones, IAfiliacionesDatosRepository afiliacionesDatos)
        {
            _afiliaciones = afiliaciones;
            _afiliacionesDatos = afiliacionesDatos;
        }


        public async Task<Unit> Handle(AddListCommand request, CancellationToken cancellationToken)
        {
            if (request.Afiliaciones.Any())
                await _afiliaciones.BulkInsertAsync(request.Afiliaciones);

            if (request.AfiliacionesDatos.Any())
                await _afiliacionesDatos.BulkInsertAsync(request.AfiliacionesDatos);

            return new Unit();
        }
    }
}

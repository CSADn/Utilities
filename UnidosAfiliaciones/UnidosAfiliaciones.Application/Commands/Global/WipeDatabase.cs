using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Interfaces.Repositories;

namespace UnidosAfiliaciones.Application.Commands.Global
{
    public class WipeDatabaseCommand : IRequest
    {
        public bool RemoveUsers { get; set; }
    }

    public class WipeDatabaseCommandHandler : IRequestHandler<WipeDatabaseCommand>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<WipeDatabaseCommandHandler> _log;
        private readonly IAfiliacionesRepository _afiliaciones;
        private readonly IAfiliacionesDatosRepository _afiliacionesDatos;
        private readonly IFotosRepository _fotos;
        private readonly IUsuariosRepository _usuarios;
        private readonly IUsuariosLocalidadesRepository _usuariosLocalidades;


        public WipeDatabaseCommandHandler(
            IConfiguration config,
            ILogger<WipeDatabaseCommandHandler> log,
            IAfiliacionesRepository afiliaciones,
            IAfiliacionesDatosRepository afiliacionesDatos,
            IFotosRepository fotos,
            IUsuariosRepository usuarios,
            IUsuariosLocalidadesRepository usuariosLocalidades)
        {
            _config = config;
            _log = log;
            _afiliaciones = afiliaciones;
            _afiliacionesDatos = afiliacionesDatos;
            _fotos = fotos;
            _usuarios = usuarios;
            _usuariosLocalidades = usuariosLocalidades;
        }


        public async Task<Unit> Handle(WipeDatabaseCommand request, CancellationToken cancellationToken)
        {
            _log.LogInformation($"Deleting table: [AfiliacionesDatos]");
            await _afiliacionesDatos.DeleteAllAsync();

            _log.LogInformation($"Deleting table: [Afiliaciones]");
            await _afiliaciones.DeleteAllAsync();

            _log.LogInformation($"Deleting table: [Fotos]");
            await _fotos.DeleteAllAsync();

            if (request.RemoveUsers)
            {
                _log.LogInformation($"Deleting table: [UsuariosLocalidades]");
                await _usuariosLocalidades.DeleteAllAsync();

                _log.LogInformation($"Deleting table: [Usuarios]");
                await _usuarios.DeleteAllAsync();
            }

            return new Unit();
        }
    }
}

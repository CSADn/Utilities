using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Commands.Usuarios
{
    public class AddListCommand : IRequest
    {
        public IList<Usuario> Users { get; set; }
        public IList<UsuarioLocalidad> UsersLocations { get; set; }
    }

    public class AddListCommandHandler : IRequestHandler<AddListCommand>
    {
        private readonly IUsuariosRepository _usuarios;
        private readonly IUsuariosLocalidadesRepository _usuariosLocalidades;


        public AddListCommandHandler(IUsuariosRepository usuarios, IUsuariosLocalidadesRepository usuariosLocalidades)
        {
            _usuarios = usuarios;
            _usuariosLocalidades = usuariosLocalidades;
        }


        public async Task<Unit> Handle(AddListCommand request, CancellationToken cancellationToken)
        {
            if (request.Users.Any())
                await _usuarios.BulkInsertAsync(request.Users);

            if (request.UsersLocations.Any())
                await _usuariosLocalidades.BulkInsertAsync(request.UsersLocations);

            return new Unit();
        }
    }
}

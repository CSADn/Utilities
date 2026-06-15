using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class EstadosUsuarios : DapperRepositoryBase<EstadoUsuario>, IEstadoUsuarioRepository
    {
        public EstadosUsuarios(IDbConnectionFactory factory, ISqlGenerator<EstadoUsuario> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Usuarios : DapperRepositoryBase<Usuario>, IUsuariosRepository
    {
        public Usuarios(IDbConnectionFactory factory, ISqlGenerator<Usuario> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

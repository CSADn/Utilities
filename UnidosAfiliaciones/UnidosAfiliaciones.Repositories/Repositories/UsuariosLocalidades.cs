using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class UsuariosLocalidades : DapperRepositoryBase<UsuarioLocalidad>, IUsuariosLocalidadesRepository
    {
        public UsuariosLocalidades(IDbConnectionFactory factory, ISqlGenerator<UsuarioLocalidad> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

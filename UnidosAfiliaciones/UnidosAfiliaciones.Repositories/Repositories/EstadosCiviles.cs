using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class EstadosCiviles : DapperRepositoryBase<EstadoCivil>, IEstadosCivilesRepository
    {
        public EstadosCiviles(IDbConnectionFactory factory, ISqlGenerator<EstadoCivil> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

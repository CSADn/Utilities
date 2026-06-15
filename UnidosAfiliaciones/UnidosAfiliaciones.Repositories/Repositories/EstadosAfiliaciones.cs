using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class EstadosAfiliaciones : DapperRepositoryBase<EstadoAfiliacion>, IEstadosAfiliacionesRepository
    {
        public EstadosAfiliaciones(IDbConnectionFactory factory, ISqlGenerator<EstadoAfiliacion> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

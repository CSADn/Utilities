using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Afiliaciones : DapperRepositoryBase<Afiliacion>, IAfiliacionesRepository
    {
        public Afiliaciones(IDbConnectionFactory factory, ISqlGenerator<Afiliacion> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

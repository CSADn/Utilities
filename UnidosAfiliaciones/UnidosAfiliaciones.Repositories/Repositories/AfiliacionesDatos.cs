using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class AfiliacionesDatos : DapperRepositoryBase<AfiliacionDatos>, IAfiliacionesDatosRepository
    {
        public AfiliacionesDatos(IDbConnectionFactory factory, ISqlGenerator<AfiliacionDatos> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Provincias : DapperRepositoryBase<Provincia>, IProvinciasRepository
    {
        public Provincias(IDbConnectionFactory factory, ISqlGenerator<Provincia> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

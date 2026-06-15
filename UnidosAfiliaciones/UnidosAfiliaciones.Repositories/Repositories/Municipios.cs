using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Municipios : DapperRepositoryBase<Municipio>, IMunicipiosRepository
    {
        public Municipios(IDbConnectionFactory factory, ISqlGenerator<Municipio> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

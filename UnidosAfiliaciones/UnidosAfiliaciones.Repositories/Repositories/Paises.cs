using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Paises : DapperRepositoryBase<Pais>, IPaisesRepository
    {
        public Paises(IDbConnectionFactory factory, ISqlGenerator<Pais> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

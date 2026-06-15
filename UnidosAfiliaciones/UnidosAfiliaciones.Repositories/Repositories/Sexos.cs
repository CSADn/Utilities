using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Sexos : DapperRepositoryBase<Sexo>, ISexosRepository
    {
        public Sexos(IDbConnectionFactory factory, ISqlGenerator<Sexo> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

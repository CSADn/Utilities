using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Departamentos : DapperRepositoryBase<Departamento>, IDepartamentosRepository
    {
        public Departamentos(IDbConnectionFactory factory, ISqlGenerator<Departamento> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

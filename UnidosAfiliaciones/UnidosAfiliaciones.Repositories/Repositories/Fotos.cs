using MicroOrm.Dapper.Repositories.SqlGenerator;
using UnidosAfiliaciones.Application.Interfaces.Repositories;
using UnidosAfiliaciones.Entities;
using UnidosAfiliaciones.Repositories.Factories;

namespace UnidosAfiliaciones.Repositories
{
    public class Fotos : DapperRepositoryBase<Foto>, IFotosRepository
    {
        public Fotos(IDbConnectionFactory factory, ISqlGenerator<Foto> sqlGenerator) : base(factory, sqlGenerator)
        {
        }
    }
}

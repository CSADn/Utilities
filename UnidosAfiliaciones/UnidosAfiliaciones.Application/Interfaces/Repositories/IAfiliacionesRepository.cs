using MicroOrm.Dapper.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IAfiliacionesRepository : IDapperRepositoryBase, IDapperRepository<Afiliacion>
    {
    }
}
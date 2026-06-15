using MicroOrm.Dapper.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IEstadosAfiliacionesRepository : IDapperRepositoryBase, IDapperRepository<EstadoAfiliacion>
    {
    }
}
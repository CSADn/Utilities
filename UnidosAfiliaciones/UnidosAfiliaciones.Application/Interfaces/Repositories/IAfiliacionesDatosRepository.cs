using MicroOrm.Dapper.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IAfiliacionesDatosRepository : IDapperRepositoryBase, IDapperRepository<AfiliacionDatos>
    {
    }
}
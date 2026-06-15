using MicroOrm.Dapper.Repositories;
using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Interfaces.Repositories
{
    public interface IUsuariosRepository : IDapperRepositoryBase, IDapperRepository<Usuario>
    {
    }
}